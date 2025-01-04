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

    // �m�b�N�o�b�N�����ǂ���
    bool isKnockBackAnim;

    #region ���b�V���֌W
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
            // �m�b�N�o�b�N���I�����A�A�j���[�V�����Đ�
            isKnockBackAnim = false;
            var angle = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, angle.y, angle.z);
            animator.SetInteger("animation", (int)ANIM_ID.StandUp);
        }
    }

    public void PlayAnimationFromFrame(float targetFlame,string animName)
    {
        // �A�j���[�V������StateInfo���擾
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // �A�j���[�V�����̑S�̃t���[�������v�Z
        int totalFrames = Mathf.RoundToInt(stateInfo.length * stateInfo.speed * stateInfo.speedMultiplier * 60f);
        Debug.Log(totalFrames);

        // �w��t���[���𐳋K���^�C���ɕϊ� (0�`1)
        float normalizedTime = targetFlame / totalFrames;

        // �w�肵���A�j���[�V�����𐳋K���^�C������Đ�
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
    /// �v���C���[�p
    /// </summary>
    /// <param name="id"></param>
    public void SetInt(ANIM_ID id)
    {
        if (playerController.IsControlEnabled && GetAnimId() == (int)id) return;    // ����s�\���&&�����A�j���[�V�������Đ����悤�Ƃ����ꍇ

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
    /// NPC�p
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
    /// �A�j���[�V�������I������
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
    /// �m�b�N�o�b�N���o
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
    /// �����オ��A�j���[�V�������I������
    /// </summary>
    public void OnEndStandUpAnim() 
    {
        playerController.IsStandUp = true;

        // ���G��Ԃ̃A�j���[�V����
        StopCoroutine(FlashCoroutine());
        StartCoroutine(FlashCoroutine());
        SetInt(ANIM_ID.IdleB);
    }

    /// <summary>
    /// �_�ŏ���
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
