using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmoteWindowUI : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion
    [SerializeField] GameObject panel;
    [SerializeField] List<Button> buttonEmotes;
    public List<Button> ButtonEmotes { get { return buttonEmotes; } }

    private void Awake()
    {
        ToggleWindowVisibility(false);
    }

    public void ToggleWindowVisibility(bool isVisibility)
    {
        panel.SetActive(isVisibility);
        Vector3 endScale = isVisibility ? Vector3.one : Vector3.zero;
        Ease setEase = isVisibility ? Ease.OutBack : Ease.InBack;

        foreach (var ui in uiList)
        {
            ui.transform.DOScale(endScale, 0.2f).SetEase(setEase);
        }

        foreach(var btn in buttonEmotes)
        {
            btn.interactable = isVisibility;
        }
    }
}
