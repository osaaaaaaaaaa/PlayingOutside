using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static PlayerSkillController;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] Animator animator;
    public Animator Animator { get { return animator; } }
    PlayerController playerController;
    PlayerSkillController skillController;

    // ノックバック中かどうか
    bool isKnockBackAnim;

    #region メッシュ関係
    [SerializeField] List<SkinnedMeshRenderer> skinnedMeshs;
    [SerializeField] MeshRenderer meshMain;
    #endregion

    public enum ANIM_ID
    {
        IdleA = 1,
        IdleB,
        Jump,
        Kick,
        MachKick_Start,
        MachKick_Midlle,
        MachKick_End,
        Skill1_Hurricane,
        Skill2_Screwkick,
        Skill3_MachAura,
        Skill4_RollKick,
        Skill5_Stamp_Stamp,
        Damage,
        Die,
        Run,
        RunFast,
        StandUp,
        Respawn,
    }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        skillController = GetComponent<PlayerSkillController>();
        isKnockBackAnim = false;
    }

    private void OnDisable()
    {
        if(!playerController || !skillController) return;
        OnEndAnim(true);
        isKnockBackAnim = false;
        StopCoroutine(FlashCoroutine());
        foreach (var meshs in skinnedMeshs)
        {
            meshs.enabled = true;
        }
        meshMain.enabled = true;
        animator.SetBool("is_invincible", false);
    }

    private void Update()
    {
        if (isKnockBackAnim && GetComponent<PlayerIsGroundController>().IsGround())
        {
            // ノックバックを終了し、アニメーション再生
            isKnockBackAnim = false;
            var angle = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, angle.y, angle.z);
            animator.SetInteger("animation", (int)ANIM_ID.StandUp);
        }
    }

    public void PlayAnimationFromFrame(float targetFlame,string animName)
    {
        // アニメーションのStateInfoを取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // アニメーションの全体フレーム数を計算
        int totalFrames = Mathf.RoundToInt(stateInfo.length * stateInfo.speed * stateInfo.speedMultiplier * 60f);
        Debug.Log(totalFrames);

        // 指定フレームを正規化タイムに変換 (0～1)
        float normalizedTime = targetFlame / totalFrames;

        // 指定したアニメーションを正規化タイムから再生
        animator.Play(animName, 0, normalizedTime);
    }

    public ANIM_ID GetSkillAnimId()
    {
        ANIM_ID resultId = ANIM_ID.IdleB;
        switch (skillController.SkillId)
        {
            case SKILL_ID.Skill1:
                resultId = ANIM_ID.Skill1_Hurricane;
                break;
            case SKILL_ID.Skill2:
                resultId = ANIM_ID.Skill2_Screwkick;
                break;
            case SKILL_ID.Skill3:
                resultId = ANIM_ID.Skill3_MachAura;
                break;
            case SKILL_ID.Skill4:
                resultId = ANIM_ID.Skill4_RollKick;
                break;
            case SKILL_ID.Skill5:
                resultId = ANIM_ID.Skill5_Stamp_Stamp;
                break;
        }

        return resultId;
    }

    /// <summary>
    /// プレイヤー用
    /// </summary>
    /// <param name="id"></param>
    public void SetInt(ANIM_ID id)
    {
        if (playerController.IsControlEnabled && GetAnimId() == (int)id) return;    // 操作不能状態&&同じアニメーションを再生しようとした場合

        if (id == ANIM_ID.Skill1_Hurricane || id == ANIM_ID.Skill2_Screwkick || id == ANIM_ID.Skill3_MachAura
            || id == ANIM_ID.Skill4_RollKick || id == ANIM_ID.Skill5_Stamp_Stamp)
        {
            skillController.OnStartSkillAnim();
            if (id == ANIM_ID.Skill3_MachAura) return;
        }
        if(id == ANIM_ID.Respawn)
        {
            isKnockBackAnim = false;
            playerController.IsInvincible = true;
            animator.SetBool("is_invincible", false);
            skillController.OnEndSkillAnim();
            animator.Play("Anim_Respawn");
        }
        if(!playerController.IsInvincible && id == ANIM_ID.Damage)
        {
            playerController.IsInvincible = true;
            animator.Play("Damage");
        }
        if(id == ANIM_ID.Kick || id == ANIM_ID.Damage)
        {
            playerController.IsControlEnabled = false;
        }

        animator.SetInteger("animation", (int)id);
    }

    /// <summary>
    /// NPC用
    /// </summary>
    /// <param name="id"></param>
    public void SetInt(int id)
    {
        if (skillController.isUsedSkill && skillController.SkillId != SKILL_ID.Skill3) return;

        if (id == (int)ANIM_ID.Skill1_Hurricane || id == (int)ANIM_ID.Skill2_Screwkick || id == (int)ANIM_ID.Skill3_MachAura
            || id == (int)ANIM_ID.Skill4_RollKick || id == (int)ANIM_ID.Skill5_Stamp_Stamp)
        {
            skillController.OnStartSkillAnim();
            if (id == (int)ANIM_ID.Skill3_MachAura) return;
        }
        if (id == (int)ANIM_ID.Respawn)
        {
            isKnockBackAnim = false;
            playerController.IsInvincible = true;
            animator.SetBool("is_invincible", false);
            skillController.OnEndSkillAnim();
            animator.Play("Anim_Respawn");
        }
        if (id == (int)ANIM_ID.Damage)
        {
            animator.Play("Damage");
        }

        animator.SetInteger("animation", id);
    }

    public int GetAnimId()
    {
        return animator.GetInteger("animation");
    }

    /// <summary>
    /// アニメーションが終了した
    /// </summary>
    public void OnEndAnim(bool isEndSkillAnim)
    {
        if (isEndSkillAnim && skillController.isUsedSkill) skillController.OnEndSkillAnim();
        if (GetAnimId() != (int)ANIM_ID.Kick) playerController.IsInvincible = false;
        playerController.IsControlEnabled = true;
        SetInt(ANIM_ID.IdleB);
    }

    public void OnEndMachKickAnim()
    {
        if (GetAnimId() != (int)ANIM_ID.Kick) playerController.IsInvincible = false;
        playerController.IsControlEnabled = true;
        SetInt(ANIM_ID.IdleA);
    }

    /// <summary>
    /// ノックバック演出
    /// </summary>
    public void PlayKnockBackAnim()
    {
        OnEndAnim(skillController.SkillId != SKILL_ID.Skill3);

        playerController.IsStandUp = false;
        isKnockBackAnim = true;
        animator.Play("Die");
        animator.SetInteger("animation", (int)ANIM_ID.Die);
    }

    /// <summary>
    /// 立ち上がるアニメーションが終了した
    /// </summary>
    public void OnEndStandUpAnim() 
    {
        playerController.IsStandUp = true;

        // 無敵状態のアニメーション
        StopCoroutine(FlashCoroutine());
        StartCoroutine(FlashCoroutine());
        SetInt(ANIM_ID.IdleB);
    }

    /// <summary>
    /// 点滅処理
    /// </summary>
    /// <returns></returns>
    IEnumerator FlashCoroutine()
    {
        playerController.IsControlEnabled = true;
        playerController.IsInvincible = true;
        animator.SetBool("is_invincible", true);

        foreach (var meshs in skinnedMeshs)
        {
            meshs.enabled = true;
        }
        meshMain.enabled = true;

        float waitSec = 0.125f;
        for (float i = 0; i < 1; i += waitSec)
        {
            yield return new WaitForSeconds(waitSec);

            foreach (var meshs in skinnedMeshs)
            {
                meshs.enabled = !meshs.enabled;
            }
            meshMain.enabled = !meshMain.enabled;
        }

        foreach (var meshs in skinnedMeshs)
        {
            meshs.enabled = true;
        }
        meshMain.enabled = true;

        if (!skillController.isUsedSkill || skillController.SkillId == SKILL_ID.Skill3) playerController.IsInvincible = false;
        animator.SetBool("is_invincible", false);
    }
}
