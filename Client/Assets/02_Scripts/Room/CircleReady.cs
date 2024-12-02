using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleReady : MonoBehaviour
{
    [SerializeField] RoomDirector roomDirector;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            roomDirector.OnReadyCircle(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            roomDirector.OnReadyCircle(false);
        }
    }
}
