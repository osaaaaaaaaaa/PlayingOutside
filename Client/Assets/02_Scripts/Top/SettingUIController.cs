using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingUIController : MonoBehaviour
{
    #region Tween�A�j���[�V��������UI�̐e
    [SerializeField] List<GameObject> uiList;
    #endregion
    [SerializeField] Slider sliderBGM;
    [SerializeField] Slider sliderSE;
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
    /// �ݒ�UI��\������{�^��
    /// </summary>
    public void OnSelectButton()
    {
        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
    }

    /// <summary>
    /// �ݒ�UI���\������{�^��
    /// </summary>
    public void OnBackButton()
    {
        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();
    }

    public void OnTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void OnSliderBGM()
    {
        // ���ʕύX
    }

    public void OnSliderSE()
    {
        // ���ʕύX
    }
}
