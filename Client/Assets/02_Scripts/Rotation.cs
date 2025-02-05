//*********************************************************
// 指定した向きに回転を続けるスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Rotation : MonoBehaviour
{
    [SerializeField] Vector3 angles;
    [SerializeField] float animSec;

    private void Start()
    {
        this.transform.DOLocalRotate(angles, animSec).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
    }
}
