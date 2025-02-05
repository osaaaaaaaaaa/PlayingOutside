//*********************************************************
// 一定時間ごとにプレファブを生成するスクリプト
// Author:Rui Enomoto
//*********************************************************
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnObjectController : MonoBehaviour
{
    [SerializeField] GameObject testPrefab;
    public float waitSeconds;
    [SerializeField] float mulPower;
    [SerializeField] Vector3 addForse;
    [SerializeField] EnumManager.SPAWN_OBJECT_ID spawanObjId = EnumManager.SPAWN_OBJECT_ID.Hay;

    private void Start()
    {
        StartCoroutine(GenerateCoroutine());
    }

    IEnumerator GenerateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitSeconds);

            if(RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
            {
                if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient) SpawnObjectAsynk();
            }
            else
            {
                var obj = Instantiate(testPrefab, transform);

                switch (spawanObjId) 
                {
                    case EnumManager.SPAWN_OBJECT_ID.Hay:
                        obj.GetComponent<Hay>().Init(addForse * mulPower);
                        break;
                }
            }
        }
    }

    async void SpawnObjectAsynk()
    {
        var spawnObj = new SpawnObject()
        {
            name = Guid.NewGuid().ToString(),
            position = transform.position,
            angle = transform.eulerAngles,
            forse = addForse * mulPower,
            objectId = this.spawanObjId,
        };
        await RoomModel.Instance.SpawnObjectAsynk(spawnObj);
    }
}
