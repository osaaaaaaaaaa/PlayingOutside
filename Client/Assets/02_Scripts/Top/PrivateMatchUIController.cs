using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using Server.Model.Entity;

public class PrivateMatchUIController : MonoBehaviour
{
    #region Tween�A�j���[�V��������UI�̐e
    [SerializeField] List<GameObject> uiList;
    #endregion

    #region ���߂ɕ\������郁�j���[
    [SerializeField] GameObject menu;
    [SerializeField] InputField inputFieldRoomName;
    [SerializeField] GameObject buttonJoinRoom;
    #endregion

    #region ���[���ҋ@����UI�֌W
    [SerializeField] GameObject roomUsers;
    [SerializeField] Text roomName;
    [SerializeField] List<GameObject> objIconImages;
    [SerializeField] List<Text> textUserNames;
    [SerializeField] List<GameObject> objYouImages;
    [SerializeField] GameObject buttonGameStart;
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

    private void Update()
    {
        // ���[���ɎQ���ł��� && ���[����UI��\������ꍇ��
        if(RoomModel.Instance.userState == RoomModel.USER_STATE.joined 
            && menu.activeSelf
            && !roomUsers.activeSelf)
        {
            menu.SetActive(false);
            roomUsers.SetActive(true);
        }
    }

    public void InitUI()
    {
        menu.SetActive(true);
        roomUsers.SetActive(false);
        buttonGameStart.SetActive(false);
        inputFieldRoomName.text = "";
        foreach (var icon in objIconImages) 
        {
            icon.SetActive(false);
        }
        foreach (var text in textUserNames)
        {
            text.text = "EMPTY";
        }
        foreach(var img in objYouImages)
        {
            img.SetActive(false);
        }
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
    /// UI����O�ɖ߂�{�^��
    /// </summary>
    public void OnBackButton()
    {
        if (menu.activeSelf)
        {
            // ���j���[��ʂ̏ꍇ�A�v���C�x�[�g�}�b�`UI��S�ĕ���
            ToggleUIVisibility(false);
            topSceneUIManager.OnBackButton();
        }
        else
        {
            // �ގ����������N�G�X�g
            topSceneDirector.LeaveRoom();
        }
    }

    /// <summary>
    /// �Q������{�^��
    /// </summary>
    public void OnJoinButton()
    {
        if (inputFieldRoomName.text == "") return;

        // �������������N�G�X�g
        topSceneDirector.JoinRoom(inputFieldRoomName.text);
        roomName.text = inputFieldRoomName.text;
    }

    /// <summary>
    /// �Q�����̃��[�U�[UI��ݒ�
    /// </summary>
    /// <param name="joinOrder"></param>
    /// <param name="isMyData"></param>
    /// <param name="user"></param>
    public void SetupUserUI(bool isMyData, JoinedUser user)
    {
        Debug.Log(isMyData + ","+ user.UserData.Name + "," + (user.JoinOrder - 1) + "�Ԗ�");
        objIconImages[user.JoinOrder - 1].SetActive(true);
        objIconImages[user.JoinOrder - 1].GetComponent<Image>().sprite = topSceneUIManager.SpriteIcons[1 - 1];  // ��Ń��[�U�[�f�[�^�ɃL�����N�^�[ID�����
        textUserNames[user.JoinOrder - 1].text = user.UserData.Name;

        // �����̃f�[�^�̏ꍇ
        if (isMyData) objYouImages[user.JoinOrder - 1].SetActive(true);
        if (isMyData && user.JoinOrder == 1) buttonGameStart.SetActive(true);

        // �������}�X�^�[�N���C�A���g && �Q���l����2�l�ȏ�̏ꍇ
        bool isSucsess = buttonGameStart.activeSelf;    // ��U�{�^�����\�����ꂽ���ǂ����Ŕ���
        buttonGameStart.GetComponent<Button>().interactable = isSucsess;
    }

    /// <summary>
    /// �ގ��������[�U�[��UI���폜
    /// </summary>
    /// <param name="joinOrder"></param>
    public void RemoveUserUI(int joinOrder)
    {
        objIconImages[joinOrder - 1].SetActive(false);
        textUserNames[joinOrder - 1].text = "EMPTY";
        objYouImages[joinOrder - 1].SetActive(false);

        // �������}�X�^�[�N���C�A���g && �Q���l�������������̏ꍇ
/*        bool isSucsess = buttonGameStart.activeSelf;    // ��U�{�^�����\�����ꂽ���ǂ����Ŕ���
        buttonGameStart.GetComponent<Button>().interactable = isSucsess;*/
    }
}
