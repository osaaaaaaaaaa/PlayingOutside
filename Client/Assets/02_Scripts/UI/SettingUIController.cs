//*********************************************************
// 設定画面のUIを管理するスクリプト
// Author:Rui Enomoto
//*********************************************************
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion
    [SerializeField] Slider sliderBGM;
    [SerializeField] Slider sliderSE;
    TopSceneUIManager topSceneUIManager;

    #region オーディオコントローラー関係
    BGMController bgmController;
    List<SEController> seControllers;
    #endregion

    virtual protected void Start()
    {
        // 各コントローラー関係を取得
        bgmController = Camera.main.GetComponent<BGMController>();
        seControllers = new List<SEController>(FindObjectsOfType<SEController>());  // シーン上の全てのSEControllerを取得

        foreach (var ui in uiList)
        {
            ui.transform.localScale = Vector3.zero;
        }
        topSceneUIManager = GetComponent<TopSceneUIManager>();
        sliderBGM.value = AudioVolume.BgmVolume;
        sliderSE.value = AudioVolume.SeVolume;
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
    /// 設定UIを表示するボタン
    /// </summary>
    public virtual void OnSelectButton()
    {
        if(topSceneUIManager != null)
        {
            if (topSceneUIManager.IsTaskRunning) return;
            topSceneUIManager.IsTaskRunning = true;

            topSceneUIManager.OnSelectButton();
        }
        ToggleUIVisibility(true);
    }

    /// <summary>
    /// 設定UIを非表示するボタン
    /// </summary>
    public virtual void OnBackButton()
    {
        if(topSceneUIManager != null)
        {
            if (topSceneUIManager.IsTaskRunning) return;
            topSceneUIManager.IsTaskRunning = true;
            topSceneUIManager.OnBackButton();
        }
        ToggleUIVisibility(false);
    }

    public void OnTitleButton()
    {
        SceneControler.Instance.StartSceneLoad("TitleScene");
    }

    public void OnSliderBGM()
    {
        AudioVolume.BgmVolume = sliderBGM.value;
        bgmController.SetupAudioVolume();
        UserModel.Instance.SaveUserData();
    }

    public void OnSliderSE()
    {
        AudioVolume.SeVolume = sliderSE.value;
        UpdateSEVolumes();
        UserModel.Instance.SaveUserData();
    }

    void UpdateSEVolumes()
    {
        foreach (var se in seControllers)
        {
            se.SetupAudioVolume();
        }
    }
}
