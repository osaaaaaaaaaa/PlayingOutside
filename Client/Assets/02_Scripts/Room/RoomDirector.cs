using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class RoomDirector : MonoBehaviour
{
    [SerializeField] Text textReadyCnt;
    [SerializeField] Text textUserCnt;
    [SerializeField] GameObject roomNameObj;
    [SerializeField] Text textRoomName;
    [SerializeField] Button btnLeave;
    [SerializeField] TargetCameraController targetCameraController;
    [SerializeField] CharacterControlUI characterControlUI;

    #region �L�����N�^�[�֌W
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] List<GameObject> characterPrefabList;
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();  // ���[�U�[�̃L�����N�^�[���
    #endregion

    SEController seController;

    #region �����}�b�`���O�̃^�C���A�E�g�֌W
    Coroutine coroutineTimeout;
    DateTime startMatchingTime;
    const float timeoutSec = 5f;
    #endregion

    const float waitSeconds = 0.1f;

    private async void Start()
    {
        if (RoomModel.Instance.IsMatchingRunning) roomNameObj.SetActive(false);
        seController = GetComponent<SEController>();

        textRoomName.text = RoomModel.Instance.ConnectionRoomName;

        // �֐���o�^����
        RoomModel.Instance.OnJoinedUser += this.NotifyJoinedUser;
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnReadyUser += this.NotifyReadyUser;

        // �ڑ�����
        if (!RoomModel.Instance.IsMatchingRunning)
        {
            await RoomModel.Instance.ConnectAsync();
        }
        else
        {
            coroutineTimeout = StartCoroutine(TimeOutCoroutine());
        }
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

    IEnumerator TimeOutCoroutine()
    {
        bool isTimeOut = false;
        startMatchingTime = DateTime.Now;
        while (!isTimeOut)
        {
            yield return new WaitForSeconds(waitSeconds);
            var currentTime = DateTime.Now;
            if((currentTime - startMatchingTime).TotalSeconds > timeoutSec)
            {
                isTimeOut = true;
            }
        }

        OnTimeOut();
    }

    async void OnTimeOut()
    {
        StopCoroutine(UpdateCoroutine());
        await RoomModel.Instance.LeaveAsync();

        UnityAction errorActoin = CallSceneLoadMethod;
        ErrorUIController.Instance.ShowErrorUI("�^�C���A�E�g���������܂����B���[������ގ����܂��B", errorActoin);
    }

    public void CallSceneLoadMethod()
    {
        if (SceneControler.Instance.isLoading) SceneManager.LoadScene("TopScene");
        else SceneControler.Instance.StartSceneLoad("TopScene");
    }

    /// <summary>
    /// �������N�G�X�g
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom()
    {
        // ��������[���[����,���[�U�[ID(�ŏI�I�ɂ̓��[�J���ɕۑ����Ă��郆�[�U�[ID)]
        await RoomModel.Instance.JoinAsync(RoomModel.Instance.ConnectionRoomName, UserModel.Instance.UserId);
    }

    /// <summary>
    /// �����ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyJoinedUser(JoinedUser user)
    {
        if (RoomModel.Instance.IsMatchingRunning) return;
        bool isMyCharacter = user.ConnectionId == RoomModel.Instance.ConnectionId;

        // �L�����N�^�[����,
        GameObject character = Instantiate(characterPrefabList[user.UserData.Character_Id - 1]);
        characterList[user.ConnectionId] = character;
        character.name = user.UserData.Name;

        // �v���C���[�̏���������
        character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.JoinOrder - 1]);
        character.GetComponent<AudioListener>().enabled = isMyCharacter;

        // ���[�U�[���̏���������
        Color colorText = isMyCharacter ? Color.white : Color.green;
        character.GetComponent<PlayerUIController>().InitUI(user.UserData.Name, colorText);

        // �����ł͂Ȃ��ꍇ�̓��C���[�^�O��ύX���Ă���PlayerController���O��
        character.layer = isMyCharacter ? 3 : 7;
        character.GetComponent<PlayerController>().enabled = isMyCharacter;

        if (isMyCharacter)
        {
            targetCameraController.InitCamera(character.transform,0,user.ConnectionId); // �����̃��f���ɃJ�����̃^�[�Q�b�g��ݒ�
            characterControlUI.SetupButtonEvent(character);

            // ���[�h��ʂ����
            SceneControler.Instance.StopSceneLoad();

            StartCoroutine(UpdateCoroutine());
        }

        int minRequiredUsers = characterList.Count < 2 ? 2 : characterList.Count;
        textUserCnt.text = "/" + minRequiredUsers + " Ready";

        seController.PlayAudio();
    }

    /// <summary>
    /// �ގ����N�G�X�g
    /// </summary>
    public async void LeaveRoom()
    {
        StopCoroutine(UpdateCoroutine());
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
            // �������ގ�����ꍇ�͑S�č폜
            foreach (var character in characterList.Values)
            {
                Destroy(character);
            }
            characterList.Clear();
        }
        else
        {
            if (characterList.ContainsKey(connectionId))
            {
                // �Y���̃L�����N�^�[�폜&���X�g����폜
                DOTween.Kill(characterList[connectionId]);
                Destroy(characterList[connectionId]);
                characterList.Remove(connectionId);
            }
        }
    }

    /// <summary>
    /// �v���C���[���X�V���N�G�X�g
    /// </summary>
    public async void UpdatePlayerState()
    {
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // �v���C���[�̑��݃`�F�b�N
        var character = characterList[RoomModel.Instance.ConnectionId];
        PlayerState playerState = new PlayerState()
        {
            position = character.transform.position,
            angle = character.transform.eulerAngles,
            animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
            isActiveSelf = true,
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
        await RoomModel.Instance.ReadyAsynk(isReady);
    }

    /// <summary>
    /// ���������ʒm
    /// </summary>
    void NotifyReadyUser(int readyCnt, bool isTransitionGameScene)
    {
        textReadyCnt.text = readyCnt.ToString();
        if (isTransitionGameScene)
        {
            if(coroutineTimeout != null) StopCoroutine(coroutineTimeout);
            StopCoroutine(UpdateCoroutine());
            RoomModel.Instance.IsMatchingRunning = false;

            if (SceneControler.Instance.isLoading) SceneManager.LoadScene("RelayGameScene");
            else SceneControler.Instance.StartSceneLoad("RelayGameScene");
        }
    }
}
