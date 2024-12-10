using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using Server.Model.Entity;

public class PrivateGameUIController : MonoBehaviour
{
    #region Tween�A�j���[�V��������UI�̐e
    [SerializeField] List<GameObject> uiList;
    #endregion

    #region ���߂ɕ\������郁�j���[
    [SerializeField] GameObject menu;
    [SerializeField] InputField inputFieldRoomName;
    [SerializeField] GameObject buttonJoinRoom;
    #endregion

    [SerializeField] TopSceneDirector topSceneDirector;
    TopSceneUIManager topSceneUIManager;

    private void Start()
    {
        InitUI();

        foreach (var ui in uiList)
        {
            ui.transform.localScale = Vector3.zero;
        }
        topSceneUIManager = GetComponent<TopSceneUIManager>();
    }

    public void InitUI()
    {
        menu.SetActive(true);
        inputFieldRoomName.text = "";
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
    /// �v���C�x�[�g�}�b�`UI��\������{�^��
    /// </summary>
    public void OnSelectButton()
    {
        InitUI();
        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
    }

    /// <summary>
    /// �v���C�x�[�g�}�b�`UI�����{�^��
    /// </summary>
    public void OnBackButton()
    {
        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();
    }

    /// <summary>
    /// �Q������{�^��
    /// </summary>
    public void OnJoinButton()
    {
        if (inputFieldRoomName.text == "") return;
        topSceneDirector.OnJoinRoomButton(inputFieldRoomName.text);
    }
}
