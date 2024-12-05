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
        // カメラのターゲットの切り替えに成功した場合
        if (camera.SearchAndChangeTarget())
        {
            textTargetName.SetActive(true);
            btnChangeTarget.SetActive(true);

            // 名前取得＆テキスト変更
            var name = RoomModel.Instance.JoinedUsers[camera.currentTargetId].UserData.Name;
            textTargetName.GetComponent<Text>().text = name + "を観戦中";
            SetupButton(true);
        }

        // カメラのターゲットの対象が１人しかいない(これ以上切り替えられない)場合
        if (camera.activeTargetCnt == 1) SetupButton(false);
    }

    public void SetupButton(bool interactable)
    {
        btnChangeTarget.GetComponent<Button>().interactable = interactable;
    }
}
