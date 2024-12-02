using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartCountDown : MonoBehaviour
{
    [SerializeField] GameDirector gameDirector;
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
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
        gameDirector.OnCountdownOver();
    }
}
