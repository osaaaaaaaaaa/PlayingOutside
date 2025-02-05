//*********************************************************
// ゲーム開始前のカウントダウンをするスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartCountDown : MonoBehaviour
{
    [SerializeField] RelayGameDirector relayGameDirector;
    [SerializeField] FinalGameDirector finalGameDirector;
    Animator animator;

    [SerializeField] AudioClip hereweSE;
    [SerializeField] AudioClip goSE;
    SEController seController;

    public enum SE_TYPE 
    {
        hereweSE,
        goSE,
    }

    private void Awake()
    {
        seController = GetComponent<SEController>();
        animator = GetComponent<Animator>();
    }

    public void PlayCountDownOverAnim()
    {
        PlayAudio(SE_TYPE.goSE);
        animator.Play("CountDownOverAnimation");
    }

    public void CallPlayAnim()
    {
        Invoke("PlayAnim", 1f);
    }

    void PlayAnim()
    {
        animator.Play("CountDownAnimation");
    }

    public void OnEndAnim()
    {
        if (relayGameDirector != null) relayGameDirector.OnCountdownOver();
        if (finalGameDirector != null) finalGameDirector.OnCountdownOver();
    }

    public void PlayAudio(SE_TYPE se)
    {
        switch (se)
        {
            case SE_TYPE.hereweSE:
                seController.PlayAudio(hereweSE);
                break;
            case SE_TYPE.goSE:
                seController.PlayAudio(goSE);
                break;
        }
    }
}
