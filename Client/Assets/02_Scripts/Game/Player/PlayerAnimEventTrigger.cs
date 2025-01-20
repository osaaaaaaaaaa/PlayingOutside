using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEventTrigger : MonoBehaviour
{
    PlayerController playerController;
    PlayerAnimatorController animController;
    PlayerEffectController effectController;
    PlayerSkillController skillController;
    PlayerAudioController audioController;

    bool isPlayStampAnimFall;
    public bool IsPlayStampAnimFall { get { return isPlayStampAnimFall; } set { isPlayStampAnimFall = value; } }

    public enum EVENT_ID
    {
        OnEndAnim = 0,
        OnStartDownAnim,
        OnEndStandUpAnim,
        SetEffect_Down,
        SetEffect_Run,
        SetEffect_KnockBackSmoke,
        OnStampAnimRising,
        OnStampAnimFloating,
        OnStartStampAnimFall,
        OnLoopStampAnimFall,
        OnEndMachAuraChargeAnim,
        CheckRapidMachKick,
    }

    private void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
        animController = transform.parent.GetComponent<PlayerAnimatorController>();
        effectController = transform.parent.GetComponent<PlayerEffectController>();
        skillController = transform.parent.GetComponent<PlayerSkillController>();
        audioController = transform.parent.GetComponent<PlayerAudioController>();
        isPlayStampAnimFall = false;
    }

    public void OnTriggerAnimEvent(EVENT_ID id)
    {
        switch (id)
        {
            case EVENT_ID.OnEndAnim:
                animController.OnEndAnim(skillController.SkillId != PlayerSkillController.SKILL_ID.Skill3);
                break;
            case EVENT_ID.OnStartDownAnim:
                transform.parent.gameObject.layer = 8;  // 他のプレイヤーとの当たり判定を一時的に無くす
                break;
            case EVENT_ID.OnEndStandUpAnim:
                // 他のプレイヤーとの当たり判定を戻す
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
            case EVENT_ID.OnStampAnimRising:
                // スキル5(Stamp)の上昇したとき
                playerController.Jump(900);
                playerController.IsControlEnabled = true;
                break;
            case EVENT_ID.OnStampAnimFloating:
                // スキル5(Stamp)の空中にとどまるとき
                playerController.Rb.drag = 20;
                isPlayStampAnimFall = true;
                break;
            case EVENT_ID.OnStartStampAnimFall:
                // スキル5(Stamp)の落下するとき
                if (isPlayStampAnimFall) playerController.Rb.drag = 2;
                break;
            case EVENT_ID.OnLoopStampAnimFall:
                // スキル5(Stamp)の落下アニメーションをループ再生する
                if(isPlayStampAnimFall) animController.PlayAnimationFromFrame(106, "Skill5");
                break;
            case EVENT_ID.OnEndMachAuraChargeAnim:
                playerController.IsInvincible = false;
                playerController.IsControlEnabled = true;
                audioController.StopLoopSkillSourse();
                break;
            case EVENT_ID.CheckRapidMachKick:
                if (playerController.enabled && !playerController.IsPressMachKickBtn)
                {
                    animController.SetInt(PlayerAnimatorController.ANIM_ID.IdleA);
                }
                playerController.IsPressMachKickBtn = false;
                break;
        }
    }

    public void SetAnimId(PlayerAnimatorController.ANIM_ID id)
    {
        animController.SetInt(id);
    }

    public void PlayAudioOneShot(PlayerAudioController.AudioClipName clipName)
    {
        audioController.PlayOneShot(clipName);
    }

    public void PlayLoopSkillAudio(PlayerAudioController.AudioClipName clipName)
    {
        audioController.ResetLoopSkillSourse(clipName);
    }

    public void StopLoopSkillAudio()
    {
        audioController.StopLoopSkillSourse();
    }
}
