//*********************************************************
// ランキングのUIを管理するスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Server.Model.Entity;

public class RankingUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion

    #region 全ユーザーのランキング関係
    [SerializeField] GameObject scrollViewGlobal;
    [SerializeField] Transform contentViewGlobal;
    #endregion

    #region フォローしているユーザーのみのランキング関係
    [SerializeField] GameObject scrollViewFollowing;
    [SerializeField] Transform contentViewFollowing;
    #endregion

    [SerializeField] Button switchViewButton;
    [SerializeField] Text switchViewText;
    [SerializeField] GameObject rankingUserUiPrefab;
    [SerializeField] GameObject errorText;
    [SerializeField] Text myRatingText;
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
    /// ランキングUIを表示するボタン
    /// </summary>
    public async void OnSelectButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;
        topSceneUIManager.OnSelectButton();
        scrollViewGlobal.SetActive(true);

        ShowGlobalRanking();
        ToggleUIVisibility(true);
        myRatingText.text = RatingModel.Instance.Rating.ToString();
    }

    /// <summary>
    /// ランキングUIを非表示するボタン
    /// </summary>
    public void OnBackButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();

        scrollViewGlobal.SetActive(false);
        scrollViewFollowing.SetActive(false);
    }

    /// <summary>
    /// ランキングの表示内容を切り替える
    /// </summary>
    public void SwitchContentView()
    {
        switchViewButton.interactable = false;
        if (!scrollViewGlobal.activeSelf) ShowGlobalRanking();
        else if (!scrollViewFollowing.activeSelf) ShowFollowingRanking();
    }

    async void ShowGlobalRanking()
    {
        switchViewText.text = "フォローのみ";
        scrollViewFollowing.SetActive(false);
        scrollViewGlobal.SetActive(true);
        // スクロールビューのコンテンツの中身をクリアする
        foreach (Transform child in contentViewGlobal)
        {
            Destroy(child.gameObject);
        }

        var rankingUsers = await RatingModel.Instance.ShowGlobalRatingRanking();
        if (rankingUsers != null)
        {
            List<RatingRanking> rankingUserList = new List<RatingRanking>(rankingUsers);

            // 降順にソート
            rankingUserList.Sort((a, b) => b.Rating - a.Rating);

            for (int i = 0; i < rankingUserList.Count; i++)
            {
                int characterIconId = rankingUserList[i].CharacterId - 1;
                var ui = Instantiate(rankingUserUiPrefab, contentViewGlobal);
                ui.GetComponent<RankingUserUI>().SetupUI(topSceneUIManager.SpriteIcons[characterIconId], rankingUserList[i], i + 1);
            }
        }
        errorText.SetActive(rankingUsers == null);
        switchViewButton.interactable = true;
    }

    async void ShowFollowingRanking()
    {
        switchViewText.text = "全ユーザー";
        scrollViewGlobal.SetActive(false);
        scrollViewFollowing.SetActive(true);
        // スクロールビューのコンテンツの中身をクリアする
        foreach (Transform child in contentViewFollowing)
        {
            Destroy(child.gameObject);
        }

        var rankingUsers = await RatingModel.Instance.ShowFollowedUsersRatingRanking(UserModel.Instance.UserId);
        if (rankingUsers != null)
        {
            List<RatingRanking> rankingUserList = new List<RatingRanking>(rankingUsers);
            rankingUserList.Add(new RatingRanking()
            {
                UserName = UserModel.Instance.UserName,
                CharacterId = UserModel.Instance.CharacterId,
                Rating = RatingModel.Instance.Rating,
            });

            // 降順にソート
            rankingUserList.Sort((a, b) => b.Rating - a.Rating);

            for (int i = 0; i < rankingUserList.Count; i++)
            {
                int characterIconId = rankingUserList[i].CharacterId - 1;
                var ui = Instantiate(rankingUserUiPrefab, contentViewFollowing);
                ui.GetComponent<RankingUserUI>().SetupUI(topSceneUIManager.SpriteIcons[characterIconId], rankingUserList[i], i + 1);
            }
        }
        errorText.SetActive(rankingUsers == null);
        switchViewButton.interactable = true;
    }
}
