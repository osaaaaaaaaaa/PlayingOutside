using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointController : MonoBehaviour
{
    Vector3 defaultPower = new Vector3(7, 20, 7);

    /// <summary>
    /// ドロップするときのベクトルを取得
    /// </summary>
    public void GetDropVector()
    {
        transform.Rotate(Vector3.up * Random.Range(1, 361));
        Vector3 forwardSelf = transform.forward.y <= 0 ? transform.forward + Vector3.up : transform.forward;
        Vector3 vector = Vector3.Scale(forwardSelf, defaultPower);
    }
}
