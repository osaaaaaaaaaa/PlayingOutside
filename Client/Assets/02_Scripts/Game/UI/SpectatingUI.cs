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
            textTargetName.GetComponent<Text>().text = name;
        }
        // �J�����̃^�[�Q�b�g�̐؂�ւ��悪���݂��Ȃ��ꍇ
        else
        {
            this.gameObject.SetActive(false);
            textTargetName.SetActive(false);
            btnChangeTarget.SetActive(false);
        }

        // �J�����̃^�[�Q�b�g�̑Ώۂ��P�l�������Ȃ��ꍇ
        if (camera.activeTargetCnt == 1) btnChangeTarget.GetComponent<Button>().interactable = false;
    }
}
