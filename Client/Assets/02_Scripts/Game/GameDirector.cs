using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class GameDirector : MonoBehaviour
{
    [SerializeField] InputField userIdField;

    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] RoomModel roomModel;
    Dictionary<Guid,GameObject> characterList = new Dictionary<Guid,GameObject>();  // ���[�U�[�̃L�����N�^�[���

    Coroutine updateCoroutine;

    private async void Start()
    {
        // ���[�U�[�����������Ƃ���this.OnJoinedUser���\�b�h�����s����悤�ɂ���
        roomModel.OnJoinedUser += this.NotifyJoinedUser;
        roomModel.OnLeavedUser += this.NotifyLeavedUser;
        roomModel.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;

        // �ڑ�����
        await roomModel.ConnectAsync();
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

            yield return new WaitForSeconds(0.04f);
        }

        updateCoroutine = null;
    }

    /// <summary>
    /// �������N�G�X�g
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom()
    {
        int id = int.Parse(userIdField.text);

        // ��������[���[����,���[�U�[ID(�ŏI�I�ɂ̓��[�J���ɕۑ����Ă���ID���g��)]
        await roomModel.JoinAsync("sampleRoom", id);
    }

    /// <summary>
    /// �����ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        // �L�����N�^�[����,
        GameObject character = Instantiate(characterPrefab);
        characterList[user.ConnectionId] = character;

        // �v���C���[�̏���������
        bool isMyCharacter = user.ConnectionId == roomModel.ConnectionId;
        Debug.Log(user.JoinOrder);
        character.GetComponent<PlayerController>().InitPlayer(this, characterStartPoints[user.JoinOrder - 1].position);

        // ���[�U�[���̏���������
        Color colorText = isMyCharacter ? Color.white : Color.green;
        character.GetComponent<PlayerUIController>().InitUI(user.UserData.Name, colorText);

        // �����ł͂Ȃ��ꍇ��PlayerController���O��
        character.GetComponent<PlayerController>().enabled = isMyCharacter;
    }

    /// <summary>
    /// �ގ����N�G�X�g
    /// </summary>
    public async void LeaveRoom()
    {
        await roomModel.LeaveAsync();
    }

    /// <summary>
    /// �ގ��ʒm����
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        if (connectionId == roomModel.ConnectionId) 
        {
            // �������ޏo����ꍇ�͑S�č폜
            foreach (var character in characterList.Values)
            {
                Destroy(character);
            }
            characterList.Clear();
        }
        else
        {
            // �Y���̃L�����N�^�[�폜
            Destroy(characterList[connectionId]);
            characterList.Remove(connectionId);
        }
    }

    /// <summary>
    /// �v���C���[���X�V���N�G�X�g
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
    /// �v���C���[���X�V�ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedPlayerState(Guid connectionId,PlayerState playerState)
    {
        if (!characterList.ContainsKey(connectionId)) return;   // �v���C���[�̑��݃`�F�b�N
        Debug.Log(playerState.position.ToString());
        Debug.Log(playerState.angle.ToString());
        characterList[connectionId].transform.position = playerState.position;
        characterList[connectionId].transform.rotation = Quaternion.Euler(playerState.angle);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

}
