using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;

public class TopSceneDirector : MonoBehaviour
{
    [SerializeField] PrivateMatchUIController privateMatchUIController;

    private void Start()
    {
        // ユーザーが入室したときにthis.OnJoinedUserメソッドを実行するようにする
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        Debug.Log(gameObject.name);
    }

    /// <summary>
    /// 入室リクエスト
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom(string roomName)
    {
        // 入室処理[ルーム名 = [最終的]入力されたルーム名,ユーザーID = [最終的]はローカルに保存してあるユーザーID]
        await RoomModel.Instance.JoinAsync(roomName, RoomModel.Instance.MyUserData.Id);
    }

    /// <summary>
    /// 入室通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        bool isMyData = user.ConnectionId == RoomModel.Instance.ConnectionId;
        privateMatchUIController.SetupUserUI(isMyData, user);
    }

    /// <summary>
    /// 退室リクエスト
    /// </summary>
    public async void LeaveRoom()
    {
        await RoomModel.Instance.LeaveAsync();
    }

    /// <summary>
    /// 退室通知処理
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            // 自分が退室した場合は、ルームのUIを全て閉じる
            privateMatchUIController.InitUI();
        }
        else
        {
            // 該当のユーザーUIを削除
            JoinedUser user = RoomModel.Instance.JoinedUsers[connectionId];
            privateMatchUIController.RemoveUserUI(user.JoinOrder);
        }
    }
}
