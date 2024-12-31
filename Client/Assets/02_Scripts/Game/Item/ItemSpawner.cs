using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] List<GameObject> itemPrefabs = new List<GameObject>(); // ìÇêhéq~
    [SerializeField] Transform rangeMin;
    [SerializeField] Transform rangeMax;
    [SerializeField] float spawnIntervalMin = 5f;
    [SerializeField] float spawnIntervalMax = 25f;
    float spawnTime;

    private void OnEnable()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private void OnDisable()
    {
        StopCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            spawnTime = UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(spawnTime);

            if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
            {
                if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient) SpawnItemAsynk();
            }
            else
            {
                Spawn(GetSpawnPoint(), GetSpawnItemId(), "localItem");
            }
        }
    }

    async void SpawnItemAsynk()
    {
        await RoomModel.Instance.SpawnItemAsynk(GetSpawnPoint(), GetSpawnItemId());
    }

    public GameObject Spawn(Vector3 spawnPoint,EnumManager.ITEM_ID itemId,string itemName)
    {
        GameObject spawnItem = null;
        foreach (var prefab in itemPrefabs) 
        { 
            if(itemId == prefab.GetComponent<ItemController>().ItemId)
            {
                spawnItem = Instantiate(prefab, spawnPoint, Quaternion.identity);
                spawnItem.name = itemName;
                break;
            }
        }

        return spawnItem;
    }

    Vector3 GetSpawnPoint()
    {
        int x = Random.Range((int)rangeMin.position.x, (int)rangeMax.position.x);
        int y = Random.Range((int)rangeMin.position.y, (int)rangeMax.position.y);
        int z = Random.Range((int)rangeMin.position.z, (int)rangeMax.position.z);
        return new Vector3(x, y, z);
    }

    EnumManager.ITEM_ID GetSpawnItemId()
    {
        int index = Random.Range(0, itemPrefabs.Count);
        return itemPrefabs[index].GetComponent<ItemController>().ItemId;
    }
}
