using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HidePlant : MonoBehaviour
{
    [SerializeField] float animSec;
    [SerializeField] GameObject damageColliderObj;
    [SerializeField] GameObject particleParent;
    SEController controller;
    const float endPointY = 0.3f;
    bool isSentRequest;
    bool isActive;

    private void OnEnable()
    {
        controller = GetComponent<SEController>();
        isSentRequest = false;
        isActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive || isSentRequest) return;

        var compornent = other.gameObject.GetComponent<PlayerController>();
        if (compornent != null)
        {
            if (!compornent.enabled) return;
            if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
            {
                ShowPlantAsynk();
            }
            else
            {
                ShowPlant();
            }
        }
    }

    public void ShowPlant()
    {
        if (isActive) return;
        isActive = true;

        var localPos = transform.localPosition;
        if (!DOTween.IsTweening(transform))
        {
            controller.PlayAudio();
            particleParent.SetActive(true);
            transform.DOLocalMove(new Vector3(localPos.x, endPointY, localPos.z), animSec).SetEase(Ease.OutElastic)
                    .OnComplete(() => { Destroy(damageColliderObj); });
        }
    }

    /// <summary>
    /// 植物のギミックを発動するリクエストを送信する
    /// </summary>
    public async void ShowPlantAsynk()
    {
        if(isSentRequest) return;
        isSentRequest = true;
        if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined) 
            await RoomModel.Instance.TriggeringPlantGimmickAsynk(this.gameObject.name);
    }
}
