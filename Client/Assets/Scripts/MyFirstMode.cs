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
        // Callback�o�[�W����
        Sum(100, 323, result =>
        {
            Debug.Log(result);
        });

        // UniTask�o�[�W����
        int result = await Sum(200, 200);
        Debug.Log(result);

        int resultSub = await Sub(1000, 200);
        Debug.Log(resultSub);
    }

    /// <summary>
    /// Callback�o�[�W����
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="callback"></param>
    public async void Sum(int x, int y, Action<int> callback)
    {
        using var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(
            ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IMyFirstService>(channel);
        var result = await client.SumAsync(x, y);   // ���ʂ���M
        callback?.Invoke(result);
    }

    /// <summary>
    /// UniTask�o�[�W����
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="callback"></param>
    public async UniTask<int> Sum(int x, int y)
    {
        using var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(
            ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IMyFirstService>(channel);
        var result = await client.SumAsync(x, y);   // ���ʂ���M
        return result;
    }

    public async UniTask<int> Sub(int x, int y)
    {
        using var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(
            ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IMyFirstService>(channel);
        var result = await client.SubAsync(x, y);   // ���ʂ���M
        return result;
    }
}
