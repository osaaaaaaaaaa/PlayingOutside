using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartCountDown : MonoBehaviour
{
    [SerializeField] RelayGameDirector relayGameDirector;
    [SerializeField] FinalGameDirector finalGameDirector;
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayCountDownOverAnim()
    {
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
        if(relayGameDirector != null) relayGameDirector.OnCountdownOver();
        if(finalGameDirector != null) finalGameDirector.OnCountdownOver();
    }
}
