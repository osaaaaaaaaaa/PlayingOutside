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
    /// �j������A���̒��I
    /// </summary>
    /// <returns></returns>
    int[] LotteryDestroyIndexs()
    {
        int[] indexs = new int[destroyCnt];
        for (int i = 0; i < indexs.Length; i++)
        {
            indexs[i] = 99999;  // ��r�ł��Ȃ��Ȃ邽�߁A�����l��0����ς���
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
    /// �j������Ώۂ̖��O�擾
    /// </summary>
    /// <returns></returns>
    public string[] GetDestroyPlantNames()
    {
        // �j���ς݂ł���Ώ������I��
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
                Debug.Log("�j�����閼�O�F" + names[i]);
            }
            else
            {
                names[i] = "";
                Debug.Log(this.name + "�̒��I��null������[" + index + "]");
            }
            i++;
        }

        return names;

    }
    /// <summary>
    /// �A����j������
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
