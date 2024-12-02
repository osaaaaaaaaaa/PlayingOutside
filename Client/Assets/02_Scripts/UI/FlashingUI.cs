using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FlashingUI : MonoBehaviour
{
    void Start()
    {
        GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        GetComponent<Image>().DOFade(0.0f, 1).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
    }
}
