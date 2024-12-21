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
                GameObject.Find("Player_Original").transform.position = points[(int)areaController.areaId].position;
            }
            else
            {
                gameDirector.characterList[RoomModel.Instance.ConnectionId].transform.position = points[(int)areaController.areaId].position;
            }
        }
    }
}
