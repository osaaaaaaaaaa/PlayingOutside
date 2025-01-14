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
#if UNITY_EDITOR
        if (UserModel.Instance.UserId == 0)
        {
            // ユーザー情報取得処理
            bool isSucsess = UserModel.Instance.LoadUserData();
            int userId = isSucsess ? UserModel.Instance.UserId : 1;
            var error = await UserModel.Instance.ShowUserAsync(userId);
            if (error != null)
            {
                ErrorUIController.Instance.ShowErrorUI(error);
                return;
            }
        }
#endif
        if (UserModel.Instance.UserId != 0)
        {
            userNameText.text = UserModel.Instance.UserName;
            await RatingModel.Instance.ShowRatingAsync(UserModel.Instance.UserId);
        }
        
       if(NGWordModel.Instance.NGWords == null)
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