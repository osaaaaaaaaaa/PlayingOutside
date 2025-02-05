//*********************************************************
// 各エリアをゴールしたときのスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerEffectController;

public class AreaGoal : MonoBehaviour
{
    [SerializeField] AreaController controller;

    private void OnCollisionEnter(Collision collision)
    {
        var compornent = collision.gameObject.GetComponent<PlayerController>();
        if (compornent != null)
        {
            if (compornent.enabled)
            {
                collision.gameObject.GetComponent<PlayerEffectController>().SetEffect(EFFECT_ID.AreaCleared);
                this.GetComponent<BoxCollider>().enabled = false;
                collision.gameObject.SetActive(false);
                StartCoroutine(controller.CurrentAreaClearCoroutine(collision.gameObject));
            }
        }
    }
}
