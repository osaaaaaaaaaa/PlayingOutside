//*********************************************************
// NGワードを管理するモデル
// Author:Rui Enomoto
//*********************************************************
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Server.Model.Entity;
using Shared.Interfaces.Services;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class NGWordModel : BaseModel
{
    public List<string> NGWords { get; private set; } = new List<string>();

    // インスタンス作成
    private static NGWordModel instance;
    public static NGWordModel Instance
    {
        get
        {
            // GETプロパティを呼ばれたときにインスタンスを作成する(初回のみ)
            if (instance == null)
            {
                GameObject gameObj = new GameObject("NGWordModel");
                instance = gameObj.AddComponent<NGWordModel>();
                DontDestroyOnLoad(gameObj);
            }
            return instance;
        }
    }

    /// <summary>
    /// NGワード取得API
    /// </summary>
    /// <returns></returns>
    public async UniTask<string[]> ShowNGWordAsync()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<INGWordService>(channel);

        try
        {
            // 取得成功
            var response = await client.ShowNGWordAsync();
            NGWords = new List<string>(response);
            NGWords.Add(" ");
            return NGWords.ToArray();
        }
        catch (RpcException e)
        {
            // 取得失敗
            Debug.Log(e.Status.Detail);
            return null;
        }

    }

    public bool ContainsNGWord(string text)
    {
        if(NGWords == null) return true;
        foreach (var ngWord in NGWords)
        {
            if (text.Contains(ngWord))
            {
                return true;
            }
        }
        return false;
    }
}
