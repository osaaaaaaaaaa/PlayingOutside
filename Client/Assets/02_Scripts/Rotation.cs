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
        this.transform.DORotate(angles, animSec).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
    }
}
