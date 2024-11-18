using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TopSceneUIManager : MonoBehaviour
{
    [SerializeField] CharacterManager characterManager;
    [SerializeField] List<GameObject> selectButtonList;

    void ToggleSelectButtonsVisibility(bool isVisibility)
    {
        foreach (var button in selectButtonList)
        {
            Vector3 endScale = isVisibility ? Vector3.one : Vector3.zero;
            Ease setEase = isVisibility ? Ease.OutBack : Ease.InBack;
            button.transform.DOScale(endScale, 0.2f).SetEase(setEase);
        }
    }

    public void OnEditPlayerButton()
    {
        ToggleSelectButtonsVisibility(false);
        characterManager.DOMoveCharacter();
    }

    public void OnEndEditPlayerButton()
    {
        ToggleSelectButtonsVisibility(true);
        characterManager.DOResetCharacter();
    }
}
