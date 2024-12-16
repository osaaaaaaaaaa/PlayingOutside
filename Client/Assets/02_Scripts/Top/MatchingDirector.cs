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
    public bool isTaskRunnning;
    bool isReceivedOnMatching;

    private void Start()
    {
        // �֐���o�^����
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnmatchingUser += this.NotifyMatching;

        RoomModel.Instance.IsMatchingRunning = false;
        isTaskRunnning = false;
        isReceivedOnMatching = false;
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
        if (isTaskRunnning  || RoomModel.Instance.IsMatchingRunning) return;
        isTaskRunnning = true;

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

        await RoomModel.Instance.JoinLobbyAsynk(UserModel.Instance.UserId);
    }

    /// <summary>
    /// �}�b�`���O�����ʒm
    /// </summary>
    void NotifyMatching()
    {
        isReceivedOnMatching = true;
    }

    /// <summary>
    /// �����ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        if (user.ConnectionId == RoomModel.Instance.ConnectionId) isTaskRunnning = false;
        userList[user.ConnectionId] = user.JoinOrder;

        // ���[�U�[��UI����ݒ�
        quickGameUIController.SetupUserFrame(user.JoinOrder - 1, user.UserData.Name, 1 - 1);
    }

    /// <summary>
    /// �ގ����N�G�X�g(�{�^������)
    /// </summary>
    public async void LeaveRoom()
    {
        if (isTaskRunnning || !RoomModel.Instance.IsMatchingRunning) return;
        isTaskRunnning = true;
        RoomModel.Instance.IsMatchingRunning = false;
        isReceivedOnMatching = false;

        await RoomModel.Instance.LeaveAsync();
        quickGameUIController.OnBackButton();
    }

    /// <summary>
    /// �ގ��ʒm����
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        Debug.Log(connectionId + "���ގ�");
        Debug.Log(RoomModel.Instance.ConnectionId + "������");
        // �}�b�`���O���������āA�����̃��r�[�ގ��ʒm(�}�b�`���O�����ʒm)���͂����ꍇ
        if (RoomModel.Instance.IsMatchingRunning && connectionId == RoomModel.Instance.ConnectionId)
        {
            // 4�Ԗڂ̃��[�U�[�����������u�ԂɃV�[���J�ڂ���̂�j�~���邽��
            Invoke("CallFuncSceneLoad", 1f);
            return;
        }

        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            // �������ޏo����ꍇ�͑S�č폜
            quickGameUIController.InitAllUserFrame();
            userList.Clear();
            isTaskRunnning = false;
        }
        else
        {
            if (isReceivedOnMatching) return;

            if (userList.ContainsKey(connectionId))
            {
                // �Y���̃��[�U�[��UI�폜
                quickGameUIController.InitUserFrame(userList[connectionId] - 1);
            }
        }
    }

    void CallFuncSceneLoad()
    {
        SceneControler.Instance.StartSceneLoad("RoomScene");
    }
}
