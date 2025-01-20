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

    #region �I�[�f�B�I�R���g���[���[�֌W
    BGMController bgmController;
    List<SEController> seControllers;
    #endregion

    private void Start()
    {
        // �e�I�[�f�B�I�R���g���[���[�֌W���擾
        bgmController = Camera.main.GetComponent<BGMController>();
        seControllers = new List<SEController>(FindObjectsOfType<SEController>());  // �V�[����̑S�Ă�SEController���擾
        Debug.Log(seControllers.Count + "�擾����");

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
    /// �ݒ�UI��\������{�^��
    /// </summary>
    public void OnSelectButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
    }

    /// <summary>
    /// �ݒ�UI���\������{�^��
    /// </summary>
    public void OnBackButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();
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
