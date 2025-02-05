//*********************************************************
// 指定したプレファブを生成するスクリプト
// Author:Rui Enomoto
//*********************************************************
using Cysharp.Threading.Tasks.Triggers;
using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPrefabController : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabList;

    public void Spawn(SpawnObject spawnObject)
    {
        var obj = Instantiate(prefabList[(int)spawnObject.objectId]);
        obj.name = spawnObject.name;
        obj.transform.position = spawnObject.position;
        obj.transform.eulerAngles = spawnObject.angle;
        switch (spawnObject.objectId) 
        {
            case EnumManager.SPAWN_OBJECT_ID.Hay:
                obj.GetComponent<Hay>().Init(spawnObject.forse);
                break;
        }
    }
}
