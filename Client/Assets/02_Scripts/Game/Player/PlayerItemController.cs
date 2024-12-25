using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.Model.Entity.EnumManager;

public class PlayerItemController : MonoBehaviour
{
    PlayerController playerController;
    PlayerEffectController effectController;

    public EnumManager.ITEM_ID slotItemId = EnumManager.ITEM_ID.None;
    public Dictionary<EnumManager.ITEM_ID, float> itemEffectTimeList { get; private set; } = new Dictionary<EnumManager.ITEM_ID, float>();
    bool isOnUseButton = false; // アイテム使用リクエストが完了するまで連続で押せないようにする

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        effectController = GetComponent<PlayerEffectController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnUseItemButton();
        }




        List<EnumManager.ITEM_ID> keyList = new List<EnumManager.ITEM_ID>(itemEffectTimeList.Keys);
        List<EnumManager.ITEM_ID> effectsToRemove = new List<EnumManager.ITEM_ID>();

        // アイテムの効果時間を計測する
        foreach (var key in keyList)
        {
            itemEffectTimeList[key] -= Time.deltaTime;

            if (itemEffectTimeList[key] <= 0)
            {
                Debug.Log(key.ToString() + "の効果が終了しました！");
                effectsToRemove.Add(key);
            }
        }

        // 終了した効果を削除
        foreach (var key in effectsToRemove)
        {
            switch (key)
            {
                case EnumManager.ITEM_ID.Pepper:
                    playerController.speed = playerController.defaultSpeed;
                    effectController.StopParticle(PlayerEffectController.EFFECT_ID.PepperFire);
                    break;
            }

            itemEffectTimeList.Remove(key);
        }
    }

    public void SetItemSlot(EnumManager.ITEM_ID itemId)
    {
        slotItemId = itemId;
    }

    public void UseItem(EnumManager.ITEM_ID _itemId)
    {
        if(_itemId != ITEM_ID.None) slotItemId = _itemId;
        int itemEffectTime = 0;
        switch (slotItemId)
        {
            case EnumManager.ITEM_ID.Pepper:
                itemEffectTime = (int)EnumManager.ITEM_EFFECT_TIME.Pepper;
                playerController.speed = playerController.pepperSpeed;
                effectController.SetEffect(PlayerEffectController.EFFECT_ID.PepperFire);
                break;
        }

        // 効果時間を設定する
        if(itemEffectTime > 0)
        {
            if (itemEffectTimeList.ContainsKey(slotItemId))
            {
                itemEffectTimeList[slotItemId] = itemEffectTime;
            }
            else
            {
                itemEffectTimeList.Add(slotItemId, itemEffectTime);
            }
        }

        slotItemId = EnumManager.ITEM_ID.None;
        isOnUseButton = false;
    }

    public async void OnUseItemButton()
    {
        if (isOnUseButton) return;
        isOnUseButton = true;
        // アイテム使用リクエスト送信
        await RoomModel.Instance.OnUseItemAsynk(slotItemId);


        // 一旦
        if(RoomModel.Instance.userState != RoomModel.USER_STATE.joined) UseItem(slotItemId);
    }
}
