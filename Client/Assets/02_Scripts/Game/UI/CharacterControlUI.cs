using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterControlUI : MonoBehaviour
{
    [SerializeField] GameObject testCharacter;

    [SerializeField] GameObject uiParent;
    [SerializeField] FloatingJoystick floatingJoystick;
    [SerializeField] Button btnKick;
    [SerializeField] Button btnJump;
    [SerializeField] Button btnSkill;
    [SerializeField] Text textSkillCoolTime;
    [SerializeField] Button btnUseItem;
    [SerializeField] List<Sprite> spriteItemList;   // 0:空,1:唐辛子...
    [SerializeField] Button btnShowEmoteWindow;
    [SerializeField] EmoteWindowUI emoteWindowUI;

    public bool IsSetupDone { get; private set; } = false;

    private void Start()
    {
        if (testCharacter != null && RoomModel.Instance.userState != RoomModel.USER_STATE.joined)
            SetupButtonEvent(testCharacter);
    }

    public enum ButtonType
    {
        kick,
        jump,
        skill,
        item
    }

    float coolTime;
    bool isCoolTime = false;

    public void SetColorAlphaButtons(float alpha)
    {
        Color color = new Color(1,1,1,alpha);
        btnKick.gameObject.GetComponent<Image>().color = color;
        btnJump.gameObject.GetComponent<Image>().color = color;
        btnSkill.gameObject.GetComponent<Image>().color = color;
        btnUseItem.gameObject.GetComponent<Image>().color = color;
        btnShowEmoteWindow.gameObject.GetComponent<Image>().color = color;
    }

    public void ToggleUIVisibiliity(bool visibility)
    {
        uiParent.SetActive(visibility);
    }

    public void ClickEvent(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.kick:
                btnKick.onClick.Invoke();
                break;
            case ButtonType.jump:
                btnJump.onClick.Invoke();
                break;
            case ButtonType.skill:
                btnSkill.onClick.Invoke();
                break;
            case ButtonType.item:
                btnUseItem.onClick.Invoke();
                break;
        }
    }

    public void SetupButtonEvent(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        PlayerAnimatorController animController = player.GetComponent<PlayerAnimatorController>();
        PlayerItemController itemController = player.GetComponent<PlayerItemController>();
        coolTime = player.GetComponent<PlayerSkillController>().CoolTime;

        btnKick.onClick.AddListener(controller.OnKickButton);
        btnJump.onClick.AddListener(controller.OnJumpButton);
        btnSkill.onClick.AddListener(controller.OnSkillButton);
        btnUseItem.onClick.AddListener(itemController.OnUseItemButton);
        btnUseItem.onClick.AddListener(OnUseItemButton);
        btnShowEmoteWindow.onClick.AddListener(OnShowEmoteWindow);

        // エモートのボタン設定
        for(int i = 0; i < emoteWindowUI.ButtonEmotes.Count; i++)
        {
            PlayerAnimatorController.ANIM_ID emoteId = new PlayerAnimatorController.ANIM_ID();
            emoteId = (PlayerAnimatorController.firstEmoteId + i);
            emoteWindowUI.ButtonEmotes[i].onClick.AddListener(() => animController.SetInt(emoteId));
        }

        ToggleButtonInteractable(false);
        IsSetupDone = true;
    }

    public void ToggleButtonInteractable(bool toggle)
    {
        btnKick.interactable = toggle;
        btnJump.interactable = toggle;
        if(!isCoolTime) btnSkill.interactable = toggle;
        btnUseItem.interactable = toggle;
        btnShowEmoteWindow.interactable = toggle;

        if (!toggle)
        {
            emoteWindowUI.ToggleWindowVisibility(false);
        }
    }

    public void ToggleButtonInteractable(bool toggle, bool isButtonKickActive)
    {
        btnKick.interactable = isButtonKickActive;
        btnJump.interactable = toggle;
        if (!isCoolTime) btnSkill.interactable = toggle;
        btnUseItem.interactable = toggle;
        btnShowEmoteWindow.interactable = toggle;

        if (!toggle)
        {
            emoteWindowUI.ToggleWindowVisibility(false);
        }
    }

    public void SetImageItem(EnumManager.ITEM_ID itemId)
    {
        int index = (int)(itemId - EnumManager.ITEM_ID.Pepper + 1);
        btnUseItem.image.sprite = spriteItemList[index];
    }

    public void OnUseItemButton()
    {
        btnUseItem.image.sprite = spriteItemList[0];
    }

    public void OnSkillButton()
    {
        StopCoroutine(CoolTimeCoroutine());
        isCoolTime = true;
        btnSkill.interactable = false;
        StartCoroutine(CoolTimeCoroutine());
    }

    IEnumerator CoolTimeCoroutine()
    {
        for (int i = 0; i < coolTime; i++) 
        {
            textSkillCoolTime.text = coolTime - i + "s";
            yield return new WaitForSeconds(1);
        }

        textSkillCoolTime.text = "";
        isCoolTime = false;
        btnSkill.interactable = true;
    }

    public void OnShowEmoteWindow()
    {
        emoteWindowUI.ToggleWindowVisibility(true);
    }
}
