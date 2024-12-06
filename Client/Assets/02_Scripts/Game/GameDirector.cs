using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;
using Server.Model.Entity;

public class GameDirector : MonoBehaviour
{
    [SerializeField] AreaController areaController;
    [SerializeField] GameStartCountDown gameStartCountDown;
    [SerializeField] TargetCameraController targetCameraController;
    [SerializeField] SpectatingUI spectatingUI;

    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    public Dictionary<Guid,GameObject> characterList { get; private set; }  = new Dictionary<Guid,GameObject>();  // ユーザーのキャラクター情報

    const float waitSeconds = 0.1f;
    bool isCountDownOver;

    public bool isDebug = false;

    private void Start()
    {
        if (isDebug) return;
        isCountDownOver = false;

        // 関数を登録する
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnCountdownOverUser += this.NotifyStartGame;
        RoomModel.Instance.OnAreaClearedUser += this.NotifyAreaClearedUser;
        RoomModel.Instance.OnReadyNextAreaUser += this.NotifyRedyNextAreaAllUsers;

        // 一時的
        RoomModel.Instance.OnAfterFinalGameUser += this.NotifyAfterFinalGameUser;

        SetupGame();
    }

    void OnDisable()
    {
        // シーン遷移時に関数の登録を解除
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnCountdownOverUser -= this.NotifyStartGame;
        RoomModel.Instance.OnAreaClearedUser -= this.NotifyAreaClearedUser;
        RoomModel.Instance.OnReadyNextAreaUser -= this.NotifyRedyNextAreaAllUsers;

        // 一時的
        RoomModel.Instance.OnAfterFinalGameUser -= this.NotifyAfterFinalGameUser;
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            UpdatePlayerState();
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    void SetupGame()
    {
        GenerateCharacters();

        // ロード画面を閉じる
        SceneControler.Instance.StopSceneLoad();

        // 数秒後にカウントダウンが開始
        gameStartCountDown.CallPlayAnim();
    }

    /// <summary>
    /// キャラクター生成処理
    /// </summary>
    void GenerateCharacters()
    {
        var users = RoomModel.Instance.JoinedUsers;

        foreach (var user in users)
        {
            // キャラクター生成,
            GameObject character = Instantiate(characterPrefab);
            characterList[user.Key] = character;

            // プレイヤーの初期化処理
            bool isMyCharacter = user.Key == RoomModel.Instance.ConnectionId;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.Value.JoinOrder - 1]);

            // ユーザー名の初期化処理
            Color colorText = isMyCharacter ? Color.white : Color.green;
            character.GetComponent<PlayerUIController>().InitUI(user.Value.UserData.Name, colorText);

            // ゲームが開始するまではPlayerControllerを外す
            character.GetComponent<PlayerController>().enabled = false;

            // レイヤータグを変更
            character.layer = isMyCharacter ? 3 : 7;

            if (isMyCharacter)
            {
                // 自分のモデルにカメラのターゲットを設定
                targetCameraController.InitCamera(character.transform, 0, user.Key);
            }
        }
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
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // プレイヤーの存在チェック
        var character = characterList[RoomModel.Instance.ConnectionId];
        if(character.GetComponent<PlayerController>().enabled)
        {
            PlayerState playerState = new PlayerState()
            {
                position = character.transform.position,
                angle = character.transform.eulerAngles,
                animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
                isActiveSelf = character.activeSelf,
            };
            await RoomModel.Instance.UpdatePlayerStateAsync(playerState);
        }
    }

    /// <summary>
    /// プレイヤー情報更新通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedPlayerState(Guid connectionId, PlayerState playerState)
    {
        if (!isCountDownOver) return;

        // プレイヤーの存在チェック
        if (!characterList.ContainsKey(connectionId)) return;

        // 移動・回転・アニメーション処理
        characterList[connectionId].SetActive(playerState.isActiveSelf);
        characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

    /// <summary>
    /// ゲーム開始前のカウントダウン終了リクエスト
    /// </summary>
    public async void OnCountdownOver()
    {
        isCountDownOver = true;
        await RoomModel.Instance.OnCountdownOverAsynk();
    }

    /// <summary>
    /// ゲーム開始通知
    /// </summary>
    void NotifyStartGame()
    {
        // プレイヤーの操作をできるようにする
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        StartCoroutine(UpdateCoroutine());
    }

    /// <summary>
    /// 現在のエリアをクリアした処理をリクエスト
    /// </summary>
    public async void OnAreaCleared()
    {
        await RoomModel.Instance.OnAreaClearedAsynk();
    }

    /// <summary>
    /// 現在のエリアをクリアした通知
    /// </summary>
    void NotifyAreaClearedUser(Guid connectionId,string userName, bool isClearedAllUsers)
    {
        // クリアしたユーザー名を表示する
        Debug.Log(userName + "が突破");

        if (isClearedAllUsers)
        {
            // 全員が現在のエリアをクリアした場合、次のエリアに移動する準備をする
            StartCoroutine(areaController.ReadyNextAreaCoroutine());
            return;
        }

        // カメラのターゲットが自分の場合は処理を終了
        if (targetCameraController.currentTargetId == RoomModel.Instance.ConnectionId) return;
        characterList[connectionId].SetActive(false);   // 非表示になっていない場合があるため

        // 他にカメラのターゲットの切り替え先が存在するかチェック
        bool isTarget = targetCameraController.IsOtherTarget();
        if (targetCameraController.activeTargetCnt == 1) spectatingUI.SetupButton(false);

        // 現在のカメラのターゲットとクリアした人が同一人物かどうか
        if (isTarget && connectionId == targetCameraController.currentTargetId)
        {
            // カメラのターゲットの切り替え先が存在する場合は切り替える
            spectatingUI.OnChangeTargetBtn();
        }
    }

    /// <summary>
    /// 次のエリアに移動する準備が完了リクエスト
    /// </summary>
    public async void OnReadyNextArea(bool isLastArea)
    {
        await RoomModel.Instance.OnReadyNextAreaAsynk(isLastArea);
    }

    /// <summary>
    /// 全員が次のエリアに移動する準備が完了した通知
    /// </summary>
    void NotifyRedyNextAreaAllUsers(float restarningWaitSec)
    {
        var myCharacter = characterList[RoomModel.Instance.ConnectionId];

        // ゲーム再開処理
        StartCoroutine(areaController.RestarningGameCoroutine(myCharacter,restarningWaitSec));
    }

    // 一旦終了処理 #######################################################################################
    /// <summary>
    /// 最後の競技が終了した通知
    /// </summary>
    void NotifyAfterFinalGameUser()
    {
        // 最終結果発表シーンに遷移
        StopCoroutine(UpdateCoroutine());
        SceneControler.Instance.StartSceneLoad("FinalResultsScene");
    }
}
