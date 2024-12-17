using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimatorController : MonoBehaviour
{
    Animator animator;

    // 立ち上がったかどうか
    public bool isStandUp { get; private set; }
    // ノックバック中かどうか
    bool isKnockBackAnim;
    // 無敵状態かどうか
    public bool isInvincible { get; private set; }
    // 操作が可能かどうか
    public bool isControlEnabled { get; private set; }

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
        Damage = 10,
        Die = 11,
        Run = 15,
        StandUp02 = 20,
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        isStandUp = true;
        isKnockBackAnim = false;
        isControlEnabled = true;
    }

    private void Update()
    {
        if (isKnockBackAnim && GetComponent<PlayerController>().GetComponent<PlayerIsGroundController>().IsGround())
        {
            // ノックバックを終了し、アニメーション再生
            isKnockBackAnim = false;
            var angle = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, angle.y, angle.z);
            animator.SetInteger("animation", (int)ANIM_ID.StandUp02);
        }
    }

    /// <summary>
    /// プレイヤー用
    /// </summary>
    /// <param name="id"></param>
    public void SetInt(ANIM_ID id)
    {
        if (isControlEnabled && GetAnimId() == (int)id) return;    // 操作不能状態&&同じアニメーションを再生しようとした場合

        if(id == ANIM_ID.Damage)
        {
            isInvincible = true;
            animator.Play("Damage");
        }
        if(id == ANIM_ID.Kick || id == ANIM_ID.Damage)
        {
            isControlEnabled = false;
        }

        animator.SetInteger("animation", (int)id);
    }

    /// <summary>
    /// NPC用
    /// </summary>
    /// <param name="id"></param>
    public void SetInt(int id)
    {
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
        if (GetAnimId() != (int)ANIM_ID.Kick) isInvincible = false;
        isControlEnabled = true;
        SetInt(ANIM_ID.IdleB);
    }

    /// <summary>
    /// ノックバック演出
    /// </summary>
    public void PlayKnockBackAnim()
    {
        OnEndAnim();

        isStandUp = false;
        isKnockBackAnim = true;
        animator.SetInteger("animation", (int)ANIM_ID.Die);
    }

    /// <summary>
    /// 立ち上がるアニメーションが終了した
    /// </summary>
    public void EndStandUpAnim() 
    {
        isStandUp = true;

        // 無敵状態のアニメーション
        StartCoroutine(FlashCoroutine());
        SetInt(ANIM_ID.IdleB);
    }

    /// <summary>
    /// 点滅処理
    /// </summary>
    /// <returns></returns>
    IEnumerator FlashCoroutine()
    {
        isInvincible = true;
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

        isInvincible = false;
        animator.SetBool("is_invincible", false);
    }
}
