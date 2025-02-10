//*********************************************************
// �`���[�g���A����UI���Ǘ�����X�N���v�g
// Author:Rui Enomoto
//*********************************************************
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIController : MonoBehaviour
{
    #region Tween�A�j���[�V��������UI�̐e
    [SerializeField] List<GameObject> uiList;
    #endregion
    [SerializeField] GameObject panelBG;
    [SerializeField] List<Sprite> spritePages;
    [SerializeField] Image imgPage;
    [SerializeField] Button btnNextPage;
    [SerializeField] Button btnBackPage;

    TopSceneUIManager topSceneUIManager;
    SEController seController;
    int page = 0;

    private void Start()
    {
        topSceneUIManager = GetComponent<TopSceneUIManager>();
        seController = GetComponent<SEController>();
    }

    void UpdatePage()
    {
        if(page == 0)
        {
            btnBackPage.interactable = false;
        }
        else if(page == spritePages.Count - 1)
        {
            btnNextPage.interactable = false;
        }
        else
        {
            btnBackPage.interactable = true;
            btnNextPage.interactable = true;
        }

        imgPage.sprite = spritePages[page];
    }

    /// <summary>
    /// UI��\���E��\�����鏈��
    /// </summary>
    /// <param name="isVisibility"></param>
    public void ToggleUIVisibility(bool isVisibility)
    {
        // UI�����ۂɁA�`���[�g���A����ǂ񂾂��Ƃɂ���
        if (!isVisibility && !UserModel.Instance.IsReadTutorial)
        {
            UserModel.Instance.OnReadTutorial();
        }

        page = 0;
        UpdatePage();
        panelBG.SetActive(isVisibility);
        Vector3 endScale = isVisibility ? Vector3.one : Vector3.zero;
        Ease setEase = isVisibility ? Ease.OutBack : Ease.InBack;

        foreach (var ui in uiList)
        {
            ui.transform.DOScale(endScale, 0.2f).SetEase(setEase);
        }

        if (isVisibility) seController.PlayAudio(topSceneUIManager.SelectSE);
        else seController.PlayAudio(topSceneUIManager.CloseSE);
    }

    /// <summary>
    /// ���̃y�[�W��\������
    /// </summary>
    public void OnNextButton()
    {
        page++;
        if (page >= spritePages.Count) page = spritePages.Count - 1;

        seController.PlayAudio(topSceneUIManager.SelectSE);
        UpdatePage();
    }

    /// <summary>
    /// �O�̃y�[�W��\������
    /// </summary>
    public void OnBackButton()
    {
        page--;
        if (page <= 0) page = 0;

        seController.PlayAudio(topSceneUIManager.SelectSE);
        UpdatePage();
    }
}
