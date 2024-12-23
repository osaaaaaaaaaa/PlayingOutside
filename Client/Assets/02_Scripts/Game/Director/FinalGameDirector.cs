using Cinemachine;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AreaController;

public class FinalGameDirector : MonoBehaviour
{
    #region UI�֌W
    [SerializeField] GameStartCountDown gameStartCountDown;
    [SerializeField] GameObject countDownUI;
    [SerializeField] GameObject finishUI;
    #endregion

    #region �L�����N�^�[���
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    public Dictionary<Guid, GameObject> characterList { get; private set; } = new Dictionary<Guid, GameObject>();  // ���[�U�[�̃L�����N�^�[���
    #endregion

    #region �J�����֌W
    [SerializeField] CinemachineTargetGroup targetGroup;
    #endregion

    [SerializeField] GameObject coinPrefab;
    [SerializeField] GameObject gimmick;

    Coroutine coroutineCountDown;
    int currentTime;
    bool isGameStartCountDownOver;

    const float waitSeconds = 0.1f;

    public bool isDebug = false;

    private void Start()
    {
        if (isDebug) return;
        isGameStartCountDownOver = false;

        // �֐���o�^����
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnCountdownOverUser += this.NotifyStartGame;
        RoomModel.Instance.OnStartCountDownUser += this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser += this.NotifyCountDownUser;
        RoomModel.Instance.OnAfterFinalGameUser += this.NotifyAfterFinalGameUser;
        RoomModel.Instance.OnDropCoinsUser += this.NotifyDropCoinsUser;
        RoomModel.Instance.OnDropCoinsAtRandomPositionsUser += this.NotifyDropCoinsAtRandomPositions;
        RoomModel.Instance.OnGetItemUser += this.NotifyGetItemUser;

        SetupGame();
    }

    void OnDisable()
    {
        // �V�[���J�ڎ��Ɋ֐��̓o�^������
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnCountdownOverUser -= this.NotifyStartGame;
        RoomModel.Instance.OnStartCountDownUser -= this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser -= this.NotifyCountDownUser;
        RoomModel.Instance.OnAfterFinalGameUser -= this.NotifyAfterFinalGameUser;
        RoomModel.Instance.OnDropCoinsUser -= this.NotifyDropCoinsUser;
        RoomModel.Instance.OnDropCoinsAtRandomPositionsUser -= this.NotifyDropCoinsAtRandomPositions;
        RoomModel.Instance.OnGetItemUser -= this.NotifyGetItemUser;
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
        if (currentTime == 0) currentTime = 121;
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

        // �J�����̃^�[�Q�b�g�O���[�v��ݒ肷��
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[characterList.Count];
        int i = 0;
        foreach (var target in characterList.Values)
        {
            targetGroup.m_Targets[i] = new CinemachineTargetGroup.Target()
            {
                target = target.transform,
                weight = 1,
                radius = 1,
            };
            i++;
        }

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
            Vector3 startPos = characterStartPoints[user.Value.JoinOrder - 1].position;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.Value.JoinOrder - 1]);

            // ���[�U�[���̏���������
            Color colorText = isMyCharacter ? Color.white : Color.green;
            character.GetComponent<PlayerUIController>().InitUI(user.Value.UserData.Name, colorText);

            // �Q�[�����J�n����܂ł�PlayerController���O��
            character.GetComponent<PlayerController>().enabled = false;

            // ���C���[�^�O��ύX
            character.layer = isMyCharacter ? 3 : 7;
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
        if (character.GetComponent<PlayerController>().enabled)
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

        // �M�~�b�N���N��
        gimmick.SetActive(true);
    }

    /// <summary>
    /// �J�E���g�_�E���J�n�ʒm
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
        if (coroutineCountDown == null) this.currentTime = currentTime;
        countDownUI.SetActive(true);
        countDownUI.GetComponent<CountDownUI>().UpdateText(currentTime);

        // �J�E���g�_�E����0�ɂȂ����ꍇ
        if (currentTime == 0)
        {
            StartCoroutine(FinishGameCoroutine());
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
    /// �Ō�̋��Z���I�������ʒm
    /// </summary>
    void NotifyAfterFinalGameUser()
    {
        // �ŏI���ʔ��\�V�[���ɑJ��
        StopCoroutine(UpdateCoroutine());
        SceneControler.Instance.StartSceneLoad("FinalResultsScene");
    }

    /// <summary>
    /// �Q�[���I������
    /// </summary>
    public IEnumerator FinishGameCoroutine()
    {
        // ����𖳌�������
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = false;
        characterList[RoomModel.Instance.ConnectionId].layer = 8;   // �M�~�b�N�Ȃǂ̓����蔻��𖳂���

        // �Q�[���I������UI��\��
        finishUI.SetActive(true);
        yield return new WaitForSeconds(finishUI.GetComponent<FinishUI>().animSec + 1f);  // �]�C�̎��Ԃ����Z

        // �Q�[���I�����N�G�X�g
        OnFinishGame();
    }

    /// <summary>
    /// �R�C��(�|�C���g)�̃h���b�v�ʒm
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="anglesY"></param>
    void NotifyDropCoinsUser(Vector3 startPoint, int[] anglesY, string[] coinNames)
    {
        for (int i = 0; i < coinNames.Length; i++)
        {
            var coin = Instantiate(coinPrefab);
            coin.transform.position = startPoint;
            coin.name = coinNames[i];
            coin.GetComponent<CoinController>().Drop(anglesY[i]);
        }
    }

    /// <summary>
    /// �����ꏊ���قȂ�R�C��(�|�C���g)�̃h���b�v�ʒm
    /// </summary>
    /// <param name="startPoins"></param>
    /// <param name="coinNames"></param>
    void NotifyDropCoinsAtRandomPositions(Vector3[] startPoins, string[] coinNames)
    {
        Debug.Log("�R�C������");
        for (int i = 0; i < coinNames.Length; i++)
        {

            Debug.Log(startPoins[i].ToString() + "�ɗ���");
            var coin = Instantiate(coinPrefab);
            coin.transform.position = startPoins[i];
            coin.name = coinNames[i];
        }
    }

    /// <summary>
    /// �A�C�e���擾�ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemName"></param>
    /// <param name="option"></param>
    void NotifyGetItemUser(Guid connectionId, string itemName, float option)
    {
        GameObject item = GameObject.Find(itemName);
        if (item == null) return;

        item.GetComponent<ItemController>().UseItem();
        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            Debug.Log("�����Ɏg�p����");
        }
    }
}
