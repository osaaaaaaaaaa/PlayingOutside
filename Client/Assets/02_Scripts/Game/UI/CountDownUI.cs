//*********************************************************
// �J�E���g�_�E������UI�̃X�N���v�g
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownUI : MonoBehaviour
{
    [SerializeField] List<Text> textCountDown;

    public void UpdateText(int currentSec)
    {
        foreach (var item in textCountDown)
        {
            item.text = currentSec.ToString();
        }
    }
}
