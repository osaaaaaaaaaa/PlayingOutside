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
    [SerializeField] TopSceneCharacterManager characterManager;
    [SerializeField] Text userNameText;

    private async void Awake()
    {
        if (UserModel.Instance.UserId != 0)
        {
            userNameText.text = UserModel.Instance.UserName;
            await RatingModel.Instance.ShowRatingAsync(UserModel.Instance.UserId);
        }
        
       if(NGWordModel.Instance.NGWords.Count == 0)
       {
           await NGWordModel.Instance.ShowNGWordAsync();
       }

        characterManager.ToggleCharacter(UserModel.Instance.CharacterId);
        if (SceneControler.Instance != null) SceneControler.Instance.StopSceneLoad();
    }

    public void OnJoinRoomButton(string roomName)
    {
        RoomModel.Instance.ConnectionRoomName = roomName;
        SceneControler.Instance.StartSceneLoad("RoomScene");
    }

    public void UpdateUserNameText()
    {
        userNameText.text = UserModel.Instance.UserName;
    }
}