using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuickGameUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion

    [SerializeField] List<Image> icons;
    [SerializeField] List<Text> texts;
    [SerializeField] List<GameObject> loadingObjs;

    TopSceneUIManager topSceneUIManager;

    private void Start()
    {
        InitAllUserFrame();
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

    public void SetupUserFrame(int index,string userName,int characterId)
    {
        icons[index].sprite = GetComponent<TopSceneUIManager>().SpriteIcons[characterId];
        icons[index].enabled = true;
        texts[index].text = userName;
        loadingObjs[index].SetActive(false);
    }

    public void InitUserFrame(int index)
    {
        icons[index].enabled = false;
        texts[index].text = "受付中...";
        loadingObjs[index].SetActive(true);
    }

    public void InitAllUserFrame()
    {
        for (int i = 0; i < icons.Count; i++) 
        {
            InitUserFrame(i);
        }
    }

    /// <summary>
    /// クイックゲームUIを表示するボタン
    /// </summary>
    public void OnSelectButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;
        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
    }

    /// <summary>
    /// クイックゲームUIを非表示するボタン
    /// </summary>
    public void OnBackButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;
        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();
    }
}
