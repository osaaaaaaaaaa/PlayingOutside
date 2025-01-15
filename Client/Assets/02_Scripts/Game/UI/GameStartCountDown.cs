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
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    public void PlayCountDownOverAnim()
    {
        audioSource.PlayOneShot(goSE);
        animator.Play("CountDownOverAnimation");
    }

    public void CallPlayAnim()
    {
        Invoke("PlayAnim", 1f);
    }

    void PlayAnim()
    {
        audioSource.PlayOneShot(hereweSE);
        animator.Play("CountDownAnimation");
    }

    public void OnEndAnim()
    {
        if (relayGameDirector != null) relayGameDirector.OnCountdownOver();
        if (finalGameDirector != null) finalGameDirector.OnCountdownOver();
    }
}
