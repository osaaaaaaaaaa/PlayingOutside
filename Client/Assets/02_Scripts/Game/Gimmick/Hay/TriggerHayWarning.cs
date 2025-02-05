//*********************************************************
// �q�����[���������Ă��邱�Ƃ��v���C���[�ɒm�点��X�N���v�g
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHayWarning : MonoBehaviour
{
    [SerializeField] GameObject warningUiGroup;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9) warningUiGroup.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 9) warningUiGroup.SetActive(false);
    }
}
