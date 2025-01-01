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
    [SerializeField] GameStartCountDown gameStartCountDown;
    [SerializeField] SpectatingUI spectatingUI;
    [SerializeField] GameObject countDownUI;

    #region �R���g���[���[�֌W
    [SerializeField] AreaController areaController;
    [SerializeField] TargetCameraController targetCameraController;
    [SerializeField] CharacterControlUI characterControlUI;
    [SerializeField] UserScoreController userScoreController;
    #endregion

    #region �L�����N�^�[�֌W
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    public Dictionary<Guid,GameObject> characterList { get; private set; }  = new Dictionary<Guid,GameObject>();  // ���[�U�[�̃L�����N�^�[���
    #endregion

    #region MoveRoot�X�N���v�g��K�p���Ă���M�~�b�N
    [SerializeField] GameObject gimmicksParent;
    [SerializeField] List<GameObject> movingObjects;
    Dictionary<string, MoveSetRoot> movingObjectList = new Dictionary<string, MoveSetRoot>();
    #endregion

    Dictionary<string, GameObject> itemList = new Dictionary<string, GameObject>();

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
        RoomModel.Instance.OnUpdateMasterClientUser += this.NotifyUpdatedMasterClient;
        RoomModel.Instance.OnCountdownOverUser += this.NotifyStartGame;
        RoomModel.Instance.OnAreaClearedUser += this.NotifyAreaClearedUser;
        RoomModel.Instance.OnReadyNextAreaUser += this.NotifyRedyNextAreaAllUsers;
        RoomModel.Instance.OnStartCountDownUser += this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser += this.NotifyCountDownUser;
        RoomModel.Instance.OnFinishGameUser += this.NotifyFinishGameUser;
        RoomModel.Instance.OnUpdateScoreUser += this.NotifyUpdateScore;

        RoomModel.Instance.OnGetItemUser += this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser += this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser += this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser += this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser += this.NotifySpawnObjectUser;

        SetupGame();
    }

    void OnDisable()
    {
        // �V�[���J�ڎ��Ɋ֐��̓o�^������
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnUpdateMasterClientUser -= this.NotifyUpdatedMasterClient;
        RoomModel.Instance.OnCountdownOverUser -= this.NotifyStartGame;
        RoomModel.Instance.OnAreaClearedUser -= this.NotifyAreaClearedUser;
        RoomModel.Instance.OnReadyNextAreaUser -= this.NotifyRedyNextAreaAllUsers;
        RoomModel.Instance.OnStartCountDownUser -= this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser -= this.NotifyCountDownUser;
        RoomModel.Instance.OnFinishGameUser -= this.NotifyFinishGameUser;
        RoomModel.Instance.OnUpdateScoreUser -= this.NotifyUpdateScore;

        RoomModel.Instance.OnGetItemUser -= this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser -= this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser -= this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser -= this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser -= this.NotifySpawnObjectUser;
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient)
            {
                UpdateMasterClientAsynk();
            }
            else
            {
                UpdatePlayerState();
            }
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

        // �����I�u�W�F�N�g��ݒ�
        foreach (var item in movingObjects)
        {
            movingObjectList.Add(item.name, item.GetComponent<MoveSetRoot>());
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
            var value = user.Value;

            // �L�����N�^�[����,
            GameObject character = Instantiate(characterPrefab);
            characterList[user.Key] = character;
            character.name = user.Value.UserData.Name;

            // �v���C���[�̏���������
            bool isMyCharacter = user.Key == RoomModel.Instance.ConnectionId;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.Value.JoinOrder - 1]);

            // ���[�U�[���̏���������
            Color colorText = isMyCharacter ? Color.white : Color.green;
            character.GetComponent<PlayerUIController>().InitUI(user.Value.UserData.Name, colorText);

            // ���C���[�^�O��ύX
            character.layer = isMyCharacter ? 3 : 7;
            // �Q�[�����J�n����܂ł�PlayerController���O��
            character.GetComponent<PlayerController>().enabled = false;

            if (isMyCharacter)
            {
                targetCameraController.InitCamera(character.transform, 0, user.Key);    // �����̃��f���ɃJ�����̃^�[�Q�b�g��ݒ�
                characterControlUI.SetupButtonEvent(character);
            }

            userScoreController.InitUserScoreList(value.JoinOrder, value.UserData.Character_Id, value.UserData.Name, value.score);
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

        if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient)
        {
            foreach (var obj in movingObjectList)
            {
                obj.Value.ResumeTween();
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
    /// �}�X�^�[�N���C�A���g�̏��X�V���N�G�X�g
    /// </summary>
    public async void UpdateMasterClientAsynk()
    {
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // �v���C���[�̑��݃`�F�b�N

        List<MovingObjectState> movingObjectStates = new List<MovingObjectState>();
        foreach (var obj in movingObjects)
        {
            if (obj.activeSelf && movingObjectList[obj.name].pathTween != null)
            {
                MovingObjectState movingObjectState = new MovingObjectState()
                {
                    name = obj.name,
                    position = obj.transform.position,
                    angle = obj.transform.eulerAngles,
                    elapsedTimeTween = movingObjectList[obj.name].pathTween.Elapsed(),
                    isActiveSelf = obj.activeSelf,
                };
                movingObjectStates.Add(movingObjectState);
            }
        }

        var character = characterList[RoomModel.Instance.ConnectionId];
        PlayerState playerState = null;
        if (character.GetComponent<PlayerController>().enabled)
        {
            playerState = new PlayerState()
            {
                position = character.transform.position,
                angle = character.transform.eulerAngles,
                animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
                isActiveSelf = character.activeSelf,
            };
        }

        MasterClient masterClient = new MasterClient()
        {
            playerState = playerState,
            objectStates = movingObjectStates,
        };
        await RoomModel.Instance.UpdateMasterClientAsynk(masterClient);
    }

    /// <summary>
    /// �}�X�^�[�N���C�A���g�̏��X�V�ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedMasterClient(Guid connectionId, MasterClient masterClient)
    {
        if (!isGameStartCountDownOver) return;

        // �v���C���[�̑��݃`�F�b�N
        if (!characterList.ContainsKey(connectionId)) return;

        if (masterClient.playerState != null)
        {
            PlayerState playerState = masterClient.playerState;

            // �ړ��E��]�E�A�j���[�V��������
            characterList[connectionId].SetActive(playerState.isActiveSelf);
            characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
            characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
            characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
        }

        // �I�u�W�F�N�g�̓���
        foreach (var obj in masterClient.objectStates)
        {
            movingObjectList[obj.name].SetPotition(obj, waitSeconds);
        }
    }


    /// <summary>
    /// �Q�[���J�n�O�̃J�E���g�_�E���I�����N�G�X�g
    /// </summary>
    public async void OnCountdownOver()
    {
        isGameStartCountDownOver = true;
        await RoomModel.Instance.CountdownOverAsynk();
    }

    /// <summary>
    /// �Q�[���J�n�ʒm
    /// </summary>
    void NotifyStartGame()
    {
        // �Q�[���J�n�O�̃J�E���g�_�E�����\���ɂ���
        gameStartCountDown.PlayCountDownOverAnim();

        // �v���C���[�̑�����ł���悤�ɂ���
        characterControlUI.OnSkillButton();
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        StartCoroutine(UpdateCoroutine());
    }

    /// <summary>
    /// ���݂̃G���A���N���A�������������N�G�X�g
    /// </summary>
    public async void OnAreaCleared()
    {
        await RoomModel.Instance.AreaClearedAsynk();
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
            await RoomModel.Instance.ReadyNextAreaAsynk();
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
    /// ���[�U�[�̏����|�C���g�X�V�ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="score"></param>
    void NotifyUpdateScore(Guid connectionId, int score)
    {
        userScoreController.UpdateScore(RoomModel.Instance.JoinedUsers[connectionId].JoinOrder, score);
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
        if (currentTime >= 0 && !areaController.isClearedArea)
        {
            await RoomModel.Instance.CountDownAsynk(currentTime);
        }
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
    /// �A�C�e���擾�ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemName"></param>
    /// <param name="option"></param>
    void NotifyGetItemUser(Guid connectionId, string itemName, float option)
    {
        if (!itemList.ContainsKey(itemName)) return;
        var itemController = itemList[itemName].GetComponent<ItemController>();

        if (itemController.ItemId == EnumManager.ITEM_ID.Coin)
        {
            userScoreController.UpdateScore(RoomModel.Instance.JoinedUsers[connectionId].JoinOrder, (int)option);
        }
        else if (connectionId == RoomModel.Instance.ConnectionId)
        {
            characterControlUI.GetComponent<CharacterControlUI>().SetImageItem(itemController.ItemId);
            characterList[connectionId].GetComponent<PlayerItemController>().SetItemSlot(itemController.ItemId);
        }

        Destroy(itemList[itemName]);
    }

    /// <summary>
    /// �A�C�e���g�p�ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    void NotifyUseItemUser(Guid connectionId, EnumManager.ITEM_ID itemId)
    {
        characterList[connectionId].GetComponent<PlayerItemController>().UseItem(itemId);
    }

    /// <summary>
    /// �A�C�e���̔j���ʒm
    /// </summary>
    /// <param name="itemName"></param>
    void NotifyDestroyItemUser(string itemName)
    {
        if (itemList.ContainsKey(itemName))
        {
            Destroy(itemList[itemName]);
        }
    }

    /// <summary>
    /// �A�C�e���̐����ʒm
    /// </summary>
    /// <param name="spawnPoint"></param>
    /// <param name="itemId"></param>
    void NotifySpawnItemUser(Vector3 spawnPoint, EnumManager.ITEM_ID itemId, string itemName)
    {
        if (areaController.ItemSpawnerList[(int)areaController.areaId].enabled)
        {
            var item = areaController.ItemSpawnerList[(int)areaController.areaId].Spawn(spawnPoint, itemId, itemName);
            if (!itemList.ContainsKey(item.name)) itemList.Add(itemName, item);
        }
    }

    /// <summary>
    /// ���I�ȃI�u�W�F�N�g�̐����ʒm
    /// </summary>
    /// <param name="spawnObject"></param>
    void NotifySpawnObjectUser(SpawnObject spawnObject)
    {
        GetComponent<ObjectPrefabController>().Spawn(spawnObject);
    }

    /// <summary>
    /// �Q�[���I���������������N�G�X�g
    /// </summary>
    public async void OnFinishGame()
    {
        await RoomModel.Instance.FinishGameAsynk();
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