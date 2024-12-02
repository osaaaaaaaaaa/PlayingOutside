using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;

public class TopSceneDirector : MonoBehaviour
{
    private void Start()
    {
        SceneControler.Instance.StopSceneLoad();
    }

    public void OnJoinRoomButton(string roomName)
    {
        RoomModel.Instance.ConnectionRoomName = roomName;
        SceneControler.Instance.StartSceneLoad("RoomScene");
    }
}
