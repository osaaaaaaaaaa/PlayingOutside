using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class SpectatingUI : MonoBehaviour
{
    [SerializeField] GameObject textTargetName;
    [SerializeField] GameObject btnChangeTarget;

    [SerializeField] TargetCameraController camera;

    public void InitUI(bool isActiveSelf)
    {
        this.gameObject.SetActive(isActiveSelf);
        if (isActiveSelf)
        {
            OnChangeTargetBtn();
        }
        else
        {
            textTargetName.SetActive(false);
            btnChangeTarget.SetActive(false);
        }
    }

    public void OnChangeTargetBtn()
    {
        // �J�����̃^�[�Q�b�g�̐؂�ւ��ɐ��������ꍇ
        if (camera.SearchAndChangeTarget())
        {
            textTargetName.SetActive(true);
            btnChangeTarget.SetActive(true);

            // ���O�擾���e�L�X�g�ύX
            var name = RoomModel.Instance.JoinedUsers[camera.currentTargetId].UserData.Name;
            textTargetName.GetComponent<Text>().text = name + "���ϐ풆";
            SetupButton(true);
        }

        // �J�����̃^�[�Q�b�g�̑Ώۂ��P�l�������Ȃ�(����ȏ�؂�ւ����Ȃ�)�ꍇ
        if (camera.activeTargetCnt == 1) SetupButton(false);
    }

    public void SetupButton(bool interactable)
    {
        btnChangeTarget.GetComponent<Button>().interactable = interactable;
    }
}
