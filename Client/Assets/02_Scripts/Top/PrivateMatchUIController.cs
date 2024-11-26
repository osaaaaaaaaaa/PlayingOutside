using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using Server.Model.Entity;

public class PrivateMatchUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion

    #region 初めに表示されるメニュー
    [SerializeField] GameObject menu;
    [SerializeField] InputField inputFieldRoomName;
    [SerializeField] GameObject buttonJoinRoom;
    #endregion

    #region ルーム待機中のUI関係
    [SerializeField] GameObject roomUsers;
    [SerializeField] Text roomName;
    [SerializeField] List<GameObject> objIconImages;
    [SerializeField] List<Text> textUserNames;
    [SerializeField] List<GameObject> objYouImages;
    [SerializeField] GameObject buttonGameStart;
    #endregion
    [SerializeField] TopSceneDirector topSceneDirector;
    TopSceneUIManager topSceneUIManager;

    private void Start()
    {
        InitUI();

        foreach (var ui in uiList)
        {
            ui.transform.localScale = Vector3.zero;
        }
        topSceneUIManager = GetComponent<TopSceneUIManager>();
    }

    private void Update()
    {
        // ルームに参加できた && ルームのUIを表示する場合は
        if(RoomModel.Instance.userState == RoomModel.USER_STATE.joined 
            && menu.activeSelf
            && !roomUsers.activeSelf)
        {
            menu.SetActive(false);
            roomUsers.SetActive(true);
        }
    }

    public void InitUI()
    {
        menu.SetActive(true);
        roomUsers.SetActive(false);
        buttonGameStart.SetActive(false);
        inputFieldRoomName.text = "";
        foreach (var icon in objIconImages) 
        {
            icon.SetActive(false);
        }
        foreach (var text in textUserNames)
        {
            text.text = "EMPTY";
        }
        foreach(var img in objYouImages)
        {
            img.SetActive(false);
        }
    }

    void ToggleUIVisibility(bool isVisibility)
    {
        Vector3 endScale = isVisibility ? Vector3.one : Vector3.zero;
        Ease setEase = isVisibility ? Ease.OutBack : Ease.InBack;

        foreach (var ui in uiList)
        {
            ui.transform.DOScale(endScale, 0.2f).SetEase(setEase);
        }
    }

    /// <summary>
    /// プライベートマッチUIを表示するボタン
    /// </summary>
    public void OnSelectButton()
    {
        InitUI();
        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
    }

    /// <summary>
    /// UIを一つ前に戻るボタン
    /// </summary>
    public void OnBackButton()
    {
        if (menu.activeSelf)
        {
            // メニュー画面の場合、プライベートマッチUIを全て閉じる
            ToggleUIVisibility(false);
            topSceneUIManager.OnBackButton();
        }
        else
        {
            // 退室処理をリクエスト
            topSceneDirector.LeaveRoom();
        }
    }

    /// <summary>
    /// 参加するボタン
    /// </summary>
    public void OnJoinButton()
    {
        if (inputFieldRoomName.text == "") return;

        // 入室処理をリクエスト
        topSceneDirector.JoinRoom(inputFieldRoomName.text);
        roomName.text = inputFieldRoomName.text;
    }

    /// <summary>
    /// 参加中のユーザーUIを設定
    /// </summary>
    /// <param name="joinOrder"></param>
    /// <param name="isMyData"></param>
    /// <param name="user"></param>
    public void SetupUserUI(bool isMyData, JoinedUser user)
    {
        Debug.Log(isMyData + ","+ user.UserData.Name + "," + (user.JoinOrder - 1) + "番目");
        objIconImages[user.JoinOrder - 1].SetActive(true);
        objIconImages[user.JoinOrder - 1].GetComponent<Image>().sprite = topSceneUIManager.SpriteIcons[1 - 1];  // 後でユーザーデータにキャラクターIDを作る
        textUserNames[user.JoinOrder - 1].text = user.UserData.Name;

        // 自分のデータの場合
        if (isMyData) objYouImages[user.JoinOrder - 1].SetActive(true);
        if (isMyData && user.JoinOrder == 1) buttonGameStart.SetActive(true);

        // 自分がマスタークライアント && 参加人数が2人以上の場合
        bool isSucsess = buttonGameStart.activeSelf;    // 一旦ボタンが表示されたかどうかで判定
        buttonGameStart.GetComponent<Button>().interactable = isSucsess;
    }

    /// <summary>
    /// 退室したユーザーのUIを削除
    /// </summary>
    /// <param name="joinOrder"></param>
    public void RemoveUserUI(int joinOrder)
    {
        objIconImages[joinOrder - 1].SetActive(false);
        textUserNames[joinOrder - 1].text = "EMPTY";
        objYouImages[joinOrder - 1].SetActive(false);

        // 自分がマスタークライアント && 参加人数が自分だけの場合
/*        bool isSucsess = buttonGameStart.activeSelf;    // 一旦ボタンが表示されたかどうかで判定
        buttonGameStart.GetComponent<Button>().interactable = isSucsess;*/
    }
}
