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
    /// ”jŠü‚·‚éA•¨‚Ì’Š‘I
    /// </summary>
    /// <returns></returns>
    int[] LotteryDestroyIndexs()
    {
        int[] indexs = new int[destroyCnt];
        for (int i = 0; i < indexs.Length; i++)
        {
            indexs[i] = 99999;  // ”äŠr‚Å‚«‚È‚­‚È‚é‚½‚ßA‰Šú’l‚ğ0‚©‚ç•Ï‚¦‚é
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
    /// ”jŠü‚·‚é‘ÎÛ‚Ì–¼‘Oæ“¾
    /// </summary>
    /// <returns></returns>
    public string[] GetDestroyPlantNames()
    {
        // ”jŠüÏ‚İ‚Å‚ ‚ê‚Îˆ—‚ğI—¹
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
                Debug.Log("”jŠü‚·‚é–¼‘OF" + names[i]);
            }
            else
            {
                names[i] = "";
                Debug.Log(this.name + "‚Ì’Š‘I‚Ånull‚ª”­¶[" + index + "]");
            }
            i++;
        }

        return names;

    }
    /// <summary>
    /// A•¨‚ğ”jŠü‚·‚é
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
