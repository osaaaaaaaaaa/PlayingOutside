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

public class FollowModel : BaseModel
{
    // インスタンス作成
    private static FollowModel instance;
    public static FollowModel Instance
    {
        get
        {
            // GETプロパティを呼ばれたときにインスタンスを作成する(初回のみ)
            if (instance == null)
            {
                GameObject gameObj = new GameObject("FollowModel");
                instance = gameObj.AddComponent<FollowModel>();
                DontDestroyOnLoad(gameObj);
            }
            return instance;
        }
    }

    /// <summary>
    /// ユーザー情報取得API
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async UniTask<FollowingUser> ShowUserByNameAsync(string name)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IFollowService>(channel);

        try
        {
            // 取得成功
            return await client.ShowUserByNameAsync(name);
        }
        catch (RpcException e)
        {
            // 取得失敗
            Debug.Log(e);
            Debug.Log(e.Status.Detail);
            return null;
        }
    }

    /// <summary>
    /// フォローしているユーザー一覧取得API
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async UniTask<FollowingUser[]> ShowFollowingUsersAsynk(int userId)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IFollowService>(channel);

        try
        {
            // 取得成功
            return await client.ShowFollowingUsersAsynk(userId);
        }
        catch (RpcException e)
        {
            // 取得失敗
            Debug.Log(e);
            Debug.Log(e.Status.Detail);
            return null;
        }
    }

    /// <summary>
    /// フォロー登録API
    /// </summary>
    /// <param name="follow"></param>
    /// <returns></returns>
    public async UniTask RegistFollowAsync(int followingId, int followeeId)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IFollowService>(channel);

        try
        {
            await client.RegistFollowAsync(followingId,followeeId);
        }
        catch (RpcException e)
        {
            Debug.Log(e);
            Debug.Log(e.Status.Detail);
        }
    }

    /// <summary>
    /// フォロー解除API
    /// </summary>
    /// <param name="follow"></param>
    /// <returns></returns>
    public async UniTask RemoveFollowAsync(int followingId, int followeeId)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IFollowService>(channel);

        try
        {
            await client.RemoveFollowAsync(followingId, followeeId);
        }
        catch (RpcException e)
        {
            Debug.Log(e);
            Debug.Log(e.Status.Detail);
        }
    }
}
