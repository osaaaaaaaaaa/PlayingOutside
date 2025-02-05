//*********************************************************
// メガサイズの鶏小屋ギミックのスクリプト
// Author:Rui Enomoto
//*********************************************************
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class MegaCoop : MonoBehaviour
{
    [SerializeField] SEController seController;
    [SerializeField] MegaChickenAnim megaChicken;
    Vector3 startPos;
    bool isPlayAnim;

    private void Start()
    {
        startPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlayAnim) return;

        // ダメージコライダーの取得チェック
        var damageCompornent = other.GetComponent<DamageCollider>();
        if (damageCompornent == null) return;

        var playerController = damageCompornent.root.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // NPCではない場合
            if (playerController.gameObject.layer == 3)
            {
                if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
                {
                    PlayCoopGimmickAsynk();
                }
                else
                {
                    PlayDamageAnim();
                }
            }
        }
    }

    /// <summary>
    /// ダメージを受けるアニメーションを再生する
    /// </summary>
    public void PlayDamageAnim()
    {
        if (isPlayAnim) return;
        isPlayAnim = true;
        seController.PlayAudio();
        DOTween.Kill(this.gameObject);
        transform.position = startPos;
        transform.DOPunchPosition(new Vector3(1f, 0, 0), 1f, 10, 1.5f).OnComplete(megaChicken.PlayShowAnim);
    }

    /// <summary>
    /// ギミック発動リクエスト
    /// </summary>
    async void PlayCoopGimmickAsynk()
    {
        // Tween発動リクエスト
        await RoomModel.Instance.TriggerMegaCoopAsynk();
    }

    /// <summary>
    /// メガ鶏が隠れる処理が完了したとき
    /// </summary>
    public async void OnHideMegaChickenTween()
    {
        if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
        {
            await RoomModel.Instance.TriggerMegaCoopEndAsynk();
        }
        else
        {
            isPlayAnim = false;
        }   
    }

    /// <summary>
    /// 鶏小屋のギミック終了通知を受信したとき
    /// </summary>
    public void OnTriggerEnd()
    {
        isPlayAnim = false;
    }
}
