using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FollowUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion
    [SerializeField] List<Text> titleTextList;

    #region ユーザーリスト関係
    [SerializeField] GameObject scrollViewUser;
    [SerializeField] Transform contentViewUser;
    [SerializeField] Text changeButtonText;
    #endregion

    #region ユーザー検索関係
    [SerializeField] GameObject scrollViewSerch;
    [SerializeField] Transform contentViewSerch;
    [SerializeField] GameObject inputFieldSerchUserName;
    #endregion

    TopSceneUIManager topSceneUIManager;

    enum CONTENT_TYPE
    {
        FOLLOW,
        FOLLOWER
    }
    CONTENT_TYPE contentType = CONTENT_TYPE.FOLLOW;

    private void Start()
    {
        InitUI();
        foreach (var ui in uiList)
        {
            ui.transform.localScale = Vector3.zero;
        }
        topSceneUIManager = GetComponent<TopSceneUIManager>();
    }

    void InitUI()
    {
        scrollViewUser.SetActive(true);
        scrollViewSerch.SetActive(false);
        inputFieldSerchUserName.SetActive(false);
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
    /// フォローUIを表示するボタン
    /// </summary>
    public void OnSelectButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
    }

    /// <summary>
    /// フォローUIを非表示するボタン
    /// </summary>
    public void OnBackButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();
        InitUI();
    }

    /// <summary>
    /// フォロー|フォロワーリストに切り替えるボタン
    /// </summary>
    public void OnChangeContentButton()
    {
        scrollViewUser.SetActive(true);
        scrollViewSerch.SetActive(false);
        inputFieldSerchUserName.SetActive(false);

        switch (contentType)
        {
            case CONTENT_TYPE.FOLLOW:
                contentType = CONTENT_TYPE.FOLLOWER;
                changeButtonText.text = "フォロー";
                foreach (var title in titleTextList)
                {
                    title.text = "FOLLOWER";
                }
                break;
            case CONTENT_TYPE.FOLLOWER:
                contentType = CONTENT_TYPE.FOLLOW;
                changeButtonText.text = "フォロワー";
                foreach (var title in titleTextList)
                {
                    title.text = "FOLLOW";
                }
                break;
        }

        // スクロールビューのコンテンツの中身をクリアする
        foreach (Transform child in contentViewUser)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// ユーザー検索ボタン
    /// </summary>
    public void OnSerchButton()
    {
        scrollViewUser.SetActive(false);
        scrollViewSerch.SetActive(true);
        inputFieldSerchUserName.SetActive(true);

        inputFieldSerchUserName.GetComponent<InputField>().text = "";

        // スクロールビューのコンテンツの中身をクリアする
        foreach (Transform child in contentViewSerch)
        {
            Destroy(child.gameObject);
        }
    }
}
