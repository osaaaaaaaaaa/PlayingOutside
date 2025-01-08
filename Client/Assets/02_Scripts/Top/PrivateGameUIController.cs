using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using Server.Model.Entity;

public class PrivateGameUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion

    #region 初めに表示されるメニュー
    [SerializeField] GameObject menu;
    [SerializeField] InputField inputFieldRoomName;
    [SerializeField] GameObject buttonJoinRoom;
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

    public void InitUI()
    {
        menu.SetActive(true);
        inputFieldRoomName.text = "";
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
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        InitUI();
        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
    }

    /// <summary>
    /// プライベートマッチUIを閉じるボタン
    /// </summary>
    public void OnBackButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();
    }

    /// <summary>
    /// 参加するボタン
    /// </summary>
    public void OnJoinButton()
    {
        if (inputFieldRoomName.text == "") return;
        if (NGWordModel.Instance.ContainsNGWord(inputFieldRoomName.text))
        {
            ErrorUIController.Instance.ShowErrorUI("使用できないワードが含まれています。");
            return;
        }

        buttonJoinRoom.GetComponent<Button>().interactable = false;
        topSceneDirector.OnJoinRoomButton(inputFieldRoomName.text);
    }
}
