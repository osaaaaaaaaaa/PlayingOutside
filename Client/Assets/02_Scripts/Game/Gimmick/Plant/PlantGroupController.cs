using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantGroupController : MonoBehaviour
{
    [SerializeField] List<GameObject> hidePlants = new List<GameObject>();
    [SerializeField] int destroyCnt;
    public Dictionary<string,HidePlant> HidePlants { get; private set; } = new Dictionary<string,HidePlant>();
    bool isRename = false;

    void Start()
    {
        DORename();

        if (RoomModel.Instance.userState != RoomModel.USER_STATE.joined)
        {
            DestroyPlants();
        }
    }

    void DORename()
    {
        if (isRename) return;
        isRename = true;
        for (int i = 0; i < hidePlants.Count; i++)
        {
            hidePlants[i].name = this.gameObject.name + "_Plant" + i;
            HidePlants.Add(hidePlants[i].name, hidePlants[i].GetComponent<HidePlant>());
        }
    }

    /// <summary>
    /// ”jŠü‚·‚éA•¨‚Ì’Š‘I
    /// </summary>
    /// <returns></returns>
    int[] LotteryDestroyIndexs()
    {
        int[] indexs = new int[destroyCnt];
        for(int i = 0; i < indexs.Length; i++)
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
    /// A•¨‚ğ”jŠü‚·‚é
    /// </summary>
    /// <param name="indexs"></param>
    public string[] DestroyPlants()
    {
        // ”jŠüÏ‚İ‚Å‚ ‚ê‚Îˆ—‚ğI—¹
        for(int cnt = 0; cnt < hidePlants.Count; cnt++)
        {
            if (hidePlants[cnt] == null)
            {
                return new string[0];
            }
        }

        if (!isRename) DORename();

        int[] indexs = LotteryDestroyIndexs();
        string[] names = new string[indexs.Length];
        int i = 0;
        foreach(int index in indexs)
        {
            if(hidePlants[index] != null)
            {
                names[i] = hidePlants[index].name;
                Destroy(hidePlants[index]);
            }
        }

        return names;
    }
}
