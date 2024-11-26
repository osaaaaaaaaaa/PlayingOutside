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
        // ���[�U�[�����������Ƃ���this.OnJoinedUser���\�b�h�����s����悤�ɂ���
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        Debug.Log(gameObject.name);
    }

    /// <summary>
    /// �������N�G�X�g
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom(string roomName)
    {
        // ��������[���[���� = [�ŏI�I]���͂��ꂽ���[����,���[�U�[ID = [�ŏI�I]�̓��[�J���ɕۑ����Ă��郆�[�U�[ID]
        await RoomModel.Instance.JoinAsync(roomName, RoomModel.Instance.MyUserData.Id);
    }

    /// <summary>
    /// �����ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        bool isMyData = user.ConnectionId == RoomModel.Instance.ConnectionId;
        privateMatchUIController.SetupUserUI(isMyData, user);
    }

    /// <summary>
    /// �ގ����N�G�X�g
    /// </summary>
    public async void LeaveRoom()
    {
        await RoomModel.Instance.LeaveAsync();
    }

    /// <summary>
    /// �ގ��ʒm����
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            // �������ގ������ꍇ�́A���[����UI��S�ĕ���
            privateMatchUIController.InitUI();
        }
        else
        {
            // �Y���̃��[�U�[UI���폜
            JoinedUser user = RoomModel.Instance.JoinedUsers[connectionId];
            privateMatchUIController.RemoveUserUI(user.JoinOrder);
        }
    }
}
