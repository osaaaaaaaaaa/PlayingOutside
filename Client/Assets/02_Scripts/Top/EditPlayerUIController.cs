using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EditPlayerUIController : MonoBehaviour
{
    #region Tween�A�j���[�V��������UI�̐e
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
    /// �v���C���[�ҏWUI��\������{�^��
    /// </summary>
    public void OnSelectButton()
    {
        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
        characterManager.DOMoveCharacter();
    }

    /// <summary>
    /// �v���C���[�ҏWUI���\������{�^��
    /// </summary>
    public void OnBackButton()
    {
        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();
        characterManager.DOResetCharacter();
    }

    /// <summary>
    /// �X�N���[���r���[��؂�ւ���{�^��
    /// </summary>
    public void OnChangeScrollViewButton()
    {
        foreach(var scrollView in scrollViewList)
        {
            // �\���E��\���؂�ւ�
            scrollView.gameObject.SetActive(!scrollView.gameObject.activeSelf);
        }
        if (scrollViewList[0].gameObject.activeSelf) changeButtonText.text = "�G���[�g";
        if (!scrollViewList[0].gameObject.activeSelf) changeButtonText.text = "�L�����N�^�[";
    }

    /// <summary>
    /// ���[�U�[����ҏW����{�^��
    /// </summary>
    public void OnEditUserNameButton()
    {
        userName.Select();
    }
}
