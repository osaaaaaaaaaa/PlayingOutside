using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    Vector3 defaultPower = new Vector3(7, 12, 7);

    /// <summary>
    /// ÉhÉçÉbÉvèàóù
    /// </summary>
    public void Drop(int angleY)
    {
        transform.Rotate(Vector3.up * angleY);
        Vector3 forwardSelf = transform.forward.y <= 0 ? transform.forward + Vector3.up : transform.forward;
        Vector3 vector = Vector3.Scale(forwardSelf, defaultPower);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().AddForce(vector,ForceMode.Impulse);
    }
}
