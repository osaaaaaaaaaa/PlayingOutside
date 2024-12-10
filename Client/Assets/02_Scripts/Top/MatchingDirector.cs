using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;
using System.Threading.Tasks;

public class MatchingDirector : MonoBehaviour
{
    [SerializeField] QuickGameUIController quickGameUIController;
    Dictionary<Guid, int> userList = new Dictionary<Guid, int>(); // <接続ID,入室順>

    private void Start()
    {
        // 関数を登録する
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
    }

    void OnDisable()
    {
        // シーン遷移時に関数の登録を解除
        RoomModel.Instance.OnJoinedUser -= this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
    }

    /// <summary>
    /// クイックゲームボタンを押した場合
    /// </summary>
    public async void OnQuickGameButtonAsync()
    {
        // 接続処理
        await RoomModel.Instance.ConnectAsync();
        // マッチング処理をリクエスト
        JoinLobby();

        quickGameUIController.OnSelectButton();
    }

    /// <summary>
    /// マッチングリクエスト
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinLobby()
    {
        RoomModel.Instance.IsMatchingRunning = true;
        await RoomModel.Instance.JoinLobbyAsynk(RoomModel.Instance.MyUserData.Id);
    }

    /// <summary>
    /// 入室通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        userList[user.ConnectionId] = user.JoinOrder;

        // ユーザーのUI情報を設定
        quickGameUIController.SetupUserFrame(user.JoinOrder - 1, user.UserData.Name, 1 - 1);
    }

    /// <summary>
    /// 退室リクエスト
    /// </summary>
    public async void LeaveRoom()
    {
        RoomModel.Instance.IsMatchingRunning = false;
        await RoomModel.Instance.LeaveAsync();
        quickGameUIController.OnBackButton();
    }

    /// <summary>
    /// 退室通知処理
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        Debug.Log(connectionId + "が退室");
        // マッチングが完了して、自分のロビー退室通知(マッチング完了通知)が届いた場合
        if (RoomModel.Instance.IsMatchingRunning && connectionId == RoomModel.Instance.ConnectionId) return;

        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            // 自分が退出する場合は全て削除
            quickGameUIController.InitAllUserFrame();
            userList.Clear();
        }
        else
        {
            if (userList.ContainsKey(connectionId))
            {
                // 該当のユーザーのUI削除
                quickGameUIController.InitUserFrame(userList[connectionId] - 1);
            }
        }
    }
}
