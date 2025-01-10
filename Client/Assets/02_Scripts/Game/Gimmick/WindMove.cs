using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindMove : MonoBehaviour
{
    [SerializeField] Vector3 windVec;

    private void OnTriggerStay(Collider other)
    {
        var controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            if(controller.enabled) controller.Rb.AddForce(windVec,ForceMode.Acceleration);
        }
    }
}
