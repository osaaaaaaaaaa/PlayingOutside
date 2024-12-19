using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hay : MonoBehaviour
{
    Rigidbody rb;
    Vector3 addForceVec;
    bool isInit = false;

    private void FixedUpdate()
    {
        if (isInit)
        {
            rb.AddForce(addForceVec, ForceMode.Acceleration);
        }
    }

    public void Init(Vector3 force)
    {
        rb = GetComponent<Rigidbody>();
        addForceVec = force;
        isInit = true;

        rb.AddForce(force * 8, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "DeathArea")
        {
            Destroy(this.gameObject);
        }
    }
}
