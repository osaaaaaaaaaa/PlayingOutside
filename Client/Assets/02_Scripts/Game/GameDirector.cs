using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class GameDirector : MonoBehaviour
{
    [SerializeField] InputField userIdField;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] RoomModel roomModel;
    Dictionary<Guid,GameObject> characterList = new Dictionary<Guid,GameObject>();  // ���[�U�[�̃L�����N�^�[���

    private async void Start()
    {
        // ���[�U�[�����������Ƃ���this.OnJoinedUser���\�b�h�����s����悤�ɂ���
        roomModel.OnJoinedUser += this.OnJoinedUser;

        // �ڑ�����
        await roomModel.ConnectAsync();
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom()
    {
        int id = int.Parse(userIdField.text);

        // ��������[���[����,���[�U�[ID(�ŏI�I�ɂ̓��[�J���ɕۑ����Ă���ID���g��)]
        await roomModel.JoinAsync("sampleRoom", id);
    }

    /// <summary>
    /// ���[�U�[�����ʒm����
    /// </summary>
    /// <param name="user"></param>
    void OnJoinedUser(JoinedUser user)
    {
        // �L�����N�^�[����,
        GameObject caracterObject = Instantiate(characterPrefab);
        caracterObject.transform.position = Vector3.zero;
        characterList[user.ConnectionId] = caracterObject;
    }
}
