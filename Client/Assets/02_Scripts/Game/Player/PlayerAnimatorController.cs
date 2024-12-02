using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimatorController : MonoBehaviour
{
    Animator animator;
    public bool isStandUp { get; private set; }
    bool isKnockBackAnim;

    public enum ANIM_ID
    {
        IdleA = 1,
        IdleB,
        Jump = 3,
        Die = 11,
        Run = 15,
        RunFast = 18,
        StandUp = 19,
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        isStandUp = true;
        isKnockBackAnim = false;
    }

    private void Update()
    {
        if (isKnockBackAnim && GetComponent<PlayerController>().IsGround())
        {
            // ノックバックを終了し、アニメーション再生
            isKnockBackAnim = false;
            var angle = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, angle.y, angle.z);
            animator.SetInteger("animation", (int)ANIM_ID.StandUp);
        }
    }

    public void SetInt(ANIM_ID id)
    {
        animator.SetInteger("animation", (int)id);
    }

    public void SetInt(int id)
    {
        animator.SetInteger("animation", id);
    }

    public int GetAnimId()
    {
        return animator.GetInteger("animation");
    }

    public void PlayKnockBackAnim()
    {
        isStandUp = false;
        isKnockBackAnim = true;
        animator.SetInteger("animation", (int)ANIM_ID.Die);
    }

    public void EndStandUpAnim() 
    {
        isStandUp = true;
        SetInt(ANIM_ID.IdleB);

        // 無敵状態のアニメーション
        StartCoroutine(GetComponent<PlayerController>().FlashCoroutine());
    }
}
