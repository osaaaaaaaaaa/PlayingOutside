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

    private void Awake()
    {
        seController = GetComponent<SEController>();
        animator = GetComponent<Animator>();
    }

    public void PlayCountDownOverAnim()
    {
        seController.PlayAudio(goSE);
        animator.Play("CountDownOverAnimation");
    }

    public void CallPlayAnim()
    {
        Invoke("PlayAnim", 1f);
    }

    void PlayAnim()
    {
        seController.PlayAudio(hereweSE);
        animator.Play("CountDownAnimation");
    }

    public void OnEndAnim()
    {
        if (relayGameDirector != null) relayGameDirector.OnCountdownOver();
        if (finalGameDirector != null) finalGameDirector.OnCountdownOver();
    }
}
