using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MegaChickenAnim : MonoBehaviour
{
    [SerializeField] MegaCoop megaCoop;
    [SerializeField] ChickenGimmick chickenGimmick;
    [SerializeField] CapsuleCollider capsuleCollider;
    [SerializeField] BoxCollider damageBoxCollider;
    [SerializeField] BoxCollider chickenGimmickCollider;
    [SerializeField] Vector3 animStartPos;
    [SerializeField] Vector3 animEndPos;
    [SerializeField] float animDuration;

    private void Awake()
    {
        capsuleCollider.enabled = false;
        damageBoxCollider.enabled = true;
        chickenGimmickCollider.enabled = false;
        chickenGimmick.enabled = false;
    }

    public void PlayShowAnim()
    {
        var endMoveVal = animEndPos;
        var endSvaleVal = Vector3.one * 5;
        Ease ease = Ease.OutBack;

        DOTween.Kill(this.gameObject);
        transform.position = animStartPos;
        transform.localScale = Vector3.one;

        var secuense = DOTween.Sequence();
        secuense.Append(transform.DOLocalMove(endMoveVal, animDuration).SetEase(ease))
            .Join(transform.DOScale(endSvaleVal, animDuration)).SetEase(ease);
        secuense.Play().OnComplete(() => { 
            capsuleCollider.enabled = true;
            damageBoxCollider.enabled = false;
            chickenGimmickCollider.enabled = true;
            chickenGimmick.enabled = true;
            chickenGimmick.InitParam();
        });
    }

    public void PlayHideAnim()
    {
        var endMoveVal = animStartPos;
        var endSvaleVal = Vector3.one;
        Ease ease = Ease.InBack;

        DOTween.Kill(this.gameObject);
        transform.position = animEndPos;
        transform.localScale = Vector3.one * 5;

        var secuense = DOTween.Sequence();
        secuense.Append(transform.DOLocalMove(endMoveVal, animDuration).SetEase(ease))
            .Join(transform.DOScale(endSvaleVal, animDuration)).SetEase(ease);
        secuense.Play().OnComplete(()=> {
            capsuleCollider.enabled = false;
            damageBoxCollider.enabled = true;
            chickenGimmickCollider.enabled = false;
            chickenGimmick.enabled = false;
            megaCoop.OnHideMegaChickenTween();
        });
    }
}
