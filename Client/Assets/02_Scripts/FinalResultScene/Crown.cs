using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Crown : MonoBehaviour
{
    [SerializeField] Vector3 scaleEndVal;

    private void Start()
    {
        const float animSec = 1f;
        transform.localScale = Vector3.zero;
        Vector3 angles = transform.eulerAngles;

        // 回転しながらサイズが大きくなるアニメーション
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DORotate(new Vector3(angles.x, 360, 0), animSec, RotateMode.FastBeyond360).SetEase(Ease.OutBack))
            .Join(transform.DOScale(scaleEndVal, animSec).SetEase(Ease.OutElastic));

    }
}
