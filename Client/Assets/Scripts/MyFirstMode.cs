using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion.Client;
using Shared.Interfaces.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MyFirstMode : MonoBehaviour
{
    const string ServerURL = "http://localhost:7000";

    private async void Start()
    {
        // Callbackバージョン
        Sum(100, 323, result =>
        {
            Debug.Log(result);
        });

        // UniTaskバージョン
        int result = await Sum(200, 200);
        Debug.Log(result);

        int resultSub = await Sub(1000, 200);
        Debug.Log(resultSub);
    }

    /// <summary>
    /// Callbackバージョン
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="callback"></param>
    public async void Sum(int x, int y, Action<int> callback)
    {
        using var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(
            ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IMyFirstService>(channel);
        var result = await client.SumAsync(x, y);   // 結果を受信
        callback?.Invoke(result);
    }

    /// <summary>
    /// UniTaskバージョン
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="callback"></param>
    public async UniTask<int> Sum(int x, int y)
    {
        using var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(
            ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IMyFirstService>(channel);
        var result = await client.SumAsync(x, y);   // 結果を受信
        return result;
    }

    public async UniTask<int> Sub(int x, int y)
    {
        using var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(
            ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IMyFirstService>(channel);
        var result = await client.SubAsync(x, y);   // 結果を受信
        return result;
    }
}
