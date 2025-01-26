using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using Server.Model.Entity;
using Shared.Interfaces.Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingModel : BaseModel
{
    // �C���X�^���X�쐬
    private static RatingModel instance;
    public static RatingModel Instance
    {
        get
        {
            // GET�v���p�e�B���Ă΂ꂽ�Ƃ��ɃC���X�^���X���쐬����(����̂�)
            if (instance == null)
            {
                GameObject gameObj = new GameObject("RatingModel");
                instance = gameObj.AddComponent<RatingModel>();
                DontDestroyOnLoad(gameObj);
            }
            return instance;
        }
    }

    public int Rating { get; private set; } = 0;

    /// <summary>
    /// �S���[�U�[��Ώۂɂ��������L���O�擾API
    /// </summary>
    /// <returns></returns>
    public async UniTask<RatingRanking[]> ShowGlobalRatingRanking()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IRatingService>(channel);

        try
        {
            // �擾����
            return await client.ShowGlobalRatingRanking();
        }
        catch (RpcException e)
        {
            // �擾���s
            Debug.Log(e);
            ErrorUIController.Instance.ShowErrorUI("�����L���O�̎擾�Ɏ��s���܂����B");
            return null;
        }
    }

    /// <summary>
    /// �t�H���[���Ă��郆�[�U�[��Ώۂɂ��������L���O�擾API
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask<RatingRanking[]> ShowFollowedUsersRatingRanking(int userId)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IRatingService>(channel);

        try
        {
            // �擾����
            return await client.ShowFollowedUsersRatingRanking(userId);
        }
        catch (RpcException e)
        {
            // �擾���s
            Debug.Log(e);
            ErrorUIController.Instance.ShowErrorUI("�����L���O�̎擾�Ɏ��s���܂����B");
            return null;
        }
    }

    /// <summary>
    /// ���[�e�B���O�X�VAPI
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="addRating"></param>
    /// <returns></returns>
    public async UniTask UpdateRatingAsync(int userId, int ratingDelta)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IRatingService>(channel);

        try
        {
            // �擾����
            await client.UpdateRatingAsync(userId, ratingDelta);
        }
        catch (RpcException e)
        {
            // �擾���s
            Debug.Log(e);
        }
    }

    /// <summary>
    /// ���[�e�B���O�擾API
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask<int> ShowRatingAsync(int userId)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IRatingService>(channel);

        try
        {
            // �擾����
            this.Rating = await client.ShowRatingAsync(userId);
            return this.Rating;
        }
        catch (RpcException e)
        {
            // �擾���s
            Debug.Log(e);
            return 0;
        }
    }
}
