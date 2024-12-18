using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] Animator animator;
    PlayerController playerController;
    PlayerSkillController skillController;

    // �����オ�������ǂ���
    public bool isStandUp { get; private set; }
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
        Jump = 3,
        Kick = 5,
        Skill = 6,
        Damage = 10,
        Die = 11,
        Run = 15,
        StandUp = 20,
    }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        skillController = GetComponent<PlayerSkillController>();
        isStandUp = true;
        isKnockBackAnim = false;
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

    /// <summary>
    /// �v���C���[�p
    /// </summary>
    /// <param name="id"></param>
    public void SetInt(ANIM_ID id)
    {
        if (playerController.IsControlEnabled && GetAnimId() == (int)id) return;    // ����s�\���&&�����A�j���[�V�������Đ����悤�Ƃ����ꍇ

        if(id == ANIM_ID.Skill)
        {
            skillController.OnStartSkillAnim();
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
        if (skillController.isUsedSkill) return;

        if (id == (int)ANIM_ID.Skill)
        {
            skillController.OnStartSkillAnim();
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
    public void OnEndAnim()
    {
        if (skillController.isUsedSkill) skillController.OnEndSkillAnim();
        if (GetAnimId() != (int)ANIM_ID.Kick) playerController.IsInvincible = false;
        playerController.IsControlEnabled = true;
        SetInt(ANIM_ID.IdleB);
    }

    /// <summary>
    /// �m�b�N�o�b�N���o
    /// </summary>
    public void PlayKnockBackAnim()
    {
        OnEndAnim();

        isStandUp = false;
        isKnockBackAnim = true;
        animator.SetInteger("animation", (int)ANIM_ID.Die);
    }

    /// <summary>
    /// �����オ��A�j���[�V�������I������
    /// </summary>
    public void OnEndStandUpAnim() 
    {
        isStandUp = true;

        // ���G��Ԃ̃A�j���[�V����
        StartCoroutine(FlashCoroutine());
        SetInt(ANIM_ID.IdleB);
    }

    /// <summary>
    /// �_�ŏ���
    /// </summary>
    /// <returns></returns>
    IEnumerator FlashCoroutine()
    {
        playerController.IsInvincible = true;
        animator.SetBool("is_invincible", true);

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

        if(!skillController.isUsedSkill) playerController.IsInvincible = false;
        animator.SetBool("is_invincible", false);
    }
}
