using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    [SerializeField] EnumManager.ITEM_ID itemID;
    public EnumManager.ITEM_ID ItemId { get { return itemID; } }

    bool isHit = false;

    private async void OnTriggerEnter(Collider other)
    {
        // プレイヤーに触れた場合
        if (other.gameObject.layer == 3 && !isHit)
        {
            if (other.GetComponent<PlayerItemController>().slotItemId == EnumManager.ITEM_ID.None)
            {
                isHit = true;
                await RoomModel.Instance.OnGetItemAsynk(itemID, this.name);

                // チュートリアルで使用する想定
                if (RoomModel.Instance.userState != RoomModel.USER_STATE.joined)
                {
                    other.GetComponent<PlayerItemController>().SetItemSlot(ItemId);
                    Destroy(this.gameObject);
                }
            }
        }
        // 場外のバリアに触れた場合
        else if (other.gameObject.layer == 10 && !isHit)
        {
            Destroy(this.gameObject);
        }
    }
}
