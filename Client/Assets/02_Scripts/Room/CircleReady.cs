//*********************************************************
// Room�V�[���ŏ����������ł���T�[�N���̃X�N���v�g
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleReady : MonoBehaviour
{
    [SerializeField] RoomDirector roomDirector;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            if(other.GetComponent<PlayerController>().enabled) roomDirector.OnReadyCircle(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            if (other.GetComponent<PlayerController>().enabled) roomDirector.OnReadyCircle(false);
        }
    }
}
