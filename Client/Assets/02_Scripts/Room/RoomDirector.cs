using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;

public class RoomDirector : MonoBehaviour
{
    [SerializeField] Text textReadyCnt;
    [SerializeField] Text textUserCnt;
    [SerializeField] Text textRoomName;
    [SerializeField] Button btnLeave;
    [SerializeField] TargetCameraController targetCameraController;

    #region キャラクター関係
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();  // ユーザーのキャラクター情報
    #endregion

    const float waitSeconds = 0.1f;

    private async void Start()
    {
        textRoomName.text = RoomModel.Instance.ConnectionRoomName;

        // 関数を登録する
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnReadyUser += this.NotifyReadyUser;

        // 接続処理
        await RoomModel.Instance.ConnectAsync();
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

    /// <summary>
    /// 入室リクエスト
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom()
    {
        // 入室処理[ルーム名,ユーザーID(最終的にはローカルに保存してあるユーザーID)]
        await RoomModel.Instance.JoinAsync(RoomModel.Instance.ConnectionRoomName, RoomModel.Instance.MyUserData.Id);
    }

    /// <summary>
    /// 入室通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        // キャラクター生成,
        GameObject character = Instantiate(characterPrefab);
        characterList[user.ConnectionId] = character;

        // プレイヤーの初期化処理
        bool isMyCharacter = user.ConnectionId == RoomModel.Instance.ConnectionId;
        Debug.Log(user.JoinOrder);
        character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.JoinOrder - 1].position);

        // ユーザー名の初期化処理
        Color colorText = isMyCharacter ? Color.white : Color.green;
        character.GetComponent<PlayerUIController>().InitUI(user.UserData.Name, colorText);

        // 自分ではない場合はPlayerControllerを外す , レイヤータグを変更
        character.GetComponent<PlayerController>().enabled = isMyCharacter;
        character.layer = isMyCharacter ? 3 : 7;

        if (isMyCharacter)
        {
            // 自分のモデルにカメラのターゲットを設定
            targetCameraController.InitCamera(character.transform,0);

            // ロード画面を閉じる
            SceneControler.Instance.StopSceneLoad();

            StartCoroutine(UpdateCoroutine());
        }

        int minRequiredUsers = characterList.Count < 2 ? 2 : characterList.Count;
        textUserCnt.text = "/" + minRequiredUsers + " Ready";
    }

    /// <summary>
    /// 退室リクエスト
    /// </summary>
    public async void LeaveRoom()
    {
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
            // 自分が退出する場合は全て削除
            foreach (var character in characterList.Values)
            {
                Destroy(character);
            }
            characterList.Clear();
        }
        else
        {
            // 該当のキャラクター削除&リストから削除
            Destroy(characterList[connectionId]);
            characterList.Remove(connectionId);
        }
    }

    /// <summary>
    /// プレイヤー情報更新リクエスト
    /// </summary>
    public async void UpdatePlayerState()
    {
        var character = characterList[RoomModel.Instance.ConnectionId];
        PlayerState playerState = new PlayerState()
        {
            position = character.transform.position,
            angle = character.transform.eulerAngles,
            animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
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
        await RoomModel.Instance.OnReadyAsynk(isReady);
    }

    /// <summary>
    /// 準備完了通知
    /// </summary>
    void NotifyReadyUser(int readyCnt, bool isTransitionGameScene)
    {
        textReadyCnt.text = readyCnt.ToString();
        if (isTransitionGameScene)
        {
            StopCoroutine(UpdateCoroutine());
            characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>();
            SceneControler.Instance.StartSceneLoad("GameScene");
        }
    }
}
