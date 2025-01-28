using Cinemachine;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using static AreaController;
using static UnityEngine.Rendering.DebugUI;

public class FinalGameDirector : MonoBehaviour
{
    #region UI�֌W
    [SerializeField] GameStartCountDown gameStartCountDown;
    [SerializeField] GameObject countDownUI;
    [SerializeField] GameObject finishUI;
    [SerializeField] CharacterControlUI characterControlUI;
    [SerializeField] UserScoreController userScoreController;
    #endregion

    #region �L�����N�^�[���
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] List<GameObject> characterPrefabList;
    public Dictionary<Guid, GameObject> characterList { get; private set; } = new Dictionary<Guid, GameObject>();  // ���[�U�[�̃L�����N�^�[���
    #endregion

    #region �J�����֌W
    [SerializeField] CinemachineTargetGroup targetGroup;
    #endregion

    #region �A�C�e���֌W
    [SerializeField] ItemSpawner itemSpawner;
    Dictionary<string, GameObject> itemList = new Dictionary<string, GameObject>();
    [SerializeField] GameObject coinPrefab;
    #endregion

    #region �}�X�^�[�N���C�A���g�Ɠ�������M�~�b�N
    [SerializeField] GameObject movingGimmicksParent;
    Dictionary<string, MoveSetRoot> movingObjectList = new Dictionary<string, MoveSetRoot>();
    Dictionary<string, Goose> gooseObjList = new Dictionary<string, Goose>();
    [SerializeField] bool isStartShowMovingGimmicks = false;
    #endregion

    #region �����̃M�~�b�N
    [SerializeField] List<GameObject> animalGimmicks = new List<GameObject>();
    Dictionary<string, GameObject> animalGimmickList = new Dictionary<string, GameObject>();
    MegaCoop megaCoopGimmick; // �{�����̃M�~�b�N
    #endregion

    #region �Q�[���I���֌W
    Coroutine coroutineFinishGame;
    bool isFinishedGame;
    #endregion

    #region �J�E���g�_�E���֌W
    Coroutine coroutineCountDown;
    const int maxTime = 31;
    int currentTime;
    bool isGameStartCountDownOver;
    #endregion

    const float waitSeconds = 0.1f;
    bool isStartGame = false;
    public bool isDebug = false;

    private void Start()
    {
        if (isDebug)
        {
            itemSpawner.enabled = true;
            return;
        }
        itemSpawner.enabled = false;
        isGameStartCountDownOver = false;
        currentTime = maxTime;

        // �֐���o�^����
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnUpdateMasterClientUser += this.NotifyUpdatedMasterClient;
        RoomModel.Instance.OnCountdownOverUser += this.NotifyStartGame;
        RoomModel.Instance.OnAfterFinalGameUser += this.NotifyAfterFinalGameUser;
        RoomModel.Instance.OnDropCoinsUser += this.NotifyDropCoinsUser;
        RoomModel.Instance.OnDropCoinsAtRandomPositionsUser += this.NotifyDropCoinsAtRandomPositions;
        #region �Q�[�����ʂ̒ʒm����
        RoomModel.Instance.OnUpdateScoreUser += this.NotifyUpdateScore;
        RoomModel.Instance.OnStartCountDownUser += this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser += this.NotifyCountDownUser;
        RoomModel.Instance.OnGetItemUser += this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser += this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser += this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser += this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser += this.NotifySpawnObjectUser;
        RoomModel.Instance.OnPlayAnimalGimmickUser += this.NotifyPlayAnimalGimmickUser;
        RoomModel.Instance.OnTriggerMegaCoopUser += this.NotifyTriggerMegaCoopUser;
        RoomModel.Instance.OnTriggerMegaCoopEndUser += this.NotifyTriggerMegaCoopEndUser;
        #endregion

        SetupGame();
    }

    void OnDisable()
    {
        // �V�[���J�ڎ��Ɋ֐��̓o�^������
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnUpdateMasterClientUser -= this.NotifyUpdatedMasterClient;
        RoomModel.Instance.OnCountdownOverUser -= this.NotifyStartGame;
        RoomModel.Instance.OnAfterFinalGameUser -= this.NotifyAfterFinalGameUser;
        RoomModel.Instance.OnDropCoinsUser -= this.NotifyDropCoinsUser;
        RoomModel.Instance.OnDropCoinsAtRandomPositionsUser -= this.NotifyDropCoinsAtRandomPositions;
        #region �Q�[�����ʂ̒ʒm����
        RoomModel.Instance.OnUpdateScoreUser -= this.NotifyUpdateScore;
        RoomModel.Instance.OnStartCountDownUser -= this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser -= this.NotifyCountDownUser;
        RoomModel.Instance.OnGetItemUser -= this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser -= this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser -= this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser -= this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser -= this.NotifySpawnObjectUser;
        RoomModel.Instance.OnPlayAnimalGimmickUser -= this.NotifyPlayAnimalGimmickUser;
        RoomModel.Instance.OnTriggerMegaCoopUser -= this.NotifyTriggerMegaCoopUser;
        RoomModel.Instance.OnTriggerMegaCoopEndUser -= this.NotifyTriggerMegaCoopEndUser;
        #endregion
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            bool isMasterClient = false;
            if (RoomModel.Instance.JoinedUsers.ContainsKey(RoomModel.Instance.ConnectionId))
            {
                if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient)
                {
                    isMasterClient = true;
                    UpdateMasterClientAsynk();
                }
            }
            if(!isMasterClient)
            {
                UpdatePlayerState();
            }
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    IEnumerator CountDownCoroutine()
    {
        while (currentTime > 0)
        {
            currentTime--;
            OnCountDown();
            yield return new WaitForSeconds(1f);
        }
        coroutineCountDown = null;
    }

    void SetupGame()
    {
        GenerateCharacters();

        // �}�X�^�[�N���C�A���g�Ɠ�������I�u�W�F�N�g���擾���Đݒ肷��
        movingGimmicksParent.SetActive(true);
        var movingRootObjs = new List<MoveSetRoot>(FindObjectsOfType<MoveSetRoot>());
        var gooseObjs = new List<Goose>(FindObjectsOfType<Goose>());
        if(!isStartShowMovingGimmicks) movingGimmicksParent.SetActive(false);
        foreach (var item in movingRootObjs)
        {
            movingObjectList.Add(item.name, item);
        }
        foreach (var item in gooseObjs)
        {
            gooseObjList.Add(item.name, item);
        }

        // �����̃M�~�b�N��ݒ�
        foreach (var item in animalGimmicks)
        {
            animalGimmickList.Add(item.name, item);
        }

        // �{�����̃M�~�b�N�擾
        var getCoopGimmick = FindObjectOfType<MegaCoop>();
        if(getCoopGimmick != null) megaCoopGimmick = getCoopGimmick;

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
            var value = user.Value;

            // �L�����N�^�[����,
            GameObject character = Instantiate(characterPrefabList[value.UserData.Character_Id - 1]);
            characterList[user.Key] = character;
            character.name = value.UserData.Name;

            // �v���C���[�̏���������
            bool isMyCharacter = user.Key == RoomModel.Instance.ConnectionId;
            Vector3 startPos = characterStartPoints[value.JoinOrder - 1].position;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[value.JoinOrder - 1], isMyCharacter);
            character.GetComponent<PlayerController>().ToggleGravityAndColliders(false);
            character.GetComponent<AudioListener>().enabled = isMyCharacter;

            // ���[�U�[���̏���������
            Color colorText = isMyCharacter ? Color.white : Color.green;
            character.GetComponent<PlayerUIController>().InitUI(value.UserData.Name, colorText);

            // ���C���[�^�O��ύX
            character.layer = isMyCharacter ? 3 : 7;
            // �Q�[�����J�n����܂ł�PlayerController���O��
            character.GetComponent<PlayerController>().enabled = false;

            if (isMyCharacter)
            {
                characterControlUI.SetupButtonEvent(character);
            }

            userScoreController.InitUserScoreList(value.JoinOrder, value.UserData.Character_Id - 1, value.UserData.Name, value.score);
        }
    }

    /// <summary>
    /// �ގ����N�G�X�g
    /// </summary>
    public async void LeaveRoom()
    {
        StopCoroutine(UpdateCoroutine());
        if (coroutineCountDown != null) StopCoroutine(coroutineCountDown);
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
            DOTween.Kill(characterList[connectionId]);
            Destroy(characterList[connectionId]);
            characterList.Remove(connectionId);

            // �������Ō�̈�l�ɂȂ����ꍇ�̓Q�[�����I������
            if (characterList.Count == 1 && isStartGame && !isFinishedGame)
            {
                if (coroutineFinishGame == null) coroutineFinishGame = StartCoroutine(FinishGameCoroutine());
            }
            else if (characterList.Count == 1 && !isStartGame)
            {
                isFinishedGame = true;
            }
        }

        if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient)
        {
            foreach (var obj in movingObjectList)
            {
                if (obj.Value != null)
                {
                    if (obj.Value.gameObject.activeSelf) obj.Value.ResumeTween();
                }
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
    /// �}�X�^�[�N���C�A���g�̏��X�V���N�G�X�g
    /// </summary>
    public async void UpdateMasterClientAsynk()
    {
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // �v���C���[�̑��݃`�F�b�N

        // ���[�g�ɉ����ē����I�u�W�F�N�g�̏��擾
        List<MovingObjectState> movingObjectStates = new List<MovingObjectState>();
        foreach (var obj in movingObjectList.Values)
        {
            if (obj.gameObject.activeSelf && movingObjectList[obj.name].pathTween != null)
            {
                MovingObjectState movingObjectState = new MovingObjectState()
                {
                    name = obj.name,
                    position = obj.transform.position,
                    angle = obj.transform.eulerAngles,
                    elapsedTimeTween = movingObjectList[obj.name].pathTween.Elapsed(),
                    isActiveSelf = obj.gameObject.activeSelf,
                };
                movingObjectStates.Add(movingObjectState);
            }
        }

        // �K�`���E�̏��擾
        List<GooseState> gooseObjStates = new List<GooseState>();
        foreach (var obj in gooseObjList.Values)
        {
            if (obj.gameObject.activeSelf)
            {
                GooseState gooseState = new GooseState()
                {
                    name = obj.name,
                    position = obj.transform.position,
                    angle = obj.transform.eulerAngles,
                    animationId = obj.GetAnimationId(),
                };
                gooseObjStates.Add(gooseState);
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
            gooseStates = gooseObjStates,
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

        if(masterClient.playerState != null)
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

        // �K�`���E�̓���
        foreach (var goose in masterClient.gooseStates)
        {
            gooseObjList[goose.name].UpdateState(goose, waitSeconds);
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

        isStartGame = true;

        // �Q�[���I������
        if (isFinishedGame)
        {
            if (coroutineFinishGame == null) coroutineFinishGame = StartCoroutine(FinishGameCoroutine());
            return;
        }

        // �M�~�b�N���N��
        movingGimmicksParent.SetActive(true);
        foreach (var goose in gooseObjList.Values)
        {
            goose.enabled = true;
            goose.InitMember();
        }

        // �v���C���[�̑�����ł���悤�ɂ���
        foreach (var character in characterList.Values)
        {
            character.GetComponent<PlayerController>().ToggleGravityAndColliders(true);
        }
        characterControlUI.OnSkillButton();
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        StartCoroutine(UpdateCoroutine());

        // �A�C�e���X�|�[���J�n
        itemSpawner.enabled = true;
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
    /// �J�E���g�_�E���J�n�ʒm
    /// (�}�X�^�[�N���C�A���g����M)
    /// </summary>
    void NotifyStartCountDown()
    {
        if (!isFinishedGame && coroutineCountDown == null && currentTime > 0) coroutineCountDown = StartCoroutine(CountDownCoroutine());
    }

    /// <summary>
    /// �J�E���g�_�E������
    /// (�}�X�^�[�N���C�A���g������)
    /// </summary>
    public async void OnCountDown()
    {
        if (currentTime >= 0 && !isFinishedGame) await RoomModel.Instance.CountDownAsynk(currentTime);
    }

    /// <summary>
    /// �J�E���g�_�E���ʒm
    /// </summary>
    /// <param name="currentTime"></param>
    void NotifyCountDownUser(int currentTime)
    {
        if (isFinishedGame) return;
        if (coroutineCountDown == null) this.currentTime = currentTime;
        countDownUI.SetActive(true);
        countDownUI.GetComponent<CountDownUI>().UpdateText(currentTime);

        // �J�E���g�_�E����0�ɂȂ����ꍇ
        if (currentTime == 0)
        {
            if(coroutineFinishGame == null) coroutineFinishGame = StartCoroutine(FinishGameCoroutine());
        }
    }

    /// <summary>
    /// �Q�[���I���������������N�G�X�g
    /// </summary>
    public async void OnFinishGame()
    {
        await RoomModel.Instance.FinishGameAsynk();
    }

    /// <summary>
    /// �Ō�̋��Z���I�������ʒm
    /// </summary>
    void NotifyAfterFinalGameUser()
    {
        // �ŏI���ʔ��\�V�[���ɑJ��
        SceneControler.Instance.StartSceneLoad("FinalResultsScene");
    }

    /// <summary>
    /// �Q�[���I������
    /// </summary>
    public IEnumerator FinishGameCoroutine()
    {
        isFinishedGame = true;
        if (coroutineCountDown != null) StopCoroutine(coroutineCountDown);
        coroutineCountDown = null;

        // ����𖳌�������
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = false;
        characterList[RoomModel.Instance.ConnectionId].layer = 8;   // �M�~�b�N�Ȃǂ̓����蔻��𖳂���

        // �Q�[���I������UI��\��
        finishUI.SetActive(true);
        yield return new WaitForSeconds(finishUI.GetComponent<FinishUI>().animSec + 1f);  // �]�C�̎��Ԃ����Z

        StopCoroutine(UpdateCoroutine());

        // �Q�[���I�����N�G�X�g
        OnFinishGame();
    }

    /// <summary>
    /// �R�C��(�|�C���g)�̃h���b�v�ʒm
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="anglesY"></param>
    void NotifyDropCoinsUser(Vector3 startPoint, int[] anglesY, string[] coinNames, UserScore latestUserScore)
    {
        for (int i = 0; i < coinNames.Length; i++)
        {
            var coin = Instantiate(coinPrefab);
            coin.transform.position = startPoint;
            coin.name = coinNames[i];
            coin.GetComponent<CoinController>().Drop(anglesY[i]);
            if(!itemList.ContainsKey(coin.name)) itemList.Add(coin.name, coin);
        }

        userScoreController.UpdateScore(RoomModel.Instance.JoinedUsers[latestUserScore.ConnectionId].JoinOrder, latestUserScore.LatestScore);
    }

    /// <summary>
    /// �����ꏊ���قȂ�R�C��(�|�C���g)�̃h���b�v�ʒm
    /// </summary>
    /// <param name="startPoins"></param>
    /// <param name="coinNames"></param>
    void NotifyDropCoinsAtRandomPositions(Vector3[] startPoins, string[] coinNames, UserScore latestUserScore)
    {
        for (int i = 0; i < coinNames.Length; i++)
        {
            var coin = Instantiate(coinPrefab);
            coin.transform.position = startPoins[i];
            coin.name = coinNames[i];
            if (!itemList.ContainsKey(coin.name)) itemList.Add(coin.name, coin);
        }

        userScoreController.UpdateScore(RoomModel.Instance.JoinedUsers[latestUserScore.ConnectionId].JoinOrder, latestUserScore.LatestScore);
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

        characterList[connectionId].GetComponent<PlayerAudioController>().PlayOneShot(PlayerAudioController.AudioClipName.item_get);

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
        var item = itemSpawner.Spawn(spawnPoint, itemId, itemName);
        if (!itemList.ContainsKey(item.name)) itemList.Add(itemName, item);
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
    /// �����̃M�~�b�N�����ʒm
    /// </summary>
    /// <param name="name"></param>
    /// <param name="option"></param>
    void NotifyPlayAnimalGimmickUser(EnumManager.ANIMAL_GIMMICK_ID animalId, string name, Vector3[] option)
    {
        var animal = animalGimmickList[name];
        if (animal != null && animal.activeSelf)
        {
            switch (animalId)
            {
                case EnumManager.ANIMAL_GIMMICK_ID.Bull:
                    animal.GetComponent<BullGimmick>().PlayEatAnim();
                    break;
                case EnumManager.ANIMAL_GIMMICK_ID.Chicken:
                    animal.transform.GetChild(0).GetComponent<ChickenGimmick>().GenerateEggBulletWarning(option);
                    break;
            }
        }
    }

    /// <summary>
    /// �{�����̃M�~�b�N�����ʒm
    /// </summary>
    void NotifyTriggerMegaCoopUser()
    {
        if(megaCoopGimmick != null) megaCoopGimmick.PlayDamageAnim();
    }

    /// <summary>
    /// �{�����̃M�~�b�N�I���ʒm
    /// </summary>
    void NotifyTriggerMegaCoopEndUser()
    {
        if (megaCoopGimmick != null) megaCoopGimmick.OnTriggerEnd();
    }
}
