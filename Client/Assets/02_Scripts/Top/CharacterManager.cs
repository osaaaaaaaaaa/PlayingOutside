using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] GameObject character;
    Vector3 startPosCharacter;
    const float constAngleY = 150;

    private void Start()
    {
        startPosCharacter = character.transform.position;
    }

    public void DOMoveCharacter()
    {
        var sequense = DOTween.Sequence();
        sequense.Append(character.transform.DOMove(new Vector3(0f, 0f, startPosCharacter.z), 0.3f).SetEase(Ease.Linear))
            .Join(character.transform.DORotate(new Vector3(0f,180f,0f), 0.3f).SetEase(Ease.Linear));
        sequense.Play();
    }

    public void DOResetCharacter()
    {
        var sequense = DOTween.Sequence();
        sequense.Append(character.transform.DOMove(startPosCharacter, 0.3f).SetEase(Ease.Linear))
            .Join(character.transform.DORotate(new Vector3(0f, constAngleY, 0f), 0.3f).SetEase(Ease.Linear));
        sequense.Play();
    }
}
