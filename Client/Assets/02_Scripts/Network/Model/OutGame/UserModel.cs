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
    private int userId; // ìoò^ÉÜÅ[ÉUÅ[ID
    public async UniTask<bool> RegistUserAsync(string name)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // í êMåoòHçÏê¨
        var client = MagicOnionClient.Create<IUserService>(channel);
        try
        {
            // ìoò^ê¨å˜
            userId = await client.RegistUserAsync(name);
            return true;
        }
        catch(RpcException e)
        {
            // ìoò^é∏îs
            Debug.Log(e);
            return false;
        }
    }
}
