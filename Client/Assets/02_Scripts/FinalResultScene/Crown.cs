using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Crown : MonoBehaviour
{
    [SerializeField] Vector3 scaleEndVal;

    private void OnEnable()
    {
        const float animSec = 1f;
        transform.localPosition = new Vector3(0f, 1.75f, 0f);
        transform.localScale = Vector3.zero;
        Vector3 angles = transform.eulerAngles;

        // 回転しながらサイズが大きくなるアニメーション
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DORotate(new Vector3(angles.x, 360, 0), animSec, RotateMode.FastBeyond360).SetEase(Ease.OutBack))
            .Join(transform.DOScale(scaleEndVal, animSec).SetEase(Ease.OutElastic));
    }
}
