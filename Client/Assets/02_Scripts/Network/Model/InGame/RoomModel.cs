using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion.Client;
using Server.Model.Entity;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static PlayerAnimatorController;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannel channel;
    private IRoomHub roomHub;

    // �ڑ�ID
    public Guid ConnectionId { get; private set; }
    // �ڑ����郋�[����
    string connectionRoomName;
    public string ConnectionRoomName {  get { return connectionRoomName; } set { connectionRoomName = value; } }
    // �Q�����Ă��郆�[�U�[�̏��
    public Dictionary<Guid,JoinedUser> JoinedUsers { get; private set; } = new Dictionary<Guid,JoinedUser>();
    // �}�b�`���O��(�}�b�`���O����)���ǂ���
    bool isMatchingRunning = false;
    public bool IsMatchingRunning { get { return isMatchingRunning; }  set { isMatchingRunning = value; } }

    #region �T�[�o�[����ʒm���͂����ۂɌĂ΂��Action�֐�
    public Action OnmatchingUser { get; set; }
    // ���[�U�[�ڑ��ʒm
    public Action<JoinedUser> OnJoinedUser { get; set; }
    // ���[�U�[�ؒf�ʒm
    public Action<Guid> OnLeavedUser { get; set; }
    // �v���C���[���X�V�ʒm
    public Action<Guid, PlayerState> OnUpdatePlayerStateUser { get; set; }
    // �}�X�^�[�N���C�A���g�̏��X�V�ʒm
    public Action<Guid, MasterClient> OnUpdateMasterClientUser { get; set; }

    #region �Q�[���J�n�܂ł̏���
    // �J���g���[�����[�̒��ԃG���A��I�������ʒm
    public Action<EnumManager.SELECT_MID_AREA_ID> OnSelectMidAreaIdUser { get; set; }
    // �����������������ǂ����̒ʐM
    public Action<int,bool> OnReadyUser { get; set; }
    // �S�����Q�[���J�n�O�̃J�E���g�_�E���I���ʒm
    public Action OnCountdownOverUser { get; set; }
    #endregion

    #region �Q�[�����ʂ̏���
    // ���[�U�[�̏����|�C���g�X�V�ʒm
    public Action<Guid,int> OnUpdateScoreUser { get; set; }
    // �J�E���g�_�E���ʒm
    public Action<int> OnCountDownUser { get; set; }
    // �S���̃Q�[���I�����������������ʒm
    public Action<string> OnFinishGameUser { get; set; }
    // �R�C���̃h���b�v�ʒm
    public Action<Vector3, int[], string[], UserScore> OnDropCoinsUser { get; set; }
    // �����ꏊ���قȂ�R�C���̃h���b�v�ʒm
    public Action<Vector3[], string[], UserScore> OnDropCoinsAtRandomPositionsUser { get; set; }
    // �A�C�e���擾�ʒm
    public Action<Guid, string,float> OnGetItemUser {  get; set; }
    // �A�C�e���g�p�ʒm
    public Action<Guid,EnumManager.ITEM_ID> OnUseItemUser { get; set; }
    // �A�C�e���̔j���ʒm
    public Action<string> OnDestroyItemUser { get; set; }
    // �A�C�e���̐����ʒm
    public Action<Vector3,EnumManager.ITEM_ID,string> OnSpawnItemUser { get; set; }
    // ���I�ȃI�u�W�F�N�g�̐����ʒm
    public Action<SpawnObject> OnSpawnObjectUser { get; set; }
    // �����̃M�~�b�N�����ʒm
    public Action<EnumManager.ANIMAL_GIMMICK_ID, string, Vector3[]> OnPlayAnimalGimmickUser { get; set; }
    #endregion

    #region ���Z�w�J���g���[�����[�x�̏���
    // �A���̃M�~�b�N��j������ʒm
    public Action<string[]> OnDestroyPlantsGimmickUser { get; set; }
    // �A���̃M�~�b�N�𔭓�����ʒm
    public Action<string> OnTriggeringPlantGimmickUser { get; set; }
    // ���݂̃G���A���N���A�����ʒm
    public Action<Guid,string,bool> OnAreaClearedUser { get; set; }
    // �S�������̃G���A�Ɉړ����鏀�������������ʒm (�Q�[���ĊJ�ʒm)
    public Action<float, EnumManager.RELAY_AREA_ID> OnReadyNextAreaUser { get; set; }
    // �J�E���g�_�E���J�n�ʒm
    public Action OnStartCountDownUser { get; set; }
    #endregion

    #region �Q�[���I���܂ł̏���(�ŏI���ʔ��\�V�[���̏���)
    /// <summary>
    /// �ŏI���ʔ��\�V�[���ɑJ�ڂ�������
    /// </summary>
    public Action OnAfterFinalGameUser { get; set; }

    /// <summary>
    /// �S�����J�ڂł����ʒm
    /// </summary>
    public Action<ResultData[]> OnTransitionFinalResultSceneUser { get; set; }
    #endregion

    #endregion

    // �o�b�N�O���E���h�Ɉڍs�������ǂ���
    bool isBackground;

    /// <summary>
    /// �����̏�
    /// </summary>
    public enum USER_STATE
    {
        disconnect = 0, // �T�[�o�[�Ɩ��ڑ�
        connect,        // �T�[�o�[�Ɛڑ�����
        joined,         // ���[���ɓ�������
        leave,          // ���[������ގ����郊�N�G�X�g�𑗐M����
        leave_done,     // ���[������̑ގ�����������
    }
    public USER_STATE userState { get; private set; } = USER_STATE.disconnect;

    // �C���X�^���X�쐬
    private static RoomModel instance;
    public static RoomModel Instance
    {
        get
        {
            // GET�v���p�e�B���Ă΂ꂽ�Ƃ��ɃC���X�^���X���쐬����(����̂�)
            if (instance == null)
            {
                GameObject gameObj = new GameObject("RoomModel");
                instance = gameObj.AddComponent<RoomModel>();
                DontDestroyOnLoad(gameObj);
            }
            return instance;
        }
    }

    /// <summary>
    /// MagicOnion�ڑ�����
    /// </summary>
    public async UniTask ConnectAsync()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler });
        roomHub = await StreamingHubClient.ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);

        userState = USER_STATE.connect;
    }

    /// <summary>
    /// MagicOnion�ؒf����
    /// </summary>
    public async UniTask DisconnectAsync()
    {
        userState = USER_STATE.disconnect;

        if (roomHub != null) await roomHub.DisposeAsync();
        if(channel != null) await channel.ShutdownAsync();
        roomHub = null;channel = null;
    }

    /// <summary>
    /// �o�b�N�O���E���h�Ɉڍs������T�[�o�[�Ƃ̐ڑ���ؒf
    /// </summary>
    /// <param name="pauseStatus"></param>
    private async void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("�A�v�����ꎞ��~(�o�b�N�O���E���h�ɍs����)");
            if(userState == USER_STATE.joined)
            {
                isBackground = true;
            }
            await DisconnectAsync();
        }
        else
        {
            Debug.Log("�A�v�����ĊJ(�o�b�N�O���E���h����߂���)");
            if (isBackground)
            {
                isBackground = false;
                ErrorUIController.Instance.ShowErrorUI("�T�[�o�[�Ƃ̐ڑ����ؒf����܂����B", OnError);
            }
        }
    }

    /// <summary>
    /// �j�������(�A�v���I�����Ȃ�)�ɃT�[�o�[�Ƃ̐ڑ���ؒf
    /// </summary>
    private async void OnDestroy()
    {
        await DisconnectAsync();
    }

    void OnError()
    {
        if (SceneControler.Instance.isLoading) SceneManager.LoadScene("TopScene");
        else SceneControler.Instance.StartSceneLoad("TopScene");
    }

    /// <summary>
    /// ���r�[��������
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask JoinLobbyAsynk(int userId)
    {
        JoinedUsers.Clear();
        JoinedUser[] users = await roomHub.JoinLobbyAsynk(userId);
        if(users != null) Debug.Log("���[�U�[��" + users.Length);

        if (users == null)
        {
            await DisconnectAsync();

            // �����Ɏ��s�����ꍇ��TopScene�ɖ߂�
            ErrorUIController.Instance.ShowErrorUI("�����Ɏ��s���܂����B������x���������������B", OnError);
        }
        else
        {
            foreach (JoinedUser user in users)
            {
                if (user.UserData.Id == userId) this.ConnectionId = user.ConnectionId;  // ���g�̐ڑ�ID��T���ĕۑ�����

                // ���݂��Ȃ���Βǉ�(�����̃��[�U�[�������ɓ��������ۂ̑΍�)
                if (!JoinedUsers.ContainsKey(user.ConnectionId))
                {
                    JoinedUsers.Add(user.ConnectionId, user);
                    OnJoinedUser(user);
                }
            }

            userState = USER_STATE.joined;

            // �}�b�`���O���������Ă���ꍇ
            if (JoinedUsers[this.ConnectionId].IsMatching)
            {
                Debug.Log("�Ō�̐l���}�b�`���O����");
                await LeaveAsync();
            }
        }
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �}�b�`���O�����������ʒm
    /// </summary>
    /// <param name="user"></param>
    public async void OnMatching(string roomName)
    {
        Debug.Log("�}�b�`���O�����ʒm");
        OnmatchingUser();
        ConnectionRoomName = roomName;
        if (userState == USER_STATE.joined) await LeaveAsync();  // 4�Ԗڂ̃��[�U�[�ȊO����������͂�
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask JoinAsync(string roomName, int userId)
    {
        JoinedUsers.Clear();
        JoinedUser[] users = await roomHub.JoinAsynk(roomName, userId, IsMatchingRunning);

        if (users == null)
        {
            await DisconnectAsync();

            // �����Ɏ��s�����ꍇ��TopScene�ɖ߂�
            ErrorUIController.Instance.ShowErrorUI("�����Ɏ��s���܂����B������x���������������B", OnError);
            return;
        }
        else
        {
            foreach (JoinedUser user in users)
            {
                if (user.UserData.Id == userId) this.ConnectionId = user.ConnectionId;  // ���g�̐ڑ�ID��T���ĕۑ�����

                // ���݂��Ȃ���Βǉ�(�����̃��[�U�[�������ɓ��������ۂ̑΍�)
                if (!JoinedUsers.ContainsKey(user.ConnectionId))
                {
                    JoinedUsers.Add(user.ConnectionId, user);
                    OnJoinedUser(user);
                }
            }

            userState = USER_STATE.joined;

            // �����}�b�`���O���͓����ł����珀���������N�G�X�g�𑗐M
            if (IsMatchingRunning)
            {
                await ReadyAsynk(true);
            }
        }
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// ���̃��[�U�[�����������ʒm
    /// </summary>
    /// <param name="user"></param>
    public void OnJoin(JoinedUser user)
    {
        // �A�N�V�������s
        if (!JoinedUsers.ContainsKey(user.ConnectionId)) JoinedUsers.Add(user.ConnectionId, user);  // ���݂��Ȃ���Βǉ�
        if (userState == USER_STATE.joined) OnJoinedUser(user);
    }

    /// <summary>
    /// �ގ�����
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask LeaveAsync()
    {
        if (userState != USER_STATE.joined) return;
        userState = USER_STATE.leave;

        // �T�[�o�[�ɑގ����������N�G�X�g
        await roomHub.LeaveAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// ���[�U�[���ގ�����ʒm�i�������܂ށj
    /// </summary>
    /// <param name="user"></param>
    public void OnLeave(Guid connectionId, JoinedUser latestData)
    {
        if (userState == USER_STATE.leave_done) return;

        // �����̃��[�U�[�����X�V
        JoinedUsers[this.ConnectionId] = latestData;

        // �A�N�V�������s
        OnLeavedUser(connectionId);
        JoinedUsers.Remove(connectionId);

        // �������ގ�����ꍇ
        if (this.ConnectionId == connectionId)
        {
            userState = USER_STATE.leave_done;
            if (!IsMatchingRunning) OnDestroy();    // �����}�b�`���O���ȊO�ł���ΐؒf����
        }
    }

    /// <summary>
    /// �v���C���[���X�V����
    /// </summary>
    /// <param name="playerState"></param>
    /// <returns></returns>
    public async UniTask UpdatePlayerStateAsync(PlayerState playerState)
    {
        if (userState == USER_STATE.joined) 
        {
            await roomHub.UpdatePlayerStateAsynk(playerState);
        }
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �v���C���[���X�V�ʒm
    /// </summary>
    /// <param name="user"></param>
    public void OnUpdatePlayerState(Guid connectionId, PlayerState playerState)
    {
        // �A�N�V�������s
        if (userState == USER_STATE.joined) 
        {
            OnUpdatePlayerStateUser(connectionId, playerState); 
        }
    }

    /// <summary>
    /// �}�X�^�[�N���C�A���g�̏��X�V����
    /// </summary>
    /// <param name="playerState"></param>
    /// <returns></returns>
    public async UniTask UpdateMasterClientAsynk(MasterClient masterClient)
    {
        if (userState == USER_STATE.joined)
        {
            await roomHub.UpdateMasterClientAsynk(masterClient);
        }
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �}�X�^�[�N���C�A���g�̏��X�V�ʒm
    /// </summary>
    /// <param name="user"></param>
    public void OnUpdateMasterClient(Guid connectionId, MasterClient masterClient)
    {
        // �A�N�V�������s
        if (userState == USER_STATE.joined)
        {
            OnUpdateMasterClientUser(connectionId, masterClient);
        }
    }

    #region �Q�[���J�n�܂ł̏���
    /// <summary>
    /// ���Z�J���g���[�����[�̒��ԃG���A��I������
    /// (�}�X�^�[�N���C�A���g������)
    /// </summary>
    /// <param name="selectMidAreaId"></param>
    /// <returns></returns>
    public async UniTask SelectMidAreaAsynk(EnumManager.SELECT_MID_AREA_ID selectMidAreaId)
    {
        if(userState == USER_STATE.joined) await roomHub.SelectMidAreaAsynk(selectMidAreaId);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// ���Z�J���g���[�����[�̒��ԃG���A��I�������ʒm
    /// </summary>
    public void OnSelectMidArea(EnumManager.SELECT_MID_AREA_ID selectMidAreaId)
    {
        Debug.Log(selectMidAreaId.ToString());
        if (userState == USER_STATE.leave || userState == USER_STATE.leave_done) return;

        OnSelectMidAreaIdUser(selectMidAreaId);
    }

    /// <summary>
    /// �����̏����������������ǂ���
    /// </summary>
    /// <returns></returns>
    public async UniTask ReadyAsynk(bool isReady)
    {
        if(userState == USER_STATE.joined) await roomHub.ReadyAsynk(isReady);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// ���������������ǂ����̒ʒm
    /// </summary>
    public void OnReady(int readyCnt, bool isTransitionGameScene)
    {
        Debug.Log(userState.ToString());
        if (userState == USER_STATE.leave || userState == USER_STATE.leave_done) return;

        // �A�N�V�������s
        OnReadyUser(readyCnt, isTransitionGameScene);
    }

    /// <summary>
    /// �����̃Q�[���J�n�O�̃J�E���g�_�E�����I��
    /// </summary>
    /// <returns></returns>
    public async UniTask CountdownOverAsynk()
    {
        // �T�[�o�[�ɃJ�E���g�_�E�����I���������Ƃ����N�G�X�g
        await roomHub.CountdownOverAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �S�����Q�[���J�n�O�̃J�E���g�_�E���I���ʒm
    /// </summary>
    public void OnCountdownOver()
    {
        // �A�N�V�������s
        if (userState == USER_STATE.joined) OnCountdownOverUser();
    }
    #endregion

    #region �Q�[�����ʏ���
    /// <summary>
    /// ���[�U�[�̏����|�C���g�X�V�ʒm
    /// </summary>
    public void OnUpdateScore(UserScore latestUserScore)
    {
        if (userState == USER_STATE.joined)
        {
            JoinedUsers[latestUserScore.ConnectionId].score = latestUserScore.LatestScore;
            OnUpdateScoreUser(latestUserScore.ConnectionId, latestUserScore.LatestScore);
        }
    }

    /// <summary>
    /// �J�E���g�_�E������
    /// (�}�X�^�[�N���C�A���g������)
    /// </summary>
    /// <param name="currentTime"></param>
    /// <returns></returns>
    public async UniTask CountDownAsynk(int currentTime)
    {
        if (userState == USER_STATE.joined) await roomHub.CountDownAsynk(currentTime);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �J�E���g�_�E���ʒm
    /// </summary>
    /// <param name="restarningWaitSec"></param>
    public void OnCountDown(int currentTime)
    {
        if (userState == USER_STATE.joined) OnCountDownUser(currentTime);
    }

    /// <summary>
    /// �Q�[���I���������������N�G�X�g
    /// </summary>
    /// <returns></returns>
    public async UniTask FinishGameAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.FinishGameAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �S�����Q�[���I�����������������ʒm
    /// </summary>
    /// <param name="scene"></param>
    public void OnFinishGame(EnumManager.SCENE_ID scene)
    {
        if (userState == USER_STATE.joined)
        {
            string sceneName = "";
            switch (scene)
            {
                case EnumManager.SCENE_ID.RelayGame:
                    sceneName = "RelayGameScene";
                    break;
                case EnumManager.SCENE_ID.FinalGame:
                    sceneName = "FinalGameScene";
                    break;
            }
            Debug.Log("���̃Q�[���F" + sceneName);
            OnFinishGameUser(sceneName);
        }
    }

    /// <summary>
    /// �m�b�N�_�E�����ɌĂяo��
    /// </summary>
    /// <param name="startPoint"></param>
    /// <returns></returns>
    public async UniTask KnockDownAsynk(Vector3 startPoint)
    {
        if (userState == USER_STATE.joined) await roomHub.KnockDownAsynk(startPoint);
    }

    /// <summary>
    /// ��O�ɏo�����ɌĂяo��
    /// </summary>
    /// <param name="rangePointA"></param>
    /// <param name="rangePointB"></param>
    /// <returns></returns>
    public async UniTask OutOfBoundsAsynk(Vector3 rangePointA, Vector3 rangePointB)
    {
        if (userState == USER_STATE.joined) await roomHub.OutOfBoundsAsynk(rangePointA, rangePointB);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �R�C��(�|�C���g)�̃h���b�v�ʒm
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="angleY"></param>
    public void OnDropCoins(Vector3 startPoint, int[] anglesY, string[] coinNames, UserScore latestUserScore)
    {
        if (userState == USER_STATE.joined)
        {
            JoinedUsers[latestUserScore.ConnectionId].score = latestUserScore.LatestScore;
            OnDropCoinsUser(startPoint, anglesY, coinNames, latestUserScore);
        }
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �����ꏊ���قȂ�R�C��(�|�C���g)�̃h���b�v�ʒm
    /// </summary>
    /// <param name="startPoins"></param>
    /// <param name="coinNames"></param>
    public void OnDropCoinsAtRandomPositions(Vector3[] startPoins, string[] coinNames, UserScore latestUserScore)
    {
        if (userState == USER_STATE.joined)
        {
            JoinedUsers[latestUserScore.ConnectionId].score = latestUserScore.LatestScore;
            OnDropCoinsAtRandomPositionsUser(startPoins, coinNames, latestUserScore);
        }
    }

    /// <summary>
    /// �A�C�e���ɐG�ꂽ���ɌĂяo��
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="itemName"></param>
    /// <returns></returns>
    public async UniTask GetItemAsynk(EnumManager.ITEM_ID itemId, string itemName)
    {
        if (userState == USER_STATE.joined) await roomHub.GetItemAsynk(itemId, itemName);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �A�C�e���擾�ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemName"></param>
    /// <param name="option"></param>
    public void OnGetItem(Guid connectionId, string itemName, float option)
    {
        if (userState == USER_STATE.joined) OnGetItemUser(connectionId, itemName, option);
    }

    /// <summary>
    /// �A�C�e���g�p���ɌĂяo��
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async UniTask UseItemAsynk(EnumManager.ITEM_ID itemId)
    {
        if (userState == USER_STATE.joined) await roomHub.UseItemAsynk(itemId);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �A�C�e���g�p�ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    public void OnUseItem(Guid connectionId, EnumManager.ITEM_ID itemId)
    {
        if (userState == USER_STATE.joined) OnUseItemUser(connectionId, itemId);
    }

    /// <summary>
    /// �A�C�e���̔j��
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async UniTask DestroyItemAsynk(string itemName)
    {
        if (userState == USER_STATE.joined) await roomHub.DestroyItemAsynk(itemName);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �A�C�e���̔j���ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    public void OnDestroyItem(string itemName)
    {
        if (userState == USER_STATE.joined) OnDestroyItemUser(itemName);
    }

    /// <summary>
    /// �A�C�e���̐���
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async UniTask SpawnItemAsynk(Vector3 spawnPoint, EnumManager.ITEM_ID itemId)
    {
        if (userState == USER_STATE.joined) await roomHub.SpawnItemAsynk(spawnPoint, itemId);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �A�C�e���̐����ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    public void OnSpawnItem(Vector3 spawnPoint, EnumManager.ITEM_ID itemId, string itemName)
    {
        if (userState == USER_STATE.joined) OnSpawnItemUser(spawnPoint, itemId, itemName);
    }

    /// <summary>
    /// ���I�ȃI�u�W�F�N�g�𐶐�
    /// </summary>
    /// <param name="spawnObject"></param>
    /// <returns></returns>
    public async UniTask SpawnObjectAsynk(SpawnObject spawnObject)
    {
        if (userState == USER_STATE.joined) await roomHub.SpawnObjectAsynk(spawnObject);
    }

    /// <summary>
    /// ���I�ȃI�u�W�F�N�g�̐����ʒm
    /// </summary>
    /// <param name="spawnObject"></param>
    public void OnSpawnObject(SpawnObject spawnObject)
    {
        if (userState == USER_STATE.joined) OnSpawnObjectUser(spawnObject);
    }

    /// <summary>
    /// �����̃M�~�b�N��������
    /// </summary>
    /// <param name="animalName"></param>
    /// <param name="optionVec"></param>
    /// <returns></returns>
    public async UniTask PlayAnimalGimmickAsynk(EnumManager.ANIMAL_GIMMICK_ID animalId, string animalName, Vector3[] optionVec)
    {
        if (userState == USER_STATE.joined) await roomHub.PlayAnimalGimmickAsynk(animalId, animalName, optionVec);
    }

    /// <summary>
    /// �����̃M�~�b�N�����ʒm
    /// </summary>
    /// <param name="animalName"></param>
    /// <param name="optionVec"></param>
    public void OnPlayAnimalGimmick(EnumManager.ANIMAL_GIMMICK_ID animalId, string animalName, Vector3[] optionVec)
    {
        if (userState == USER_STATE.joined) OnPlayAnimalGimmickUser(animalId, animalName, optionVec);
    }
    #endregion

    #region ���Z�w�J���g���[�����[�x�̏���
    /// <summary>
    /// �A���̃M�~�b�N��j�����郊�N�G�X�g
    /// (�}�X�^�[�N���C�A���g���Ăяo��)
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    public async UniTask DestroyPlantsGimmickAsynk(string[] names)
    {
        if (userState == USER_STATE.joined) await this.roomHub.DestroyPlantsGimmickAsynk(names);
    }

    /// <summary>
    /// �A���̃M�~�b�N��j������ʒm
    /// </summary>
    /// <param name="names"></param>
    public void OnDestroyPlantsGimmick(string[] names)
    {
        if (userState == USER_STATE.joined) OnDestroyPlantsGimmickUser(names);
    }

    /// <summary>
    /// �A���̃M�~�b�N�𔭓����郊�N�G�X�g
    /// </summary>
    /// <returns></returns>
    public async UniTask TriggeringPlantGimmickAsynk(string name)
    {
        if (userState == USER_STATE.joined) await this.roomHub.TriggeringPlantGimmickAsynk(name);
    }

    /// <summary>
    /// �A���̃M�~�b�N�𔭓�����ʒm
    /// </summary>
    /// <param name="name"></param>
    public void OnTriggeringPlantGimmick(string name)
    {
        if (userState == USER_STATE.joined) OnTriggeringPlantGimmickUser(name);
    }

    /// <summary>
    /// �G���A���N���A��������
    /// </summary>
    /// <param name="isLastArea"></param>
    /// <returns></returns>
    public async UniTask AreaClearedAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.AreaClearedAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// ���݂̃G���A���N���A�����ʒm
    /// </summary>
    public void OnAreaCleared(Guid connectionId, string userName, bool isClearedAllUsers)
    {
        if (userState == USER_STATE.joined) OnAreaClearedUser(connectionId, userName, isClearedAllUsers);
    }

    /// <summary>
    /// ���̃G���A�Ɉړ����鏀����������������
    /// </summary>
    /// <returns></returns>
    public async UniTask ReadyNextAreaAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.ReadyNextAreaAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �S�������̃G���A�Ɉړ����鏀�������������ʒm (�Q�[���ĊJ�ʒm)
    /// </summary>
    /// <param name="restarningWaitSec"></param>
    public void OnReadyNextAreaAllUsers(float restarningWaitSec, EnumManager.RELAY_AREA_ID nextAreaId)
    {
        if (userState == USER_STATE.joined) OnReadyNextAreaUser(restarningWaitSec,nextAreaId);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// (�G���A�N���A���ȂǂɃ}�X�^�[�N���C�A���g����M)�J�E���g�_�E���J�n�ʒm
    /// </summary>
    /// <param name="restarningWaitSec"></param>
    public void OnStartCountDown()
    {
        if (userState == USER_STATE.joined) OnStartCountDownUser();
    }
    #endregion


    #region �Q�[���I���܂ł̏���(�ŏI���ʔ��\�V�[���̏���)

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �Ō�̋��Z���I�������ʒm
    /// </summary>
    public void OnAfterFinalGame()
    {
        if (userState == USER_STATE.joined) OnAfterFinalGameUser();
    }

    /// <summary>
    /// �ŏI���ʔ��\�V�[���ɑJ�ڂ�������
    /// </summary>
    /// <returns></returns>
    public async UniTask TransitionFinalResultSceneAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.TransitionFinalResultSceneAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �S�����J�ڂł����ʒm
    /// </summary>
    /// <param name="result"></param>
    public async void OnTransitionFinalResultSceneAllUsers(ResultData[] result, int ratingDelta)
    {
        if (userState == USER_STATE.joined)
        {
            OnTransitionFinalResultSceneUser(result);

            // ���[�e�B���O�X�VAPI
            await RatingModel.Instance.UpdateRatingAsync( UserModel.Instance.UserId, ratingDelta);
        }
    }
    #endregion
}
