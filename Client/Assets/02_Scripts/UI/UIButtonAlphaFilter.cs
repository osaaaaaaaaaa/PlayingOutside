using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonAlphaFilter : MonoBehaviour
{
    private void Awake()
    {
        var image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = 1;
    }
}
