//*********************************************************
// ボタンの画像の透明部分を押せなくするスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonAlphaFilter : MonoBehaviour
{
    private void Awake()
    {
        var image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = 1;
    }
}
