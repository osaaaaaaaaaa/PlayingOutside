using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEventTrigger : MonoBehaviour
{
    PlayerController playerController;
    PlayerAnimatorController animController;
    PlayerEffectController effectController;

    public enum EVENT_ID
    {
        OnEndAnim = 0,
        OnStartDownAnim,
        OnEndStandUpAnim,
        SetEffect_Down,
        SetEffect_Run,
        SetEffect_KnockBackSmoke,
    }

    private void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
        animController = transform.parent.GetComponent<PlayerAnimatorController>();
        effectController = transform.parent.GetComponent<PlayerEffectController>();
    }

    public void OnTriggerAnimEvent(EVENT_ID id)
    {
        switch (id)
        {
            case EVENT_ID.OnEndAnim:
                animController.OnEndAnim();
                break;
            case EVENT_ID.OnStartDownAnim:
                transform.parent.gameObject.layer = 8;  // 他のプレイヤーとの当たり判定を一時的に無くす
                break;
            case EVENT_ID.OnEndStandUpAnim:
                // 他のプレイヤーとの当たり判定を戻す
                if (playerController.enabled) transform.parent.gameObject.layer = 3;    // Player
                if (!playerController.enabled) transform.parent.gameObject.layer = 7; // NPC

                animController.OnEndStandUpAnim();
                break;
            case EVENT_ID.SetEffect_Down:
                effectController.SetEffect(PlayerEffectController.EFFECT_ID.Down);
                break;
            case EVENT_ID.SetEffect_Run:
                effectController.SetEffect(PlayerEffectController.EFFECT_ID.Run);
                break;
            case EVENT_ID.SetEffect_KnockBackSmoke:
                effectController.SetEffect(PlayerEffectController.EFFECT_ID.KnockBackSmoke);
                break;
        }
    }
}
