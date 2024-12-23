using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;
using Server.Model.Entity;

public class RelayGameDirector : MonoBehaviour
{
    [SerializeField] AreaController areaController;
    [SerializeField] GameStartCountDown gameStartCountDown;
    [SerializeField] TargetCameraController targetCameraController;
    [SerializeField] SpectatingUI spectatingUI;
    [SerializeField] GameObject countDownUI;

    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    public Dictionary<Guid,GameObject> characterList { get; private set; }  = new Dictionary<Guid,GameObject>();  // ���[�U�[�̃L�����N�^�[���

    Coroutine coroutineCountDown;
    int currentTime;
    bool isGameStartCountDownOver;

    const float waitSeconds = 0.1f;

    public bool isDebug = false;

    private void Start()
    {
        if (isDebug) return;
        isGameStartCountDownOver = false;
        currentTime = 0;

        // �֐���o�^����
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnCountdownOverUser += this.NotifyStartGame;
        RoomModel.Instance.OnAreaClearedUser += this.NotifyAreaClearedUser;
        RoomModel.Instance.OnReadyNextAreaUser += this.NotifyRedyNextAreaAllUsers;
        RoomModel.Instance.OnStartCountDownUser += this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser += this.NotifyCountDownUser;
        RoomModel.Instance.OnFinishGameUser += this.NotifyFinishGameUser;

        SetupGame();
    }

    void OnDisable()
    {
        // �V�[���J�ڎ��Ɋ֐��̓o�^������
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnCountdownOverUser -= this.NotifyStartGame;
        RoomModel.Instance.OnAreaClearedUser -= this.NotifyAreaClearedUser;
        RoomModel.Instance.OnReadyNextAreaUser -= this.NotifyRedyNextAreaAllUsers;
        RoomModel.Instance.OnStartCountDownUser -= this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser -= this.NotifyCountDownUser;
        RoomModel.Instance.OnFinishGameUser -= this.NotifyFinishGameUser;
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            UpdatePlayerState();
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    IEnumerator CountDownCoroutine()
    {
        if (currentTime == 0) currentTime = 11;
        while (currentTime > 0)
        {
            currentTime--;
            OnCountDown();
            yield return new WaitForSeconds(1f);
        }
    }

    void SetupGame()
    {
        GenerateCharacters();

        // ���[�h��ʂ����
        SceneControler.Instance.StopSceneLoad();

        // ���b��ɃJ�E���g�_�E�����J�n
        gameStartCountDown.CallPlayAnim();
    }

    /// <summary>
    /// �L�����N�^�[��������
    /// </summary>
    void GenerateCharacters()
    {
        var users = RoomModel.Instance.JoinedUsers;

        foreach (var user in users)
        {
            // �L�����N�^�[����,
            GameObject character = Instantiate(characterPrefab);
            characterList[user.Key] = character;

            // �v���C���[�̏���������
            bool isMyCharacter = user.Key == RoomModel.Instance.ConnectionId;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.Value.JoinOrder - 1]);

            // ���[�U�[���̏���������
            Color colorText = isMyCharacter ? Color.white : Color.green;
            character.GetComponent<PlayerUIController>().InitUI(user.Value.UserData.Name, colorText);

            // �Q�[�����J�n����܂ł�PlayerController���O��
            character.GetComponent<PlayerController>().enabled = false;

            // ���C���[�^�O��ύX
            character.layer = isMyCharacter ? 3 : 7;

            if (isMyCharacter)
            {
                // �����̃��f���ɃJ�����̃^�[�Q�b�g��ݒ�
                targetCameraController.InitCamera(character.transform, 0, user.Key);
            }
        }
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
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // �v���C���[�̑��݃`�F�b�N
        var character = characterList[RoomModel.Instance.ConnectionId];
        if(character.GetComponent<PlayerController>().enabled)
        {
            PlayerState playerState = new PlayerState()
            {
                position = character.transform.position,
                angle = character.transform.eulerAngles,
                animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
                isActiveSelf = character.activeSelf,
            };
            await RoomModel.Instance.UpdatePlayerStateAsync(playerState);
        }
    }

    /// <summary>
    /// �v���C���[���X�V�ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedPlayerState(Guid connectionId, PlayerState playerState)
    {
        if (!isGameStartCountDownOver) return;

        // �v���C���[�̑��݃`�F�b�N
        if (!characterList.ContainsKey(connectionId)) return;

        // �ړ��E��]�E�A�j���[�V��������
        characterList[connectionId].SetActive(playerState.isActiveSelf);
        characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

    /// <summary>
    /// �Q�[���J�n�O�̃J�E���g�_�E���I�����N�G�X�g
    /// </summary>
    public async void OnCountdownOver()
    {
        isGameStartCountDownOver = true;
        await RoomModel.Instance.OnCountdownOverAsynk();
    }

    /// <summary>
    /// �Q�[���J�n�ʒm
    /// </summary>
    void NotifyStartGame()
    {
        // �Q�[���J�n�O�̃J�E���g�_�E�����\���ɂ���
        gameStartCountDown.PlayCountDownOverAnim();

        // �v���C���[�̑�����ł���悤�ɂ���
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        StartCoroutine(UpdateCoroutine());
    }

    /// <summary>
    /// ���݂̃G���A���N���A�������������N�G�X�g
    /// </summary>
    public async void OnAreaCleared()
    {
        await RoomModel.Instance.OnAreaClearedAsynk();
    }

    /// <summary>
    /// ���݂̃G���A���N���A�����ʒm
    /// </summary>
    void NotifyAreaClearedUser(Guid connectionId,string userName, bool isClearedAllUsers)
    {
        // �N���A�������[�U�[����\������
        Debug.Log(userName + "���˔j");

        if (isClearedAllUsers)
        {
            // �J�E���g�_�E���̃R���[�`�����~����
            if(coroutineCountDown != null) StopCoroutine(coroutineCountDown);
            coroutineCountDown = null;

            // �S�������݂̃G���A���N���A�����ꍇ�A���̃G���A�Ɉړ����鏀��������
            StartCoroutine(areaController.ReadyNextAreaCoroutine());
            return;
        }

        // �J�����̃^�[�Q�b�g�������̏ꍇ�͏������I��
        if (targetCameraController.currentTargetId == RoomModel.Instance.ConnectionId) return;
        characterList[connectionId].SetActive(false);   // ��\���ɂȂ��Ă��Ȃ��ꍇ�����邽��

        // ���ɃJ�����̃^�[�Q�b�g�̐؂�ւ��悪���݂��邩�`�F�b�N
        bool isTarget = targetCameraController.IsOtherTarget();
        if (targetCameraController.activeTargetCnt == 1) spectatingUI.SetupButton(false);

        // ���݂̃J�����̃^�[�Q�b�g�ƃN���A�����l������l�����ǂ���
        if (isTarget && connectionId == targetCameraController.currentTargetId)
        {
            // �J�����̃^�[�Q�b�g�̐؂�ւ��悪���݂���ꍇ�͐؂�ւ���
            spectatingUI.OnChangeTargetBtn();
        }
    }

    /// <summary>
    /// ���̃G���A�Ɉړ����鏀�����������N�G�X�g
    /// </summary>
    public async void OnReadyNextArea(bool isLastArea)
    {
        // �J�E���g�_�E���̃R���[�`�����~����
        if (coroutineCountDown != null) StopCoroutine(coroutineCountDown);
        coroutineCountDown = null;

        if (isLastArea)
        {
            // �Q�[���I�����N�G�X�g
            OnFinishGame();
        }
        else
        {
            // ���݂̃G���A���Ō�̃G���A�ł͂Ȃ��ꍇ
            await RoomModel.Instance.OnReadyNextAreaAsynk();
        }
    }

    /// <summary>
    /// �S�������̃G���A�Ɉړ����鏀�������������ʒm
    /// </summary>
    void NotifyRedyNextAreaAllUsers(float restarningWaitSec)
    {
        countDownUI.SetActive(false);
        coroutineCountDown = null;
        currentTime = 0;

        var myCharacter = characterList[RoomModel.Instance.ConnectionId];
        myCharacter.SetActive(false);

        // �Q�[���ĊJ����
        StartCoroutine(areaController.RestarningGameCoroutine(myCharacter,restarningWaitSec));
    }

    /// <summary>
    /// �G���A�N���A���̃J�E���g�_�E���J�n�ʒm
    /// (�}�X�^�[�N���C�A���g����M)
    /// </summary>
    void NotifyStartCountDown()
    {
        if (coroutineCountDown == null) coroutineCountDown = StartCoroutine(CountDownCoroutine());
    }

    /// <summary>
    /// �J�E���g�_�E������
    /// (�}�X�^�[�N���C�A���g������)
    /// </summary>
    public async void OnCountDown()
    {
        if (currentTime >= 0) await RoomModel.Instance.OnCountDownAsynk(currentTime);
    }

    /// <summary>
    /// �J�E���g�_�E���ʒm
    /// </summary>
    /// <param name="currentTime"></param>
    void NotifyCountDownUser(int currentTime)
    {
        if(coroutineCountDown == null) this.currentTime = currentTime;
        countDownUI.SetActive(true);
        countDownUI.GetComponent<CountDownUI>().UpdateText(currentTime);

        // �܂��N���A���Ă��Ȃ� && �J�E���g�_�E����0�ȉ��ɂȂ�����A���̃G���A�֋����ړ�
        if (!areaController.isClearedArea && currentTime == 0)
        {
            StartCoroutine(areaController.ReadyNextAreaCoroutine());
        }
    }

    /// <summary>
    /// �Q�[���I���������������N�G�X�g
    /// </summary>
    public async void OnFinishGame()
    {
        await RoomModel.Instance.OnFinishGameAsynk();
    }

    /// <summary>
    /// �S���̃Q�[���I�����������������ʒm
    /// </summary>
    void NotifyFinishGameUser(string nextSceneName)
    {
        StopCoroutine(UpdateCoroutine());
        SceneControler.Instance.StartSceneLoad(nextSceneName);
    }
}
