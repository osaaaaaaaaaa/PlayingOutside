using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEventTrigger : MonoBehaviour
{
    PlayerController playerController;
    PlayerAnimatorController animController;
    PlayerEffectController effectController;
    PlayerSkillController skillController;

    public enum EVENT_ID
    {
        OnEndAnim = 0,
        OnStartDownAnim,
        OnEndStandUpAnim,
        SetEffect_Down,
        SetEffect_Run,
        SetEffect_KnockBackSmoke,
        OnEndMachKickStartAnim,
        OnStartMachKickMidlleAnim,
        OnEndMachKickEndAnim,
        OnStampAnimRising,
        OnStampAnimFloating,
        OnStartStampAnimFall,
        OnLoopStampAnimFall,
    }

    private void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
        animController = transform.parent.GetComponent<PlayerAnimatorController>();
        effectController = transform.parent.GetComponent<PlayerEffectController>();
        skillController = transform.parent.GetComponent<PlayerSkillController>();
    }

    public void OnTriggerAnimEvent(EVENT_ID id)
    {
        switch (id)
        {
            case EVENT_ID.OnEndAnim:
                animController.OnEndAnim(skillController.SkillId != PlayerSkillController.SKILL_ID.Skill3);
                break;
            case EVENT_ID.OnStartDownAnim:
                transform.parent.gameObject.layer = 8;  // ���̃v���C���[�Ƃ̓����蔻����ꎞ�I�ɖ�����
                break;
            case EVENT_ID.OnEndStandUpAnim:
                // ���̃v���C���[�Ƃ̓����蔻���߂�
                if (playerController.enabled) transform.parent.gameObject.layer = 3;    // Player
                if (!playerController.enabled) transform.parent.gameObject.layer = 7; // NPC
                animController.OnEndStandUpAnim();
                break;
            case EVENT_ID.SetEffect_Down:
                effectController.SetEffect(PlayerEffectController.EFFECT_ID.Down);
                break;
            case EVENT_ID.SetEffect_Run:
                effectController.SetEffect(PlayerEffectController.EFFECT_ID.Run);
                break;
            case EVENT_ID.SetEffect_KnockBackSmoke:
                effectController.SetEffect(PlayerEffectController.EFFECT_ID.KnockBackSmoke);
                break;
            case EVENT_ID.OnEndMachKickStartAnim:
                if(animController.GetAnimId() != (int)PlayerAnimatorController.ANIM_ID.MachKick_Midlle) 
                    animController.Animator.SetInteger("animation", (int)PlayerAnimatorController.ANIM_ID.MachKick_End);
                break;
            case EVENT_ID.OnStartMachKickMidlleAnim:
                animController.Animator.SetInteger("animation", (int)PlayerAnimatorController.ANIM_ID.MachKick_End);
                break;
            case EVENT_ID.OnEndMachKickEndAnim:
                animController.OnEndMachKickAnim();
                break;
            case EVENT_ID.OnStampAnimRising:
                // �X�L��5(Stamp)�̏㏸�����Ƃ�
                playerController.Jump(900);
                break;
            case EVENT_ID.OnStampAnimFloating:
                // �X�L��5(Stamp)�̋󒆂ɂƂǂ܂�Ƃ�
                playerController.Rb.drag = 20;
                break;
            case EVENT_ID.OnStartStampAnimFall:
                // �X�L��5(Stamp)�̗�������Ƃ�
                playerController.Rb.drag = 2;
                break;
            case EVENT_ID.OnLoopStampAnimFall:
                // �X�L��5(Stamp)�̗����A�j���[�V���������[�v�Đ�����
                animController.PlayAnimationFromFrame(106, "Skill5");
                break;
        }
    }
}
