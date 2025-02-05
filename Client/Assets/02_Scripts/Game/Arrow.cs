//*********************************************************
// ���̃I�u�W�F�N�g�ɃA�j���[�V������������X�N���v�g
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Arrow : MonoBehaviour
{
    float dis = 2;
    float duration = 1;

    void Start()
    {
        transform.localPosition = new Vector3(dis, 0, 0);
        transform.DOLocalMoveX(-dis, duration).SetEase(Ease.InOutSine).SetLoops(-1,LoopType.Yoyo);
    }
}
