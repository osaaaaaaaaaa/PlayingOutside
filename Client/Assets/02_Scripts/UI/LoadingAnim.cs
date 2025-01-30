using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingAnim : MonoBehaviour
{
    private const float DURATION = 1f;  // ローディングアニメーションの間隔

    void Start()
    {
        Image[] circles = GetComponentsInChildren<Image>(); // 子オブジェクトを取得する

        for (var i = 0; i < circles.Length; i++)
        {// 取得した画像の枚数分ループ
            var angle = -2 * Mathf.PI * i / circles.Length;
            circles[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 50f;
            circles[i].DOFade(0f, DURATION).SetLoops(-1, LoopType.Yoyo).SetDelay(DURATION * i / circles.Length);
        }
    }

}
