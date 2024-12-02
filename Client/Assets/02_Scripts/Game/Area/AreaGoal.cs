using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaGoal : MonoBehaviour
{
    [SerializeField] AreaController controller;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            other.gameObject.SetActive(false);


            // �{���͂����ŃG���A�N���A���������N�G�X�g

            // �T�[�o�[�Ȃ��Œʂ��ł��Ƃ��̃f�o�b�N�p
            controller.AreaGoal(true,other.gameObject);
        }
    }
}
