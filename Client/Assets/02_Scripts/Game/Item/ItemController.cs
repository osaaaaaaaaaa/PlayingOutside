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
        // �v���C���[�ɐG�ꂽ�ꍇ
        if (other.gameObject.layer == 3 && !isHit)
        {
            if (other.GetComponent<PlayerItemController>().slotItemId == EnumManager.ITEM_ID.None)
            {
                isHit = true;
                await RoomModel.Instance.OnGetItemAsynk(itemID, this.name);

                // �`���[�g���A���Ŏg�p����z��
                if (RoomModel.Instance.userState != RoomModel.USER_STATE.joined)
                {
                    other.GetComponent<PlayerItemController>().SetItemSlot(ItemId);
                    Destroy(this.gameObject);
                }
            }
        }
        // ��O�̃o���A�ɐG�ꂽ�ꍇ
        else if (other.gameObject.layer == 10 && !isHit)
        {
            Destroy(this.gameObject);
        }
    }
}
