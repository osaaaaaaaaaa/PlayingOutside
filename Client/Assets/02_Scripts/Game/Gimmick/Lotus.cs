using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lotus : MonoBehaviour
{
    [SerializeField] float addPower;

    private void OnCollisionEnter(Collision collision)
    {
        var compornent = collision.gameObject.GetComponent<PlayerController>();
        if(compornent != null)
        {
            if (compornent.enabled)
            {
                var rb = collision.gameObject.GetComponent<Rigidbody>();
                if (rb == null) return;

                if (collision.gameObject.layer == 3)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    compornent.Jump(addPower);
                }
                else
                {
                    // ノックダウン状態などの場合
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    rb.AddForce(collision.gameObject.transform.up * addPower);
                }
            }
        }
    }
}
