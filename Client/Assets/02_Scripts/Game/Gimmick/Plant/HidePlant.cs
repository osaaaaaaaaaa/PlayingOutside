using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HidePlant : MonoBehaviour
{
    [SerializeField] float animSec;
    [SerializeField] GameObject damageColliderObj;
    const float endPointY = 0.3f;
    bool isSentRequest;
    bool isActive;

    // Start is called before the first frame update
    void Start()
    {
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
                if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient) ShowPlantAsynk();
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
            transform.DOLocalMove(new Vector3(localPos.x, endPointY, localPos.z), animSec).SetEase(Ease.OutElastic)
                    .OnComplete(() => { Destroy(damageColliderObj); });
        }
    }

    /// <summary>
    /// 植物のギミックを発動するリクエストを送信する
    /// </summary>
    public async void ShowPlantAsynk()
    {
        isSentRequest = true;
        if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
        {
            // マスタークライアントが植物のギミックを破棄する処理をリクエスト
            if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient)
                await RoomModel.Instance.TriggeringPlantGimmickAsynk(this.gameObject.name);
        }
    }
}
