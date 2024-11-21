using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TopSceneUIManager : MonoBehaviour
{
    [SerializeField] CharacterManager characterManager;
    [SerializeField] List<GameObject> selectButtonList;
    [SerializeField] GameObject textUserName;

    void ToggleTopUIVisibility(bool isVisibility)
    {
        Vector3 endScale = isVisibility ? Vector3.one : Vector3.zero;
        Ease setEase = isVisibility ? Ease.OutBack : Ease.InBack;

        foreach (var button in selectButtonList)
        {
            button.transform.DOScale(endScale, 0.2f).SetEase(setEase);
        }
        textUserName.transform.DOScale(endScale, 0.2f).SetEase(setEase);
    }

    public void OnEditPlayerButton()
    {
        ToggleTopUIVisibility(false);
        characterManager.DOMoveCharacter();
    }

    public void OnEndEditPlayerButton()
    {
        ToggleTopUIVisibility(true);
        characterManager.DOResetCharacter();
    }
}
