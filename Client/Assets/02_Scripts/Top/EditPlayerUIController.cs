using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EditPlayerUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion
    [SerializeField] List<GameObject> scrollViewList;
    [SerializeField] InputField userName;
    [SerializeField] Text changeButtonText;
    [SerializeField] CharacterManager characterManager;
    TopSceneUIManager topSceneUIManager;

    private void Start()
    {
        foreach (var ui in uiList)
        {
            ui.transform.localScale = Vector3.zero;
        }
        topSceneUIManager = GetComponent<TopSceneUIManager>();
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
    /// プレイヤー編集UIを表示するボタン
    /// </summary>
    public void OnSelectButton()
    {
        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
        characterManager.DOMoveCharacter();
    }

    /// <summary>
    /// プレイヤー編集UIを非表示するボタン
    /// </summary>
    public void OnBackButton()
    {
        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();
        characterManager.DOResetCharacter();
    }

    /// <summary>
    /// スクロールビューを切り替えるボタン
    /// </summary>
    public void OnChangeScrollViewButton()
    {
        foreach(var scrollView in scrollViewList)
        {
            // 表示・非表示切り替え
            scrollView.gameObject.SetActive(!scrollView.gameObject.activeSelf);
        }
        if (scrollViewList[0].gameObject.activeSelf) changeButtonText.text = "エモート";
        if (!scrollViewList[0].gameObject.activeSelf) changeButtonText.text = "キャラクター";
    }

    /// <summary>
    /// ユーザー名を編集するボタン
    /// </summary>
    public void OnEditUserNameButton()
    {
        userName.Select();
    }
}
