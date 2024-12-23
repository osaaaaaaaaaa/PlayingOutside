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
        // プレイヤーに触れた場合
        if (other.gameObject.layer == 3 && !isHit)
        {
            isHit = true;
            await RoomModel.Instance.OnGetItemAsynk(itemID, this.name);
        }
        // 場外のバリアに触れた場合
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
