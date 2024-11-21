using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    Animator animator;

    public enum ANIM_ID
    {
        IdleA = 1,
        IdleB,
        Jump = 4,
        Run = 15
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
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
}
