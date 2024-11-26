using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;

public class GameDirector : MonoBehaviour
{
    [SerializeField] InputField userIdField;

    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] RoomModel roomModel;
    Dictionary<Guid,GameObject> characterList = new Dictionary<Guid,GameObject>();  // ユーザーのキャラクター情報

    Coroutine updateCoroutine;
    const float waitSeconds = 0.1f;

    private async void Start()
    {
        //// ユーザーが入室したときにthis.OnJoinedUserメソッドを実行するようにする
        //roomModel.OnJoinedUser += this.NotifyJoinedUser;
        //roomModel.OnLeavedUser += this.NotifyLeavedUser;
        //roomModel.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;

        //// 接続処理
        //await roomModel.ConnectAsync();
    }

    private void Update()
    {
        if (updateCoroutine == null && roomModel.userState == RoomModel.USER_STATE.joined) 
        {
            updateCoroutine = StartCoroutine(UpdateCoroutine());
        }
    }

    IEnumerator UpdateCoroutine()
    {
        while (roomModel.userState == RoomModel.USER_STATE.joined)
        {
            UpdatePlayerState();
            yield return new WaitForSeconds(waitSeconds);
        }

        updateCoroutine = null;
    }

    /// <summary>
    /// 入室リクエスト
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom()
    {
        int id = int.Parse(userIdField.text);

        // 入室処理[ルーム名 = [最終的]入力されたルーム名,ユーザーID = [最終的]はローカルに保存してあるユーザーID]
        await roomModel.JoinAsync("sampleRoom", id);
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
        bool isMyCharacter = user.ConnectionId == roomModel.ConnectionId;
        Debug.Log(user.JoinOrder);
        character.GetComponent<PlayerController>().InitPlayer(this, characterStartPoints[user.JoinOrder - 1].position);

        // ユーザー名の初期化処理
        Color colorText = isMyCharacter ? Color.white : Color.green;
        character.GetComponent<PlayerUIController>().InitUI(user.UserData.Name, colorText);

        // 自分ではない場合はPlayerControllerを外す
        character.GetComponent<PlayerController>().enabled = isMyCharacter;
    }

    /// <summary>
    /// 退室リクエスト
    /// </summary>
    public async void LeaveRoom()
    {
        await roomModel.LeaveAsync();
    }

    /// <summary>
    /// 退室通知処理
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        if (connectionId == roomModel.ConnectionId) 
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
        var character = characterList[roomModel.ConnectionId];
        PlayerState playerState = new PlayerState()
        {
            position = character.transform.position,
            angle = character.transform.eulerAngles,
            animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
        };
        await roomModel.UpdatePlayerStateAsync(playerState);
    }

    /// <summary>
    /// プレイヤー情報更新通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedPlayerState(Guid connectionId,PlayerState playerState)
    {
        if (!characterList.ContainsKey(connectionId)) return;   // プレイヤーの存在チェック

        // 移動・回転・アニメーション処理
        characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].transform.DORotate(playerState.angle,waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

}
