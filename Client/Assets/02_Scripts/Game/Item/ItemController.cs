using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    [SerializeField] EnumManager.ITEM_ID itemID;
    public EnumManager.ITEM_ID ItemId { get { return itemID; } }

    [SerializeField] GameObject model;

    bool isHit = false;
    float lifetime = 15;

    private void OnEnable()
    {
        StartCoroutine(DestroyCoroutin());
    }

    private async void OnTriggerEnter(Collider other)
    {
        // プレイヤーに触れた場合
        if (other.gameObject.layer == 3 && !isHit)
        {
            bool isPlayAudio = false;
            if (RoomModel.Instance.userState != RoomModel.USER_STATE.joined) isPlayAudio = true;
            if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined && other.GetComponent<PlayerItemController>().enabled) isPlayAudio = true;

            if (ItemId == EnumManager.ITEM_ID.Coin)
            {
                isHit = true;
                await RoomModel.Instance.GetItemAsynk(itemID, this.name);

                // チュートリアルで使用する想定
                if (RoomModel.Instance.userState != RoomModel.USER_STATE.joined)
                {
                    if (isPlayAudio) other.GetComponent<PlayerAudioController>().PlayOneShot(PlayerAudioController.AudioClipName.item_get);
                    Destroy(this.gameObject);
                }
            }
            else if (other.GetComponent<PlayerItemController>().slotItemId == EnumManager.ITEM_ID.None)
            {
                isHit = true;
                await RoomModel.Instance.GetItemAsynk(itemID, this.name);

                // チュートリアルで使用する想定
                if (RoomModel.Instance.userState != RoomModel.USER_STATE.joined)
                {
                    if (isPlayAudio) other.GetComponent<PlayerAudioController>().PlayOneShot(PlayerAudioController.AudioClipName.item_get);
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

    IEnumerator DestroyCoroutin()
    {
        const float flashingTime = 5;
        const float flashingCnt = 20;
        while (lifetime > flashingTime)
        {
            lifetime--;
            yield return new WaitForSeconds(1);
        }

        while (lifetime > 0)
        {
            lifetime -= flashingTime / flashingCnt;
            model.SetActive(!model.activeSelf);
            yield return new WaitForSeconds(flashingTime / flashingCnt);
        }

        if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
        {
            if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient) DestroyItemAsynk();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void DestroyItemAsynk()
    {
        await RoomModel.Instance.DestroyItemAsynk(this.name);
    }
}
