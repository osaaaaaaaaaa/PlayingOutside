using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class GameDirector : MonoBehaviour
{
    [SerializeField] InputField userIdField;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] RoomModel roomModel;
    Dictionary<Guid,GameObject> characterList = new Dictionary<Guid,GameObject>();  // ユーザーのキャラクター情報

    private async void Start()
    {
        // ユーザーが入室したときにthis.OnJoinedUserメソッドを実行するようにする
        roomModel.OnJoinedUser += this.OnJoinedUser;

        // 接続処理
        await roomModel.ConnectAsync();
    }

    /// <summary>
    /// 入室処理
    /// </summary>
    /// <param name="strId"></param>
    public async void JoinRoom()
    {
        int id = int.Parse(userIdField.text);

        // 入室処理[ルーム名,ユーザーID(最終的にはローカルに保存してあるIDを使う)]
        await roomModel.JoinAsync("sampleRoom", id);
    }

    /// <summary>
    /// ユーザー入室通知処理
    /// </summary>
    /// <param name="user"></param>
    void OnJoinedUser(JoinedUser user)
    {
        // キャラクター生成,
        GameObject caracterObject = Instantiate(characterPrefab);
        caracterObject.transform.position = Vector3.zero;
        characterList[user.ConnectionId] = caracterObject;
    }
}
