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

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannel channel;
    private IRoomHub roomHub;

    // �ڑ�ID
    public Guid ConnectionId { get; private set; }
    // �ڑ����郋�[����
    string connectionRoomName;
    public string ConnectionRoomName {  get { return connectionRoomName; } set { connectionRoomName = value; } }
    // DB����擾���������̃��[�U�[���
    User myUserData;
    public User MyUserData { get { return myUserData; }set { myUserData = value; } }
    // �Q�����Ă��郆�[�U�[�̏��
    public Dictionary<Guid,JoinedUser> JoinedUsers { get; private set; } = new Dictionary<Guid,JoinedUser>();

    #region �T�[�o�[����Ă΂��Action�֐�
    // ���[�U�[�ڑ��ʒm
    public Action<JoinedUser> OnJoinedUser { get; set; }    // �T�[�o�[����ʒm���͂����ۂɁAAction�^�ɓo�^����Ă���֐����Ăяo��
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

    #region ���Z�w�J���g���[�����[�x�̏���
    // ���݂̃G���A���N���A�����ʒm
    public Action<Guid,string,bool> OnAreaClearedUser { get; set; }
    // �S�������̃G���A�Ɉړ����鏀�������������ʒm (�Q�[���ĊJ�ʒm)
    public Action<float> OnReadyNextAreaUser { get; set; }
    // �J�E���g�_�E���J�n�ʒm
    public Action OnStartCountDownUser { get; set; }
    // �J�E���g�_�E���ʒm
    public Action<int> OnCountDownUser { get; set; }
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
    private void OnDestroy()
    {
        DisconnectAsync();
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask JoinAsync(string roomName, int userId)
    {
        JoinedUser[] users = await roomHub.JoinAsynk(roomName, userId);
        foreach (JoinedUser user in users)
        {
            JoinedUsers.Add(user.ConnectionId, user);
            if (user.UserData.Id == userId) this.ConnectionId = user.ConnectionId;  // ���g�̐ڑ�ID��T���ĕۑ�����
            OnJoinedUser(user); // �A�N�V�����Ń��f�����g���N���X�ɒʒm
        }

        userState = USER_STATE.joined;
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// ���̃��[�U�[�����������ʒm
    /// </summary>
    /// <param name="user"></param>
    public void OnJoin(JoinedUser user)
    {
        // �A�N�V�������s
        JoinedUsers.Add(user.ConnectionId, user);
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
        JoinedUsers.Clear();

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
            DisconnectAsync();
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

        // �T�[�o�[�ɏ����������������ǂ��������N�G�X�g
        await roomHub.OnReadyAsynk(isReady);
    }

    /// <summary>
    /// [IRoomHubReceiver�̃C���^�[�t�F�C�X]
    /// ���������������ǂ����̒ʒm
    /// </summary>
    public void OnReady(int readyCnt, bool isTransitionGameScene)
    {
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
    public async UniTask OnReadyNextAreaAsynk(bool isLastArea)
    {
        if (userState == USER_STATE.joined) await roomHub.OnReadyNextAreaAsynk(isLastArea);
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
    /// �J�E���g�_�E���J�n�ʒm
    /// </summary>
    /// <param name="restarningWaitSec"></param>
    public void OnStartCountDown()
    {
        if (userState == USER_STATE.joined) OnStartCountDownUser();
    }

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
