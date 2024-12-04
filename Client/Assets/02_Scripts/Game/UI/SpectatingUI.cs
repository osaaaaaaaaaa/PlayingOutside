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
            textTargetName.GetComponent<Text>().text = name;
        }
        // カメラのターゲットの切り替え先が存在しない場合
        else
        {
            this.gameObject.SetActive(false);
            textTargetName.SetActive(false);
            btnChangeTarget.SetActive(false);
        }

        // カメラのターゲットの対象が１人しかいない場合
        if (camera.activeTargetCnt == 1) btnChangeTarget.GetComponent<Button>().interactable = false;
    }
}
