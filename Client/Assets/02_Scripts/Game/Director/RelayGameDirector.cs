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
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
    [SerializeField] List<GameObject> characterPrefabList;
    public Dictionary<Guid,GameObject> characterList { get; private set; }  = new Dictionary<Guid,GameObject>();  // ���[�U�[�̃L�����N�^�[���
    #endregion

    #region �}�X�^�[�N���C�A���g�Ɠ�������M�~�b�N
    Dictionary<string, Goose> gooseObjList = new Dictionary<string, Goose>();
    Dictionary<string, MoveSetRoot> movingObjectList = new Dictionary<string, MoveSetRoot>();
    #endregion

    #region �����̃M�~�b�N
    [SerializeField] List<GameObject> animalGimmicks;
    Dictionary<string,GameObject> animalGimmickList = new Dictionary<string, GameObject>();
    #endregion

    #region �A���̃M�~�b�N�֌W
    [SerializeField] List<PlantGroupController> plantGroupControllers;
    bool isDestroyPlantRequest;
    bool isDestroyedPlants;
    #endregion

    Dictionary<string, GameObject> itemList = new Dictionary<string, GameObject>();

    #region �Q�[���I���֌W
    Coroutine coroutineFinishGame;
    bool isFinishedGame;
    #endregion

    #region �J�E���g�_�E���֌W
    Coroutine coroutineCountDown;
    const int maxTime = 16;
    int currentTime;
    bool isGameStartCountDownOver;
    #endregion

    const float waitSeconds = 0.1f;
    bool isStartGame = false;
    public bool isDebug = false;

    private void Start()
    {
        if (isDebug) return;

        isDestroyPlantRequest = false;
        isDestroyedPlants = false;
        isGameStartCountDownOver = false;
        currentTime = maxTime;

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
        RoomModel.Instance.OnAfterFinalGameUser += this.NotifyAfterFinalGameUser;

        RoomModel.Instance.OnGetItemUser += this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser += this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser += this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser += this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser += this.NotifySpawnObjectUser;
        RoomModel.Instance.OnPlayAnimalGimmickUser += this.NotifyPlayAnimalGimmickUser;

        RoomModel.Instance.OnDestroyPlantsGimmickUser += this.NotifyDestroyPlantsGimmickUser;
        RoomModel.Instance.OnTriggeringPlantGimmickUser += this.NotifyTriggeringPlantGimmickUser;

        // [�����}�b�`���O������] �Q���҂����������������ꍇ
        if (RoomModel.Instance.JoinedUsers.Count == 1)
        {
            OnOnlyPlayerRemaining();
            return;
        }

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
        RoomModel.Instance.OnAfterFinalGameUser -= this.NotifyAfterFinalGameUser;

        RoomModel.Instance.OnGetItemUser -= this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser -= this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser -= this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser -= this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser -= this.NotifySpawnObjectUser;
        RoomModel.Instance.OnPlayAnimalGimmickUser -= this.NotifyPlayAnimalGimmickUser;

        RoomModel.Instance.OnDestroyPlantsGimmickUser -= this.NotifyDestroyPlantsGimmickUser;
        RoomModel.Instance.OnTriggeringPlantGimmickUser -= this.NotifyTriggeringPlantGimmickUser;
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
                    if (!isDestroyPlantRequest)
                    {
                        // �܂��A���̃M�~�b�N��j�����Ă��Ȃ��ꍇ
                        DestroyPlantsGimmickAsynk();
                    }
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
        while (currentTime > 0 && !isFinishedGame)
        {
            currentTime--;
            OnCountDown();
            yield return new WaitForSeconds(1f);
        }
        coroutineCountDown = null;
    }

    /// <summary>
    /// �Q���҂�����1�l�������Ƃ��̏���
    /// </summary>
    async void OnOnlyPlayerRemaining()
    {
        // �ޏo����
        StopCoroutine(UpdateCoroutine());
        if (coroutineCountDown != null) StopCoroutine(coroutineCountDown);
        await RoomModel.Instance.LeaveAsync();

        UnityAction errorActoin = CallSceneLoadMethod;
        ErrorUIController.Instance.ShowErrorUI("���̃��[�U�[���ؒf���A�Ō�̎Q���҂ɂȂ������߁A���[������ގ����܂��B", errorActoin);
    }

    /// <summary>
    /// �V�[���J�ڂ̊֐����Ă�
    /// </summary>
    public void CallSceneLoadMethod()
    {
        if (SceneControler.Instance.isLoading) SceneManager.LoadScene("TopScene");
        else SceneControler.Instance.StartSceneLoad("TopScene");
    }

    void SetupGame()
    {
        GenerateCharacters();

        // �}�X�^�[�N���C�A���g�Ɠ�������I�u�W�F�N�g���擾���Đݒ肷��
        areaController.ToggleAllGimmicks(true);
        var movingRootObjs = new List<MoveSetRoot>(FindObjectsOfType<MoveSetRoot>());
        var gooseObjs = new List<Goose>(FindObjectsOfType<Goose>());
        areaController.ToggleAllGimmicks(false);
        foreach (var item in movingRootObjs)
        {
            movingObjectList.Add(item.name, item);
        }
        foreach(var item in gooseObjs)
        {
            gooseObjList.Add(item.name, item);
        }

        // �����̃M�~�b�N��ݒ�
        foreach (var item in animalGimmicks)
        {
            animalGimmickList.Add(item.name, item);
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
            character.name = user.Value.UserData.Name;

            // �v���C���[�̏���������
            bool isMyCharacter = user.Key == RoomModel.Instance.ConnectionId;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.Value.JoinOrder - 1],isMyCharacter);
            character.GetComponent<PlayerController>().ToggleGravityAndColliders(false);
            character.GetComponent<AudioListener>().enabled = isMyCharacter;

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

            userScoreController.InitUserScoreList(value.JoinOrder, value.UserData.Character_Id - 1, value.UserData.Name, value.score);
        }
    }

    /// <summary>
    /// �ގ����N�G�X�g
    /// </summary>
    public async void LeaveRoom()
    {
        StopCoroutine(UpdateCoroutine());
        if(coroutineCountDown != null) StopCoroutine(coroutineCountDown);
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
            else if(characterList.Count == 1 && !isStartGame)
            {
                isFinishedGame = true;
            }
        }

        if (RoomModel.Instance.JoinedUsers.ContainsKey(RoomModel.Instance.ConnectionId))
        {
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

        // �v���C���[�̑�����ł���悤�ɂ���
        foreach(var character in characterList.Values)
        {
            character.GetComponent<PlayerController>().ToggleGravityAndColliders(true);
        }

        characterControlUI.OnSkillButton();
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        StartCoroutine(UpdateCoroutine());

        // �A�C�e���̃X�|�[���J�n
        areaController.ActiveItemSpawner();
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

        // �����ȊO���N���A�����ꍇ
        if (RoomModel.Instance.ConnectionId != connectionId)
        {
            characterList[connectionId].GetComponent<PlayerEffectController>().SetEffect(PlayerEffectController.EFFECT_ID.AreaCleared);
            characterList[connectionId].SetActive(false);
        }

        // �S�Ẵ��[�U�[���N���A�����ꍇ
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
    void NotifyRedyNextAreaAllUsers(float restarningWaitSec, EnumManager.RELAY_AREA_ID nextAreaId)
    {
        countDownUI.SetActive(false);
        if (coroutineCountDown != null) StopCoroutine(coroutineCountDown);
        coroutineCountDown = null;
        currentTime = maxTime;

        var myCharacter = characterList[RoomModel.Instance.ConnectionId];
        myCharacter.SetActive(false);

        // �Q�[���ĊJ����
        StartCoroutine(areaController.RestarningGameCoroutine(nextAreaId, myCharacter,restarningWaitSec));
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
        if (!isFinishedGame && coroutineCountDown == null && currentTime > 0) coroutineCountDown = StartCoroutine(CountDownCoroutine());
    }

    /// <summary>
    /// �J�E���g�_�E������
    /// (�}�X�^�[�N���C�A���g������)
    /// </summary>
    public async void OnCountDown()
    {
        if (currentTime >= 0 && !areaController.isClearedArea && !isFinishedGame)
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
        if (isFinishedGame) return;
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
        if (areaController.ItemSpawnerList[(int)areaController.currentAreaId].enabled)
        {
            var item = areaController.ItemSpawnerList[(int)areaController.currentAreaId].Spawn(spawnPoint, itemId, itemName);
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
    /// �����̃M�~�b�N�����ʒm
    /// </summary>
    /// <param name="name"></param>
    /// <param name="option"></param>
    void NotifyPlayAnimalGimmickUser(EnumManager.ANIMAL_GIMMICK_ID animalId, string name, Vector3[] option)
    {
        var animal = animalGimmickList[name];
        if(animal != null && animal.activeSelf)
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
    /// �A���̃M�~�b�N��j�����郊�N�G�X�g
    /// (�}�X�^�[�N���C�A���g�����s)
    /// </summary>
    async void DestroyPlantsGimmickAsynk()
    {
        if (isDestroyPlantRequest) return;
        isDestroyPlantRequest = true;
        List<string> destroyNames = new List<string>();
        foreach(var group in plantGroupControllers)
        {
            var names = group.GetDestroyPlantNames();
            if (names.Length > 0) destroyNames.AddRange(names);
        }

        if(destroyNames.Count > 0) await RoomModel.Instance.DestroyPlantsGimmickAsynk(destroyNames.ToArray());
    }

    /// <summary>
    /// �A���̃M�~�b�N��j������ʒm
    /// </summary>
    /// <param name="names"></param>
    void NotifyDestroyPlantsGimmickUser(string[] names)
    {
        if(isDestroyedPlants) return;
        isDestroyPlantRequest = true;
        isDestroyedPlants = true;

        foreach (var group in plantGroupControllers)
        {
            group.DestroyPlants(names);
        }
    }

    /// <summary>
    /// �A���̃M�~�b�N�𔭓�����ʒm
    /// </summary>
    /// <param name="name"></param>
    void NotifyTriggeringPlantGimmickUser(string name)
    {
        foreach (var group in plantGroupControllers)
        {
            if (group.HidePlantList.ContainsKey(name))
            {
                group.HidePlantList[name].ShowPlant();
            }
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
    /// �S����(�J���g���[�����[�ɂ����Ă�)�Q�[���I�����������������ʒm
    /// </summary>
    void NotifyFinishGameUser(string nextSceneName)
    {
        StopCoroutine(UpdateCoroutine());
        SceneControler.Instance.StartSceneLoad(nextSceneName);
    }

    /// <summary>
    /// �S�Ă̋��Z���I�����A���U���g�V�[���ֈړ�����ʒm
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
        isFinishedGame = true;
        if (coroutineCountDown != null) StopCoroutine(coroutineCountDown);
        coroutineCountDown = null;

        // ����𖳌�������
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = false;
        characterList[RoomModel.Instance.ConnectionId].layer = 8;   // �M�~�b�N�Ȃǂ̓����蔻��𖳂���

        // �Q�[���I������UI��\��
        areaController.FinishUI.SetActive(true);
        yield return new WaitForSeconds(areaController.FinishUI.GetComponent<FinishUI>().animSec + 1f);  // �]�C�̎��Ԃ����Z

        // �Q�[���I�����N�G�X�g
        OnFinishGame();
    }
}
