using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Server.Model.Entity;
using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] GameObject errorTextUser;
    [SerializeField] GameObject userCntTextObj;
    #endregion

    #region ユーザー検索関係
    [SerializeField] GameObject scrollViewSerch;
    [SerializeField] Transform contentViewSerch;
    [SerializeField] GameObject inputFieldSerchUserName;
    [SerializeField] GameObject errorTextSerch;
    #endregion

    [SerializeField] GameObject followingUserUiPrefab;
    TopSceneUIManager topSceneUIManager;
    List<FollowingUser> followingUsers = new List<FollowingUser>();
    public List<FollowingUser> FollowingUsers { get { return followingUsers; } set { followingUsers = value; } }

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
    public async void OnSelectButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        topSceneUIManager.OnSelectButton();

        followingUsers.Clear();
        var followings = await FollowModel.Instance.ShowFollowingUsersAsynk(UserModel.Instance.UserId);
        if (followings != null)
        {
            foreach (var following in followings)
            {
                followingUsers.Add(following);
                var ui = Instantiate(followingUserUiPrefab, contentViewUser);
                ui.GetComponent<FollowUserUI>().SetupUI(this, topSceneUIManager.SpriteIcons[following.CharacterId - 1], following, true);
            }
            userCntTextObj.GetComponent<Text>().text = followings.Length + "/" + ConstantManager.followingCntMax;
        }
        else
        {
            userCntTextObj.GetComponent<Text>().text = 0 + "/" + ConstantManager.followingCntMax;
        }

        errorTextUser.SetActive(followings == null);
        userCntTextObj.SetActive(true);

        ToggleUIVisibility(true);
    }

    /// <summary>
    /// フォローUIを非表示するボタン
    /// </summary>
    public void OnBackButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        // スクロールビューのコンテンツの中身をクリアする
        foreach (Transform child in contentViewUser)
        {
            Destroy(child.gameObject);
        }

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
    /// ユーザー検索メニューに切り替えるボタン
    /// </summary>
    public void OnChangeSerchUserMenu()
    {
        userCntTextObj.SetActive(false);
        scrollViewUser.SetActive(false);
        scrollViewSerch.SetActive(true);
        inputFieldSerchUserName.GetComponent<InputField>().interactable = true;
        inputFieldSerchUserName.SetActive(true);

        inputFieldSerchUserName.GetComponent<InputField>().text = "";

        // スクロールビューのコンテンツの中身をクリアする
        foreach (Transform child in contentViewSerch)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 名前でユーザーを検索する
    /// </summary>
    public async void SerchUserByName()
    {
        var inputField = inputFieldSerchUserName.GetComponent<InputField>();
        inputField.interactable = false;

        // スクロールビューのコンテンツの中身をクリアする
        foreach (Transform child in contentViewSerch)
        {
            Destroy(child.gameObject);
        }

        if (inputField.text == "")
        {
            inputField.interactable = true;
            return;
        }

        var user = await FollowModel.Instance.ShowUserByNameAsync(inputField.text);
        inputField.interactable = true;
        errorTextSerch.SetActive(user == null);
        if (user == null)
        {
            Debug.Log("ユーザーが見つかりませんでした");
            return;
        }

        bool isFollowingUser = false;
        if (followingUsers.Count > 0)
        {
            foreach (var folloing in followingUsers)
            {
                if (folloing.UserId == user.UserId)
                {
                    isFollowingUser = true;
                    break;
                }
            }
        }

        var ui = Instantiate(followingUserUiPrefab, contentViewSerch);
        ui.GetComponent<FollowUserUI>().SetupUI(this, topSceneUIManager.SpriteIcons[user.CharacterId - 1], user, isFollowingUser);
    }
}
