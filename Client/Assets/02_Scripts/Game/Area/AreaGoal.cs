using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerEffectController;

public class AreaGoal : MonoBehaviour
{
    [SerializeField] AreaController controller;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 7 || collision.gameObject.layer == 8)
        {
            collision.gameObject.GetComponent<PlayerEffectController>().SetEffect(EFFECT_ID.AreaCleared);
        }
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            if (collision.gameObject.GetComponent<PlayerController>().enabled)
            {
                collision.gameObject.SetActive(false);
                StartCoroutine(controller.CurrentAreaClearCoroutine(collision.gameObject));
            }
        }
    }
}
