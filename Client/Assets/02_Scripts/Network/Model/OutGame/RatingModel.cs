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
    // インスタンス作成
    private static RatingModel instance;
    public static RatingModel Instance
    {
        get
        {
            // GETプロパティを呼ばれたときにインスタンスを作成する(初回のみ)
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
    /// 全ユーザーを対象にしたランキング取得API
    /// </summary>
    /// <returns></returns>
    public async UniTask<RatingRanking[]> ShowGlobalRatingRanking()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IRatingService>(channel);

        try
        {
            // 取得成功
            return await client.ShowGlobalRatingRanking();
        }
        catch (RpcException e)
        {
            // 取得失敗
            Debug.Log(e);
            ErrorUIController.Instance.ShowErrorUI("ランキングの取得に失敗しました。");
            return null;
        }
    }

    /// <summary>
    /// フォローしているユーザーを対象にしたランキング取得API
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask<RatingRanking[]> ShowFollowedUsersRatingRanking(int userId)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IRatingService>(channel);

        try
        {
            // 取得成功
            return await client.ShowFollowedUsersRatingRanking(userId);
        }
        catch (RpcException e)
        {
            // 取得失敗
            Debug.Log(e);
            ErrorUIController.Instance.ShowErrorUI("ランキングの取得に失敗しました。");
            return null;
        }
    }

    /// <summary>
    /// レーティング更新API
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="addRating"></param>
    /// <returns></returns>
    public async UniTask UpdateRatingAsync(int userId, int ratingDelta)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IRatingService>(channel);

        try
        {
            // 取得成功
            await client.UpdateRatingAsync(userId, ratingDelta);
        }
        catch (RpcException e)
        {
            // 取得失敗
            Debug.Log(e);
        }
    }

    /// <summary>
    /// レーティング取得API
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask<int> ShowRatingAsync(int userId)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IRatingService>(channel);

        try
        {
            // 取得成功
            this.Rating = await client.ShowRatingAsync(userId);
            return this.Rating;
        }
        catch (RpcException e)
        {
            // 取得失敗
            Debug.Log(e);
            return 0;
        }
    }
}
