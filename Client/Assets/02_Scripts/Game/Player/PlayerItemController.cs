using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.Model.Entity.EnumManager;

public class PlayerItemController : MonoBehaviour
{
    #region コントローラー関係
    PlayerController playerController;
    PlayerEffectController effectController;
    PlayerAudioController audioController;
    #endregion

    #region アイテム関係
    public EnumManager.ITEM_ID slotItemId = EnumManager.ITEM_ID.None;
    public Dictionary<EnumManager.ITEM_ID, float> itemEffectTimeList { get; private set; } = new Dictionary<EnumManager.ITEM_ID, float>();
    bool isOnUseButton = false; // アイテム使用リクエストが完了するまで連続で押せないようにする
    #endregion

    private void OnDisable()
    {
        ClearAllItemEffects();
    }

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        effectController = GetComponent<PlayerEffectController>();
        audioController = GetComponent<PlayerAudioController>();
    }

    private void Update()
    {
        List<EnumManager.ITEM_ID> keyList = new List<EnumManager.ITEM_ID>(itemEffectTimeList.Keys);
        List<EnumManager.ITEM_ID> effectsToRemove = new List<EnumManager.ITEM_ID>();

        // アイテムの効果時間を計測する
        foreach (var key in keyList)
        {
            itemEffectTimeList[key] -= Time.deltaTime;

            if (itemEffectTimeList[key] <= 0)
            {
                effectsToRemove.Add(key);
            }
        }

        // 終了した効果を削除
        foreach (var key in effectsToRemove)
        {
            ClearItem(key);
            itemEffectTimeList.Remove(key);
        }
    }

    public void ClearAllItemEffects()
    {
        foreach (var key in itemEffectTimeList.Keys)
        {
            ClearItem(key);
        }
        itemEffectTimeList.Clear();
    }

    void ClearItem(EnumManager.ITEM_ID itemId)
    {
        switch (itemId)
        {
            case EnumManager.ITEM_ID.Pepper:
                playerController.speed -= playerController.addPepperSpeed;
                effectController.ClearParticle(PlayerEffectController.EFFECT_ID.PepperFire);
                break;
        }
    }

    /// <summary>
    /// アイテムスロットに設定する
    /// </summary>
    /// <param name="itemId"></param>
    public void SetItemSlot(EnumManager.ITEM_ID itemId)
    {
        if(itemId != ITEM_ID.None && itemId != ITEM_ID.ItemBox && itemId != ITEM_ID.Coin) slotItemId = itemId;
    }

    /// <summary>
    /// アイテム使用処理
    /// </summary>
    /// <param name="_itemId"></param>
    public void UseItem(EnumManager.ITEM_ID _itemId)
    {
        if(_itemId != ITEM_ID.None) slotItemId = _itemId;
        int itemEffectTime = 0;
        switch (slotItemId)
        {
            case EnumManager.ITEM_ID.Pepper:
                itemEffectTime = (int)EnumManager.ITEM_EFFECT_TIME.Pepper;
                if (!itemEffectTimeList.ContainsKey(EnumManager.ITEM_ID.Pepper)) playerController.speed += playerController.addPepperSpeed;
                effectController.SetEffect(PlayerEffectController.EFFECT_ID.PepperFire);
                audioController.PlayOneShot(PlayerAudioController.AudioClipName.item_pepper);
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

    /// <summary>
    /// アイテム使用ボタン
    /// </summary>
    public async void OnUseItemButton()
    {
        if (isOnUseButton) return;
        isOnUseButton = true;
        // アイテム使用リクエスト送信
        await RoomModel.Instance.UseItemAsynk(slotItemId);


        // 一旦
        if(RoomModel.Instance.userState != RoomModel.USER_STATE.joined) UseItem(slotItemId);
    }
}
