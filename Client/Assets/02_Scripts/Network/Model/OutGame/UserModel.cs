using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Shared.Interfaces.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UserModel : BaseModel
{
    private int userId; // �o�^���[�U�[ID
    public async UniTask<bool> RegistUserAsync(string name)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IUserService>(channel);
        try
        {
            // �o�^����
            userId = await client.RegistUserAsync(name);
            return true;
        }
        catch(RpcException e)
        {
            // �o�^���s
            Debug.Log(e);
            return false;
        }
    }
}
