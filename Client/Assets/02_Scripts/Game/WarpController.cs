using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpController : MonoBehaviour
{
    [SerializeField] AreaController areaController;
    [SerializeField] RelayGameDirector gameDirector;
    [SerializeField] List<Transform> points;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (gameDirector.isDebug)
            {
                GameObject.Find("01_OriginalHiyoko").transform.position = points[(int)areaController.currentAreaId].position;
            }
            else
            {
                if ((int)areaController.currentAreaId > points.Count) return;
                gameDirector.characterList[RoomModel.Instance.ConnectionId].transform.position = points[(int)areaController.currentAreaId].position;
            }
        }
    }
}
