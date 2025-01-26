using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lotus : MonoBehaviour
{
    [SerializeField] float addPower;
    const float jumpPower = -1f;
    const float duration = 0.5f;
    SEController controller;
    Vector3 startPos;

    private void OnEnable()
    {
        controller = GetComponent<SEController>();
        startPos = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var compornent = collision.gameObject.GetComponent<PlayerController>();
        if(compornent != null)
        {
            if (compornent.enabled)
            {
                var rb = collision.gameObject.GetComponent<Rigidbody>();
                if (rb == null) return;

                if (collision.gameObject.layer == 3)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    compornent.Jump(addPower);
                }
                else
                {
                    // ノックダウン状態などの場合
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    rb.AddForce(collision.gameObject.transform.up * addPower);
                }
            }
        }
    }

    public void OnAnimTrigger(GameObject character)
    {
        if (character.layer != 8)
        {
            // ノックダウン状態以外の場合はジャンプアニメーション
            character.GetComponent<PlayerAnimatorController>().SetInt(PlayerAnimatorController.ANIM_ID.Jump);
        }

        PlayTween();
        controller.PlayAudio();
    }

    void PlayTween()
    {
        if (DOTween.IsTweening(this.gameObject))
        {
            DOTween.Kill(this.gameObject);
            transform.position = startPos;
        }

        transform.DOJump(startPos, jumpPower, 1, duration).SetEase(Ease.OutElastic);
    }
}
