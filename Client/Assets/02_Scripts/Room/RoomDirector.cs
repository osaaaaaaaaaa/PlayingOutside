using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;

public class RoomDirector : MonoBehaviour
{
    [SerializeField] Text textReadyCnt;
    [SerializeField] Text textUserCnt;
    [SerializeField] Text textRoomName;
    [SerializeField] Button btnLeave;
    [SerializeField] TargetCameraController targetCameraController;

    #region �L�����N�^�[�֌W
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();  // ���[�U�[�̃L�����N�^�[���
    #endregion

    const float waitSeconds = 0.1f;

    private async void Start()
    {
        textRoomName.text = RoomModel.Instance.ConnectionRoomName;

        // �֐���o�^����
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnReadyUser += this.NotifyReadyUser;

        // �ڑ�����
        await RoomModel.Instance.ConnectAsync();
        // �������������N�G�X�g
        JoinRoom();
    }

    void OnDisable()
    {
        // �V�[���J�ڎ��Ɋ֐��̓o�^������
        RoomModel.Instance.OnJoinedUser -= this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnReadyUser -= this.NotifyReadyUser;
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            UpdatePlayerState();
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    /// <summary>
    /// �������N�G�X�g
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom()
    {
        // ��������[���[����,���[�U�[ID(�ŏI�I�ɂ̓��[�J���ɕۑ����Ă��郆�[�U�[ID)]
        await RoomModel.Instance.JoinAsync(RoomModel.Instance.ConnectionRoomName, RoomModel.Instance.MyUserData.Id);
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
        bool isMyCharacter = user.ConnectionId == RoomModel.Instance.ConnectionId;
        Debug.Log(user.JoinOrder);
        character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.JoinOrder - 1].position);

        // ���[�U�[���̏���������
        Color colorText = isMyCharacter ? Color.white : Color.green;
        character.GetComponent<PlayerUIController>().InitUI(user.UserData.Name, colorText);

        // �����ł͂Ȃ��ꍇ��PlayerController���O�� , ���C���[�^�O��ύX
        character.GetComponent<PlayerController>().enabled = isMyCharacter;
        character.layer = isMyCharacter ? 3 : 7;

        if (isMyCharacter)
        {
            // �����̃��f���ɃJ�����̃^�[�Q�b�g��ݒ�
            targetCameraController.InitCamera(character.transform,0);

            // ���[�h��ʂ����
            SceneControler.Instance.StopSceneLoad();

            StartCoroutine(UpdateCoroutine());
        }

        int minRequiredUsers = characterList.Count < 2 ? 2 : characterList.Count;
        textUserCnt.text = "/" + minRequiredUsers + " Ready";
    }

    /// <summary>
    /// �ގ����N�G�X�g
    /// </summary>
    public async void LeaveRoom()
    {
        await RoomModel.Instance.LeaveAsync();

        SceneControler.Instance.StartSceneLoad("TopScene");
    }

    /// <summary>
    /// �ގ��ʒm����
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        if (connectionId == RoomModel.Instance.ConnectionId)
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
            // �Y���̃L�����N�^�[�폜&���X�g����폜
            Destroy(characterList[connectionId]);
            characterList.Remove(connectionId);
        }
    }

    /// <summary>
    /// �v���C���[���X�V���N�G�X�g
    /// </summary>
    public async void UpdatePlayerState()
    {
        var character = characterList[RoomModel.Instance.ConnectionId];
        PlayerState playerState = new PlayerState()
        {
            position = character.transform.position,
            angle = character.transform.eulerAngles,
            animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
        };
        await RoomModel.Instance.UpdatePlayerStateAsync(playerState);
    }

    /// <summary>
    /// �v���C���[���X�V�ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedPlayerState(Guid connectionId, PlayerState playerState)
    {
        if (!characterList.ContainsKey(connectionId)) return;   // �v���C���[�̑��݃`�F�b�N

        // �ړ��E��]�E�A�j���[�V��������
        characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

    /// <summary>
    /// �����ł������ǂ����̃��N�G�X�g
    /// </summary>
    public async void OnReadyCircle(bool isReady)
    {
        btnLeave.interactable = !isReady;   // �����������͑ގ��{�^���������Ȃ��悤�ɂ���
        await RoomModel.Instance.OnReadyAsynk(isReady);
    }

    /// <summary>
    /// ���������ʒm
    /// </summary>
    void NotifyReadyUser(int readyCnt, bool isTransitionGameScene)
    {
        textReadyCnt.text = readyCnt.ToString();
        if (isTransitionGameScene)
        {
            StopCoroutine(UpdateCoroutine());
            characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>();
            SceneControler.Instance.StartSceneLoad("GameScene");
        }
    }
}
