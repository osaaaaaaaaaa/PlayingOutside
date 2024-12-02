using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mud : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerController>())
        {
            other.GetComponent<PlayerController>().Speed = 2f;
            //other.GetComponent<PlayerController>().JumpPower = 250;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            other.GetComponent<PlayerController>().Speed = other.GetComponent<PlayerController>().defaultSpeed;
            //other.GetComponent<PlayerController>().JumpPower = other.GetComponent<PlayerController>().defaultJumpPower;
        }
    }
}
