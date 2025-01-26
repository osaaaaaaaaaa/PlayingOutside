using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LotusAnimTrigger : MonoBehaviour
{
    [SerializeField] Lotus lotus;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Character")
        {
            lotus.OnAnimTrigger(other.gameObject);
        }
    }
}
