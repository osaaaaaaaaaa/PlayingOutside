//*********************************************************
// 一時的にキャラクターの親オブジェクトを変更する
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParentSetter : MonoBehaviour
{
    List<GameObject> children = new List<GameObject>();

    private void OnCollisionStay(Collision collision)
    {
        if (!children.Contains(collision.gameObject))
        {
            var compornent = collision.gameObject.GetComponent<PlayerController>();
            if (compornent != null)
            {
                if (compornent.enabled)
                {
                    children.Add(compornent.gameObject);
                    collision.transform.parent = this.transform;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (children.Contains(collision.gameObject))
        {
            collision.transform.parent = null;
            children.Remove(collision.gameObject);
        }
    }
}
