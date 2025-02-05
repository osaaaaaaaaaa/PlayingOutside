//*********************************************************
// 非表示になったときに自動破棄するスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
