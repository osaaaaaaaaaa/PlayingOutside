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
        Jump = 3,
        Kick = 5,
        Skill = 6,
        Damage = 10,
        Die = 11,
        Run = 15,
        RunFast = 18,
        StandUp = 20,
        Respawn = 21,
    }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        skillController = GetComponent<PlayerSkillController>();
        isKnockBackAnim = false;
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

    /// <summary>
    /// プレイヤー用
    /// </summary>
    /// <param name="id"></param>
    public void SetInt(ANIM_ID id)
    {
        if (playerController.IsControlEnabled && GetAnimId() == (int)id) return;    // 操作不能状態&&同じアニメーションを再生しようとした場合

        if (id == ANIM_ID.Skill)
        {
            skillController.OnStartSkillAnim();
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
        if (skillController.isUsedSkill) return;

        if (id == (int)ANIM_ID.Skill)
        {
            skillController.OnStartSkillAnim();
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
    public void OnEndAnim()
    {
        if (skillController.isUsedSkill) skillController.OnEndSkillAnim();
        if (GetAnimId() != (int)ANIM_ID.Kick) playerController.IsInvincible = false;
        playerController.IsControlEnabled = true;
        SetInt(ANIM_ID.IdleB);
    }

    /// <summary>
    /// ノックバック演出
    /// </summary>
    public void PlayKnockBackAnim()
    {
        OnEndAnim();

        playerController.IsStandUp = false;
        isKnockBackAnim = true;
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

        if (!skillController.isUsedSkill) playerController.IsInvincible = false;
        animator.SetBool("is_invincible", false);
    }
}
