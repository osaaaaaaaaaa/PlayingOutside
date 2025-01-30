using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Newtonsoft.Json;
using Server.Model.Entity;
using Shared.Interfaces.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class UserModel : BaseModel
{
    #region ユーザー情報
    public string AuthToken { get; private set; } = "";
    public int UserId { get; private set; } = 0;
    public string UserName { get; private set; } = "";
    public int CharacterId { get; private set; } = 0;
    #endregion

    // インスタンス作成
    private static UserModel instance;
    public static UserModel Instance
    {
        get
        {
            // GETプロパティを呼ばれたときにインスタンスを作成する(初回のみ)
            if (instance == null)
            {
                GameObject gameObj = new GameObject("UserModel");
                instance = gameObj.AddComponent<UserModel>();
                DontDestroyOnLoad(gameObj);
            }
            return instance;
        }
    }

    /// <summary>
    /// ユーザー情報をローカルに保存する
    /// </summary>
    public void SaveUserData()
    {
        SaveData saveData = new SaveData();
        saveData.AuthToken = this.AuthToken;
        saveData.Name = this.UserName;
        saveData.UserID = this.UserId;
        saveData.BGMVolume = AudioVolume.BgmVolume;
        saveData.SEVolume = AudioVolume.SeVolume;

        string json = JsonConvert.SerializeObject(saveData);
        // Application.persistentDataPathはOS毎で保存場所が固定されている
        var writer = new StreamWriter(Application.persistentDataPath + "/saveData.json");
        writer.Write(json);
        writer.Flush();     // すぐに書き出すよう命令する
        writer.Close();
    }

    /// <summary>
    /// ユーザー情報をローカルから読み込む
    /// </summary>
    public bool LoadUserData()
    {
        // ファイルの存在チェック
        if (!File.Exists(Application.persistentDataPath + "/saveData.json")) return false;

        var reader = new StreamReader(Application.persistentDataPath + "/saveData.json");
        string json = reader.ReadToEnd();
        reader.Close();
        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);
        this.UserId = saveData.UserID;
        this.UserName = saveData.Name;
        this.AuthToken = saveData.AuthToken;
        AudioVolume.BgmVolume = saveData.BGMVolume;
        AudioVolume.SeVolume = saveData.SEVolume;
        return true;
    }

    /// <summary>
    /// ユーザー登録API
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async UniTask<string> RegistUserAsync(string name)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IUserService>(channel);
        try
        {
            // 登録成功
            var user = await client.RegistUserAsync(name);
            AuthToken = user.Token;
            UserId = user.Id;
            UserName = user.Name;
            CharacterId = user.Character_Id;

            // ローカルに保存
            SaveUserData();

            return null;
        }
        catch(RpcException e)
        {
            // 登録失敗
            Debug.Log(e);
            return "ユーザーの登録に失敗しました。通信環境の良い所で遊ぶか、アプリを再起動してください。";
        }
    }

    /// <summary>
    /// ユーザー情報取得API
    /// </summary>
    /// <returns></returns>
    public async UniTask<string> ShowUserAsync(int userId)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            // 取得成功
            var user = await client.ShowUserAsync(userId);
            AuthToken = user.Token;
            UserId = user.Id;
            UserName = user.Name;
            CharacterId = user.Character_Id;

            return null;
        }
        catch (RpcException e)
        {
            // 取得失敗
            Debug.Log(e);
            return e.Status.Detail;
        }

    }

    /// <summary>
    /// ユーザー情報更新API
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async UniTask<string> UpdateUserAsync(User request)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // 通信経路作成
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            // 取得成功
            await client.UpdateUserAsync(request);
            UserName = request.Name;
            CharacterId = request.Character_Id;
            return null;
        }
        catch (RpcException e)
        {
            // 取得失敗
            Debug.Log(e);
            return "ユーザー情報を更新できませんでした。";
        }
    }
}
