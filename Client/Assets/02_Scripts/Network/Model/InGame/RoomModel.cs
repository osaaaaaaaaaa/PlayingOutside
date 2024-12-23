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

    #region �Q�[���J�n�܂ł̏���
    // �����������������ǂ����̒ʐM
    public Action<int,bool> OnReadyUser { get; set; }
    // �S�����Q�[���J�n�O�̃J�E���g�_�E���I���ʒm
    public Action OnCountdownOverUser { get; set; }
    #endregion

    #region �Q�[�����ʂ̏���
    // �J�E���g�_�E���ʒm
    public Action<int> OnCountDownUser { get; set; }
    // �S���̃Q�[���I�����������������ʒm
    public Action<string> OnFinishGameUser { get; set; }
    // �R�C���̃h���b�v�ʒm
    public Action<Vector3, int[], string[]> OnDropCoinsUser { get; set; }
    // �����ꏊ���قȂ�R�C���̃h���b�v�ʒm
    public Action<Vector3[], string[]> OnDropCoinsAtRandomPositionsUser { get; set; }
    // �A�C�e���擾�ʒm
    public Action<Guid, string,float> OnGetItemUser {  get; set; }
    #endregion

    #region ���Z�w�J���g���[�����[�x�̏���
    // ���݂̃G���A���N���A�����ʒm
    public Action<Guid,string,bool> OnAreaClearedUser { get; set; }
    // �S�������̃G���A�Ɉړ����鏀�������������ʒm (�Q�[���ĊJ�ʒm)
    public Action<float> OnReadyNextAreaUser { get; set; }
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
        if(roomHub != null) await roomHub.DisposeAsync();
        if(channel != null) await channel.ShutdownAsync();
        roomHub = null;channel = null;

        userState = USER_STATE.disconnect;
    }

    /// <summary>
    /// �j�������(�A�v���I�����Ȃ�)�ɃT�[�o�[�Ƃ̐ڑ���ؒf
    /// </summary>
    private async void OnDestroy()
    {
        await DisconnectAsync();
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
        Debug.Log("���[�U�[��" + users.Length);

        if (users == null)
        {
            await DisconnectAsync();
            // �����Ɏ��s�����ꍇ��TopScene�ɖ߂�
            SceneManager.LoadScene("TopScene");
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
        if(userState == USER_STATE.joined) await LeaveAsync();  // 4�Ԗڂ̃��[�U�[�ȊO����������͂�
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
            SceneManager.LoadScene("TopScene");
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
                await OnReadyAsynk(true);
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
    public void OnLeave(Guid connectionId)
    {
        if (userState == USER_STATE.leave_done) return;

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
        if (userState != USER_STATE.leave && userState != USER_STATE.leave_done) 
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
        if (userState != USER_STATE.leave && userState != USER_STATE.leave_done) 
        {
            OnUpdatePlayerStateUser(connectionId, playerState); 
        }
    }

    #region �Q�[���J�n�܂ł̏���
    /// <summary>
    /// �����̏����������������ǂ���
    /// </summary>
    /// <returns></returns>
    public async UniTask OnReadyAsynk(bool isReady)
    {
        Debug.Log("�����������N�G�X�g�𑗐M");
        // �T�[�o�[�ɏ����������������ǂ��������N�G�X�g
        await roomHub.OnReadyAsynk(isReady);
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
    public async UniTask OnCountdownOverAsynk()
    {
        // �T�[�o�[�ɃJ�E���g�_�E�����I���������Ƃ����N�G�X�g
        await roomHub.OnCountdownOverAsynk();
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
    /// �J�E���g�_�E������
    /// (�}�X�^�[�N���C�A���g������)
    /// </summary>
    /// <param name="currentTime"></param>
    /// <returns></returns>
    public async UniTask OnCountDownAsynk(int currentTime)
    {
        if (userState == USER_STATE.joined) await roomHub.OnCountDownAsynk(currentTime);
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
    public async UniTask OnFinishGameAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.OnFinishGameAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �S�����Q�[���I�����������������ʒm
    /// </summary>
    /// <param name="scene"></param>
    public void OnFinishGame(GameScene scene)
    {
        if (userState == USER_STATE.joined)
        {
            string sceneName = "";
            switch (scene.GameSceneId)
            {
                case GameScene.SCENE_ID.RelayGame:
                    sceneName = "RelayGameScene";
                    break;
                case GameScene.SCENE_ID.FinalGame:
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
    public async UniTask OnKnockDownAsynk(Vector3 startPoint)
    {
        if (userState == USER_STATE.joined) await roomHub.OnKnockDownAsynk(startPoint);
    }

    /// <summary>
    /// ��O�ɏo�����ɌĂяo��
    /// </summary>
    /// <param name="rangePointA"></param>
    /// <param name="rangePointB"></param>
    /// <returns></returns>
    public async UniTask OnOutOfBoundsAsynk(Vector3 rangePointA, Vector3 rangePointB)
    {
        if (userState == USER_STATE.joined) await roomHub.OnOutOfBoundsAsynk(rangePointA, rangePointB);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �R�C��(�|�C���g)�̃h���b�v�ʒm
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="angleY"></param>
    public void OnDropCoins(Vector3 startPoint, int[] anglesY, string[] coinNames)
    {
        if (userState == USER_STATE.joined) OnDropCoinsUser(startPoint, anglesY, coinNames);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �����ꏊ���قȂ�R�C��(�|�C���g)�̃h���b�v�ʒm
    /// </summary>
    /// <param name="startPoins"></param>
    /// <param name="coinNames"></param>
    public void OnDropCoinsAtRandomPositions(Vector3[] startPoins, string[] coinNames)
    {
        if (userState == USER_STATE.joined) OnDropCoinsAtRandomPositionsUser(startPoins, coinNames);
    }

    /// <summary>
    /// �A�C�e���ɐG�ꂽ���ɌĂяo��
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="itemName"></param>
    /// <returns></returns>
    public async UniTask OnGetItemAsynk(Item.ITEM_ID itemId, string itemName)
    {
        if (userState == USER_STATE.joined) await roomHub.OnGetItemAsynk(itemId, itemName);
    }

    /// <summary>
    /// �A�C�e���擾�ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemName"></param>
    /// <param name="option"></param>
    public void OnGetItem(Guid connectionId, string itemName, float option)
    {
        if (userState == USER_STATE.joined) OnGetItemUser(connectionId, itemName, option);
    }
    #endregion

    #region ���Z�w�J���g���[�����[�x�̏���
    /// <summary>
    /// �G���A���N���A��������
    /// </summary>
    /// <param name="isLastArea"></param>
    /// <returns></returns>
    public async UniTask OnAreaClearedAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.OnAreaClearedAsynk();
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
    public async UniTask OnReadyNextAreaAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.OnReadyNextAreaAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �S�������̃G���A�Ɉړ����鏀�������������ʒm (�Q�[���ĊJ�ʒm)
    /// </summary>
    /// <param name="restarningWaitSec"></param>
    public void OnReadyNextAreaAllUsers(float restarningWaitSec)
    {
        if (userState == USER_STATE.joined) OnReadyNextAreaUser(restarningWaitSec);
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
    public async UniTask OnTransitionFinalResultSceneAsynk()
    {
        if (userState == USER_STATE.joined) await roomHub.OnTransitionFinalResultSceneAsynk();
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// �S�����J�ڂł����ʒm
    /// </summary>
    /// <param name="result"></param>
    public void OnTransitionFinalResultSceneAllUsers(ResultData[] result)
    {
        if (userState == USER_STATE.joined) OnTransitionFinalResultSceneUser(result);
    }
    #endregion
}
