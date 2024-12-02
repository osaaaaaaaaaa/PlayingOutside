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
    [SerializeField] GameStartCountDown gameStartCountDown;
    [SerializeField] TargetCameraController targetCameraController;

    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    Dictionary<Guid,GameObject> characterList = new Dictionary<Guid,GameObject>();  // ユーザーのキャラクター情報

    const float waitSeconds = 0.1f;

    private void Start()
    {
        // 関数を登録する
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnCountdownOverAllUsers += this.NotifyStartGame;

        SetupGame();
    }

    void OnDisable()
    {
        // シーン遷移時に関数の登録を解除
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnCountdownOverAllUsers -= this.NotifyStartGame;
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
            Debug.Log(user.Value.JoinOrder);
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.Value.JoinOrder - 1].position);

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
                targetCameraController.InitTarget(character.transform);
            }
        }
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
        if(character.activeSelf && character.GetComponent<PlayerController>().enabled)
        {
            PlayerState playerState = new PlayerState()
            {
                position = character.transform.position,
                angle = character.transform.eulerAngles,
                animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
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
        // プレイヤーの存在チェック
        if (!characterList.ContainsKey(connectionId) || characterList[connectionId] == null
            || !characterList[connectionId].activeSelf) return;

        // 移動・回転・アニメーション処理
        characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

    /// <summary>
    /// ゲーム開始前のカウントダウン終了リクエスト
    /// </summary>
    public async void OnCountdownOver()
    {
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
}
