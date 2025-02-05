//*********************************************************
// 自動マッチングシステムを管理するスクリプト
// Author:Rui Enomoto
//*********************************************************
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
    bool isJoinTaskRunnning; // 入室処理中かどうか
    bool isLeaveTaskRunning; // 退室処理中かどうか
    bool isJoindUsersMax;    // ユーザーが集まったかどうか
    bool isReceivedOnMatching;

    private void Start()
    {
        // 関数を登録する
        RoomModel.Instance.OnJoinedLobbyUser += this.NotifyJoinedLobbyUser;
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnmatchingUser += this.NotifyMatching;

        RoomModel.Instance.IsMatchingRunning = false;
        isJoinTaskRunnning = false;
        isLeaveTaskRunning = false;
        isJoindUsersMax = false;
        isReceivedOnMatching = false;
    }

    void OnDisable()
    {
        // シーン遷移時に関数の登録を解除
        RoomModel.Instance.OnJoinedLobbyUser -= this.NotifyJoinedLobbyUser;
        RoomModel.Instance.OnJoinedUser -= this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnmatchingUser -= this.NotifyMatching;
    }

    /// <summary>
    /// クイックゲームボタンを押した場合
    /// </summary>
    public async void OnQuickGameButtonAsync()
    {
        if (isJoinTaskRunnning || isLeaveTaskRunning || RoomModel.Instance.IsMatchingRunning) return;
        isJoinTaskRunnning = true;

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

        await RoomModel.Instance.JoinLobbyAsynk(UserModel.Instance.UserId);
    }

    /// <summary>
    /// マッチング完了通知
    /// </summary>
    void NotifyMatching()
    {
        isReceivedOnMatching = true;
    }

    /// <summary>
    /// ロビー入室時の処理
    /// </summary>
    void NotifyJoinedLobbyUser()
    {
        foreach (var user in RoomModel.Instance.JoinedUsers.Values)
        {
            if(!userList.ContainsKey(user.ConnectionId)) 
                userList[user.ConnectionId] = user.JoinOrder;

            // ユーザーのUI情報を設定
            quickGameUIController.SetupUserFrame(user.JoinOrder - 1, user.UserData.Name, user.UserData.Character_Id - 1);
        }

        if (RoomModel.Instance.JoinedUsers.Count == ConstantManager.userMaxCnt) isJoindUsersMax = true;
        isJoinTaskRunnning = false;
    }

    /// <summary>
    /// 入室通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        if (RoomModel.Instance.JoinedUsers.Count == ConstantManager.userMaxCnt) isJoindUsersMax = true;

        if (!userList.ContainsKey(user.ConnectionId))
            userList[user.ConnectionId] = user.JoinOrder;

        // ユーザーのUI情報を設定
        quickGameUIController.SetupUserFrame(user.JoinOrder - 1, user.UserData.Name, user.UserData.Character_Id - 1);
    }

    /// <summary>
    /// 退室リクエスト(ボタンから処理する)
    /// </summary>
    public async void LeaveRoom()
    {
        if (isJoindUsersMax || isJoinTaskRunnning || isLeaveTaskRunning 
            || !RoomModel.Instance.IsMatchingRunning
            || RoomModel.Instance.JoinedUsers.Count == ConstantManager.userMaxCnt) return;
        isLeaveTaskRunning = true;
        RoomModel.Instance.IsMatchingRunning = false;
        isReceivedOnMatching = false;

        await RoomModel.Instance.LeaveAsync();
        quickGameUIController.OnBackButton();
    }

    /// <summary>
    /// 退室通知処理
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        // マッチングが完了して、自分のロビー退室通知(マッチング完了通知)が届いた場合
        if (RoomModel.Instance.IsMatchingRunning && connectionId == RoomModel.Instance.ConnectionId)
        {
            // 4番目のユーザーが入室した瞬間にシーン遷移するのを阻止するため
            Invoke("CallFuncSceneLoad", 1f);
            return;
        }

        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            // 自分が退出する場合は全て削除
            quickGameUIController.InitAllUserFrame();
            userList.Clear();
            isJoinTaskRunnning = false;
            isLeaveTaskRunning = false;
            isJoindUsersMax = false;
        }
        else
        {
            if (isReceivedOnMatching) return;

            isJoindUsersMax = false;
            if (userList.ContainsKey(connectionId))
            {
                // 該当のユーザーのUI削除
                quickGameUIController.InitUserFrame(userList[connectionId] - 1);
            }
        }
    }

    void CallFuncSceneLoad()
    {
        SceneControler.Instance.StartSceneLoad("RoomScene");
    }
}
