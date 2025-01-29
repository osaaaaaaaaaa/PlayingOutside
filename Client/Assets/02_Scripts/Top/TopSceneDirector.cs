using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Server.Model.Entity;

public class TopSceneDirector : MonoBehaviour
{
    private async void Awake()
    {
       if(NGWordModel.Instance.NGWords.Count == 0)
       {
           await NGWordModel.Instance.ShowNGWordAsync();
       }

       if (SceneControler.Instance != null) SceneControler.Instance.StopSceneLoad();
    }

    public void OnJoinRoomButton(string roomName)
    {
        RoomModel.Instance.ConnectionRoomName = roomName;
        SceneControler.Instance.StartSceneLoad("RoomScene");
    }
}