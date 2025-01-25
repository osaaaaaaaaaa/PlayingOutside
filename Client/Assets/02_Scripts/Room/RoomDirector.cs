using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class RoomDirector : MonoBehaviour
{
    [SerializeField] Text textReadyCnt;
    [SerializeField] Text textUserCnt;
    [SerializeField] GameObject roomNameObj;
    [SerializeField] Text textRoomName;
    [SerializeField] Button btnLeave;
    [SerializeField] TargetCameraController targetCameraController;
    [SerializeField] CharacterControlUI characterControlUI;

    #region キャラクター関係
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] List<GameObject> characterPrefabList;
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();  // ユーザーのキャラクター情報
    #endregion

    SEController seController;

    #region 自動マッチングのタイムアウト関係
    Coroutine coroutineTimeout;
    DateTime startMatchingTime;
    const float timeoutSec = 5f;
    #endregion

    const float waitSeconds = 0.1f;

    private async void Start()
    {
        if (RoomModel.Instance.IsMatchingRunning) roomNameObj.SetActive(false);
        seController = GetComponent<SEController>();

        textRoomName.text = RoomModel.Instance.ConnectionRoomName;

        // 関数を登録する
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnReadyUser += this.NotifyReadyUser;

        // 接続処理
        if (!RoomModel.Instance.IsMatchingRunning)
        {
            await RoomModel.Instance.ConnectAsync();
        }
        else
        {
            coroutineTimeout = StartCoroutine(TimeOutCoroutine());
        }
        // 入室処理をリクエスト
        JoinRoom();
    }

    void OnDisable()
    {
        // シーン遷移時に関数の登録を解除
        RoomModel.Instance.OnJoinedUser -= this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnReadyUser -= this.NotifyReadyUser;
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            UpdatePlayerState();
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    IEnumerator TimeOutCoroutine()
    {
        bool isTimeOut = false;
        startMatchingTime = DateTime.Now;
        while (!isTimeOut)
        {
            yield return new WaitForSeconds(waitSeconds);
            var currentTime = DateTime.Now;
            if((currentTime - startMatchingTime).TotalSeconds > timeoutSec)
            {
                isTimeOut = true;
            }
        }

        OnTimeOut();
    }

    async void OnTimeOut()
    {
        StopCoroutine(UpdateCoroutine());
        await RoomModel.Instance.LeaveAsync();

        UnityAction errorActoin = CallSceneLoadMethod;
        ErrorUIController.Instance.ShowErrorUI("タイムアウトが発生しました。ルームから退室します。", errorActoin);
    }

    public void CallSceneLoadMethod()
    {
        if (SceneControler.Instance.isLoading) SceneManager.LoadScene("TopScene");
        else SceneControler.Instance.StartSceneLoad("TopScene");
    }

    /// <summary>
    /// 入室リクエスト
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom()
    {
        // 入室処理[ルーム名,ユーザーID(最終的にはローカルに保存してあるユーザーID)]
        await RoomModel.Instance.JoinAsync(RoomModel.Instance.ConnectionRoomName, UserModel.Instance.UserId);
    }

    /// <summary>
    /// 入室通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        if (RoomModel.Instance.IsMatchingRunning) return;
        bool isMyCharacter = user.ConnectionId == RoomModel.Instance.ConnectionId;

        // キャラクター生成,
        GameObject character = Instantiate(characterPrefabList[user.UserData.Character_Id - 1]);
        characterList[user.ConnectionId] = character;
        character.name = user.UserData.Name;

        // プレイヤーの初期化処理
        character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.JoinOrder - 1]);
        character.GetComponent<AudioListener>().enabled = isMyCharacter;

        // ユーザー名の初期化処理
        Color colorText = isMyCharacter ? Color.white : Color.green;
        character.GetComponent<PlayerUIController>().InitUI(user.UserData.Name, colorText);

        // 自分ではない場合はレイヤータグを変更してからPlayerControllerを外す
        character.layer = isMyCharacter ? 3 : 7;
        character.GetComponent<PlayerController>().enabled = isMyCharacter;

        if (isMyCharacter)
        {
            targetCameraController.InitCamera(character.transform,0,user.ConnectionId); // 自分のモデルにカメラのターゲットを設定
            characterControlUI.SetupButtonEvent(character);

            // ロード画面を閉じる
            SceneControler.Instance.StopSceneLoad();

            StartCoroutine(UpdateCoroutine());
        }

        int minRequiredUsers = characterList.Count < 2 ? 2 : characterList.Count;
        textUserCnt.text = "/" + minRequiredUsers + " Ready";

        seController.PlayAudio();
    }

    /// <summary>
    /// 退室リクエスト
    /// </summary>
    public async void LeaveRoom()
    {
        StopCoroutine(UpdateCoroutine());
        await RoomModel.Instance.LeaveAsync();

        SceneControler.Instance.StartSceneLoad("TopScene");
    }

    /// <summary>
    /// 退室通知処理
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            // 自分が退室する場合は全て削除
            foreach (var character in characterList.Values)
            {
                Destroy(character);
            }
            characterList.Clear();
        }
        else
        {
            if (characterList.ContainsKey(connectionId))
            {
                // 該当のキャラクター削除&リストから削除
                DOTween.Kill(characterList[connectionId]);
                Destroy(characterList[connectionId]);
                characterList.Remove(connectionId);
            }
        }
    }

    /// <summary>
    /// プレイヤー情報更新リクエスト
    /// </summary>
    public async void UpdatePlayerState()
    {
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // プレイヤーの存在チェック
        var character = characterList[RoomModel.Instance.ConnectionId];
        PlayerState playerState = new PlayerState()
        {
            position = character.transform.position,
            angle = character.transform.eulerAngles,
            animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
            isActiveSelf = true,
        };
        await RoomModel.Instance.UpdatePlayerStateAsync(playerState);
    }

    /// <summary>
    /// プレイヤー情報更新通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedPlayerState(Guid connectionId, PlayerState playerState)
    {
        if (!characterList.ContainsKey(connectionId)) return;   // プレイヤーの存在チェック

        // 移動・回転・アニメーション処理
        characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

    /// <summary>
    /// 準備できたかどうかのリクエスト
    /// </summary>
    public async void OnReadyCircle(bool isReady)
    {
        btnLeave.interactable = !isReady;   // 準備完了中は退室ボタンを押せないようにする
        await RoomModel.Instance.ReadyAsynk(isReady);
    }

    /// <summary>
    /// 準備完了通知
    /// </summary>
    void NotifyReadyUser(int readyCnt, bool isTransitionGameScene)
    {
        textReadyCnt.text = readyCnt.ToString();
        if (isTransitionGameScene)
        {
            if(coroutineTimeout != null) StopCoroutine(coroutineTimeout);
            StopCoroutine(UpdateCoroutine());
            RoomModel.Instance.IsMatchingRunning = false;

            if (SceneControler.Instance.isLoading) SceneManager.LoadScene("RelayGameScene");
            else SceneControler.Instance.StartSceneLoad("RelayGameScene");
        }
    }
}
