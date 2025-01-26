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
    #region ���[�U�[���
    public string AuthToken { get; private set; } = "";
    public int UserId { get; private set; } = 0;
    public string UserName { get; private set; } = "";
    public int CharacterId { get; private set; } = 0;
    #endregion

    // �C���X�^���X�쐬
    private static UserModel instance;
    public static UserModel Instance
    {
        get
        {
            // GET�v���p�e�B���Ă΂ꂽ�Ƃ��ɃC���X�^���X���쐬����(����̂�)
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
    /// ���[�U�[�������[�J���ɕۑ�����
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
        // Application.persistentDataPath��OS���ŕۑ��ꏊ���Œ肳��Ă���
        var writer = new StreamWriter(Application.persistentDataPath + "/saveData.json");
        writer.Write(json);
        writer.Flush();     // �����ɏ����o���悤���߂���
        writer.Close();
    }

    /// <summary>
    /// ���[�U�[�������[�J������ǂݍ���
    /// </summary>
    public bool LoadUserData()
    {
        // �t�@�C���̑��݃`�F�b�N
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
    /// ���[�U�[�o�^API
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async UniTask<string> RegistUserAsync(string name)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IUserService>(channel);
        try
        {
            // �o�^����
            var user = await client.RegistUserAsync(name);
            AuthToken = user.Token;
            UserId = user.Id;
            UserName = user.Name;
            CharacterId = user.Character_Id;

            // ���[�J���ɕۑ�
            SaveUserData();

            return null;
        }
        catch(RpcException e)
        {
            // �o�^���s
            Debug.Log(e);
            return "���[�U�[�̓o�^�Ɏ��s���܂����B�ʐM���̗ǂ����ŗV�Ԃ��A�A�v�����ċN�����Ă��������B";
        }
    }

    /// <summary>
    /// ���[�U�[���擾API
    /// </summary>
    /// <returns></returns>
    public async UniTask<string> ShowUserAsync(int userId)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            // �擾����
            var user = await client.ShowUserAsync(userId);
            AuthToken = user.Token;
            UserId = user.Id;
            UserName = user.Name;
            CharacterId = user.Character_Id;

            return null;
        }
        catch (RpcException e)
        {
            // �擾���s
            Debug.Log(e);
            return e.Status.Detail;
        }

    }

    /// <summary>
    /// ���[�U�[���X�VAPI
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async UniTask<string> UpdateUserAsync(User request)
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler }); // �ʐM�o�H�쐬
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            // �擾����
            await client.UpdateUserAsync(request);
            UserName = request.Name;
            CharacterId = request.Character_Id;
            return null;
        }
        catch (RpcException e)
        {
            // �擾���s
            Debug.Log(e);
            return "���[�U�[�����X�V�ł��܂���ł����B";
        }
    }
}
