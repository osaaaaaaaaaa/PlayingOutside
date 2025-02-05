//*********************************************************
// 競技終了時に表示するFINISHというUIのスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FinishUI : MonoBehaviour
{
    public float animSec { get; private set; } = 0.5f;
    private void OnDisable()
    {
        transform.localScale = Vector3.one * 2f;
        GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
    }

    private void OnEnable()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(new Vector3(1f, 1f, 1f), animSec).SetEase(Ease.OutBack))
            .Join(GetComponent<Image>().DOFade(1f, animSec).SetEase(Ease.Linear));
        sequence.Play();

        GetComponent<SEController>().PlayAudio();
    }
}
