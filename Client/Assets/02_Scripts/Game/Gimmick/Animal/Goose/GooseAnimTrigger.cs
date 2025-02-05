//*********************************************************
// ガチョウのアニメーションから呼ぶスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooseAnimTrigger : MonoBehaviour
{
    Goose goose;

    // Start is called before the first frame update
    void Start()
    {
        goose = transform.parent.GetComponent<Goose>();
    }

    public void CallInitParam()
    {
        goose.InitParam();
    }

    public void OnAttack()
    {
        goose.IsControllEnable = true;
    }
}
