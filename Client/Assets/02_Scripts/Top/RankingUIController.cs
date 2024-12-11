using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RankingUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion
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
    public void OnSelectButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
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
    }
}
