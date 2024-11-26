using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TopSceneUIManager : MonoBehaviour
{
    [SerializeField] List<GameObject> selectButtonList;
    [SerializeField] GameObject textUserName;

    [SerializeField] List<Sprite> spriteIcons;
    public List<Sprite> SpriteIcons { get { return spriteIcons; } }

    void ToggleUIVisibility(bool isVisibility)
    {
        Vector3 endScale = isVisibility ? Vector3.one : Vector3.zero;
        Ease setEase = isVisibility ? Ease.OutBack : Ease.InBack;

        foreach (var button in selectButtonList)
        {
            button.transform.DOScale(endScale, 0.2f).SetEase(setEase);
        }
        textUserName.transform.DOScale(endScale, 0.2f).SetEase(setEase);
    }

    public virtual void OnSelectButton()
    {
        ToggleUIVisibility(false);
    }

    public virtual void OnBackButton()
    {
        ToggleUIVisibility(true);
    }
}
