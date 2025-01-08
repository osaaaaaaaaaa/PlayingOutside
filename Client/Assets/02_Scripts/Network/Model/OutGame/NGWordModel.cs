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

    // �C���X�^���X�쐬
    private static NGWordModel instance;
    public static NGWordModel Instance
    {
        get
        {
            // GET�v���p�e�B���Ă΂ꂽ�Ƃ��ɃC���X�^���X���쐬����(����̂�)
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
    /// NG���[�h�擾API
    /// </summary>
    /// <returns></returns>
    public async UniTask<string[]> ShowNGWordAsync()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<INGWordService>(channel);

        try
        {
            // �擾����
            var response = await client.ShowNGWordAsync();
            NGWords = new List<string>(response);
            NGWords.Add(" ");
            return NGWords.ToArray();
        }
        catch (RpcException e)
        {
            // �擾���s
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
