using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerEffectController;

public class AreaGoal : MonoBehaviour
{
    [SerializeField] AreaController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 7)
        {
            other.GetComponent<PlayerEffectController>().SetEffect(EFFECT_ID.AreaCleared);
        }
        if (other.gameObject.layer == 3)
        {
            other.gameObject.SetActive(false);

            // サーバーなしで通しでやるときのデバック用
            StartCoroutine(controller.CurrentAreaClearCoroutine(other.gameObject));
        }
    }
}
