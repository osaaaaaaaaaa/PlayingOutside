using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_DAMAGCUBE : MonoBehaviour
{
    [SerializeField] BoxCollider boxCollider;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            boxCollider.enabled = !boxCollider.enabled;
        }
    }
}
