using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    [SerializeField] Item.ITEM_ID itemID;
    bool isHit = false;

    private async void OnTriggerEnter(Collider other)
    {
        // �v���C���[�ɐG�ꂽ�ꍇ
        if (other.gameObject.layer == 3 && !isHit)
        {
            isHit = true;
            await RoomModel.Instance.OnGetItemAsynk(itemID, this.name);
        }
        // ��O�̃o���A�ɐG�ꂽ�ꍇ
        else if (other.gameObject.layer == 10 && !isHit)
        {
            Destroy(this.gameObject);
        }
    }

    public void UseItem()
    {
        switch (itemID) 
        {
            case Item.ITEM_ID.Coin:
                break;
        }

        Destroy(this.gameObject);
    }
}
