using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlantGroupController : MonoBehaviour
{
    [SerializeField] List<GameObject> hidePlants = new List<GameObject>();
    [SerializeField] int destroyCnt;
    public Dictionary<string, HidePlant> HidePlantList { get; private set; } = new Dictionary<string, HidePlant>();

    private void OnEnable()
    {
        HidePlantList.Clear();
        foreach (var plant in hidePlants)
        {
            if(plant != null)
            HidePlantList.Add(plant.gameObject.name, plant.GetComponent<HidePlant>());
        }

        if (RoomModel.Instance.userState != RoomModel.USER_STATE.joined)
        {
            DestroyPlants(GetDestroyPlantNames());
        }
    }

    /// <summary>
    /// 破棄する植物の抽選
    /// </summary>
    /// <returns></returns>
    int[] LotteryDestroyIndexs()
    {
        int[] indexs = new int[destroyCnt];
        for (int i = 0; i < indexs.Length; i++)
        {
            indexs[i] = 99999;  // 比較できなくなるため、初期値を0から変える
        }

        for (int i = 0; i < indexs.Length; i++)
        {
            bool isSucsess = false;
            while (!isSucsess)
            {
                int rnd = Random.Range(0, hidePlants.Count);
                if (!indexs.Contains(rnd))
                {
                    indexs[i] = rnd;
                    isSucsess = true;
                }
            }
        }
        return indexs;
    }

    /// <summary>
    /// 破棄する対象の名前取得
    /// </summary>
    /// <returns></returns>
    public string[] GetDestroyPlantNames()
    {
        // 破棄済みであれば処理を終了
        for (int cnt = 0; cnt < hidePlants.Count; cnt++)
        {
            if (hidePlants[cnt] == null)
            {
                return new string[0];
            }
        }

        int[] indexs = LotteryDestroyIndexs();
        string[] names = new string[indexs.Length];
        int i = 0;
        foreach (int index in indexs)
        {
            if (hidePlants[index] != null)
            {
                names[i] = hidePlants[index].name;
                Debug.Log("破棄する名前：" + names[i]);
            }
            else
            {
                names[i] = "";
                Debug.Log(this.name + "の抽選でnullが発生[" + index + "]");
            }
            i++;
        }

        return names;

    }
    /// <summary>
    /// 植物を破棄する
    /// </summary>
    /// <param name="indexs"></param>
    public void DestroyPlants(string[] names)
    {
        foreach(var plant in hidePlants)
        {
            if (plant == null) continue;
            foreach (var name in names)
            {
                if(name == plant.name)
                {
                    Destroy(plant);
                }
            }
        }
    }
}
