using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.TextCore.Text;

public class TopSceneCharacterManager : MonoBehaviour
{
    [SerializeField] GameObject characterModelsparent;
    [SerializeField] List<GameObject> characterModels;
    Vector3 startPosCharacter;
    const float constAngleY = 150;

    private void Start()
    {
        startPosCharacter = characterModelsparent.transform.position;
    }

    public void ToggleCharacter(int characterid)
    {
        for (int i = 0; i < characterModels.Count; i++)
        {
            if (i == characterid - 1)
                characterModels[i].SetActive(true);
            else characterModels[i].SetActive(false);
        }
    }

    public void DOMoveCharacter()
    {
        var sequense = DOTween.Sequence();
        sequense.Append(characterModelsparent.transform.DOMove(new Vector3(0f, 0f, startPosCharacter.z), 0.3f).SetEase(Ease.Linear))
            .Join(characterModelsparent.transform.DORotate(new Vector3(0f,180f,0f), 0.3f).SetEase(Ease.Linear));
        sequense.Play();
    }

    public void DOResetCharacter()
    {
        var sequense = DOTween.Sequence();
        sequense.Append(characterModelsparent.transform.DOMove(startPosCharacter, 0.3f).SetEase(Ease.Linear))
            .Join(characterModelsparent.transform.DORotate(new Vector3(0f, constAngleY, 0f), 0.3f).SetEase(Ease.Linear));
        sequense.Play();
    }
}
