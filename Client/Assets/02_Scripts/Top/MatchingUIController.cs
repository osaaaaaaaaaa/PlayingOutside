using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MatchingUIController : MonoBehaviour
{
    #region Tween�A�j���[�V��������UI�̐e
    [SerializeField] List<GameObject> uiList;
    #endregion

    #region ���߂ɕ\������郁�j���[�{�^��
    [SerializeField] GameObject buttonCreateRoom;
    [SerializeField] GameObject buttonJoinRoom;
    #endregion

    #region �Q�����郋�[�����X�g��UI�֌W
    [SerializeField] GameObject scrollViewRoom;
    [SerializeField] Transform contentViewRoom;
    #endregion

    #region ���[���ҋ@����UI�֌W
    [SerializeField] GameObject roomUsers;
    #endregion

    [SerializeField] Sprite spriteLockOn;
    [SerializeField] Sprite spriteLockOff;
    [SerializeField] Image imageLockButton;
    [SerializeField] InputField inputFieldRoomName;
    TopSceneUIManager topSceneUIManager;

    public enum MATCHING_TYPE
    {
        NONE = 0,
        CREATE,
        JOIN
    }
    MATCHING_TYPE matchingType = MATCHING_TYPE.NONE;

    enum LOCK_MODE
    {
        LOCK_OFF = 0,
        LOCK_ON
    }
    LOCK_MODE lockMode = LOCK_MODE.LOCK_OFF;

    private void Start()
    {
        InitUI();
        imageLockButton.sprite = spriteLockOff;

        foreach (var ui in uiList)
        {
            ui.transform.localScale = Vector3.zero;
        }
        topSceneUIManager = GetComponent<TopSceneUIManager>();
    }

    private void InitUI()
    {
        scrollViewRoom.SetActive(false);
        roomUsers.SetActive(false);
        buttonCreateRoom.SetActive(true);
        buttonJoinRoom.SetActive(true);
        inputFieldRoomName.interactable = true;
        matchingType = MATCHING_TYPE.NONE;
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
    /// �}�b�`���OUI��\������{�^��
    /// </summary>
    public void OnSelectButton()
    {
        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
    }

    /// <summary>
    /// �}�b�`���OUI���\������{�^��
    /// </summary>
    public void OnBackButton()
    {
        if(matchingType == MATCHING_TYPE.NONE)
        {
            ToggleUIVisibility(false);
            InitUI();
            topSceneUIManager.OnBackButton();
        }
        else
        {
            matchingType = MATCHING_TYPE.NONE;
            roomUsers.SetActive(false);
            scrollViewRoom.SetActive(false);
            buttonCreateRoom.SetActive(true);
            buttonJoinRoom.SetActive(true);
        }
    }

    /// <summary>
    /// ���b�N�{�^��
    /// </summary>
    public void OnLockButton()
    {
        switch (lockMode)
        {
            case LOCK_MODE.LOCK_ON:
                imageLockButton.sprite = spriteLockOff;
                lockMode = LOCK_MODE.LOCK_OFF;
                break;
            case LOCK_MODE.LOCK_OFF:
                imageLockButton.sprite = spriteLockOn;
                lockMode = LOCK_MODE.LOCK_ON;
                break;
        }
    }

    /// <summary>
    /// ���߂ɕ\�������{�^��������������
    /// </summary>
    public void OnMenuButton(int typeId)
    {
        buttonCreateRoom.SetActive(false);
        buttonJoinRoom.SetActive(false);
        inputFieldRoomName.interactable = false;

        switch (typeId) 
        {
            case (int)MATCHING_TYPE.CREATE:
                matchingType = MATCHING_TYPE.CREATE;
                roomUsers.SetActive(true);
                break;
            case (int)MATCHING_TYPE.JOIN:
                matchingType = MATCHING_TYPE.JOIN;

                // �X�N���[���r���[�̃R���e���c�̒��g���N���A����
                foreach (Transform child in contentViewRoom)
                {
                    Destroy(child.gameObject);
                }
                scrollViewRoom.SetActive(true);
                break;
        }
    }
}
