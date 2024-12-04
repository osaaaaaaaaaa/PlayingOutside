using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaGoal : MonoBehaviour
{
    [SerializeField] AreaController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            other.gameObject.SetActive(false);

            // �T�[�o�[�Ȃ��Œʂ��ł��Ƃ��̃f�o�b�N�p
            StartCoroutine(controller.CurrentAreaClearCoroutine(other.gameObject));
        }
    }
}
