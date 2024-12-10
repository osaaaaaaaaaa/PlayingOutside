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
    Dictionary<Guid, int> userList = new Dictionary<Guid, int>(); // <�ڑ�ID,������>

    private void Start()
    {
        // �֐���o�^����
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
    }

    void OnDisable()
    {
        // �V�[���J�ڎ��Ɋ֐��̓o�^������
        RoomModel.Instance.OnJoinedUser -= this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
    }

    /// <summary>
    /// �N�C�b�N�Q�[���{�^�����������ꍇ
    /// </summary>
    public async void OnQuickGameButtonAsync()
    {
        // �ڑ�����
        await RoomModel.Instance.ConnectAsync();
        // �}�b�`���O���������N�G�X�g
        JoinLobby();

        quickGameUIController.OnSelectButton();
    }

    /// <summary>
    /// �}�b�`���O���N�G�X�g
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinLobby()
    {
        RoomModel.Instance.IsMatchingRunning = true;
        await RoomModel.Instance.JoinLobbyAsynk(RoomModel.Instance.MyUserData.Id);
    }

    /// <summary>
    /// �����ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        userList[user.ConnectionId] = user.JoinOrder;

        // ���[�U�[��UI����ݒ�
        quickGameUIController.SetupUserFrame(user.JoinOrder - 1, user.UserData.Name, 1 - 1);
    }

    /// <summary>
    /// �ގ����N�G�X�g
    /// </summary>
    public async void LeaveRoom()
    {
        RoomModel.Instance.IsMatchingRunning = false;
        await RoomModel.Instance.LeaveAsync();
        quickGameUIController.OnBackButton();
    }

    /// <summary>
    /// �ގ��ʒm����
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        Debug.Log(connectionId + "���ގ�");
        // �}�b�`���O���������āA�����̃��r�[�ގ��ʒm(�}�b�`���O�����ʒm)���͂����ꍇ
        if (RoomModel.Instance.IsMatchingRunning && connectionId == RoomModel.Instance.ConnectionId) return;

        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            // �������ޏo����ꍇ�͑S�č폜
            quickGameUIController.InitAllUserFrame();
            userList.Clear();
        }
        else
        {
            if (userList.ContainsKey(connectionId))
            {
                // �Y���̃��[�U�[��UI�폜
                quickGameUIController.InitUserFrame(userList[connectionId] - 1);
            }
        }
    }
}
