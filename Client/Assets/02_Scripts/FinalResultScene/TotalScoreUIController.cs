using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TotalScoreUIController : MonoBehaviour
{
    [SerializeField] List<GameObject> textParents;
    [SerializeField] List<Text> textScores;
    int[] scores;

    private void Start()
    {
        PlayAnim();
    }

    public void Init(int activeSelfNum, int[] _scores)
    {
        scores = new int[_scores.Length];
        scores = _scores;
        for (int i = 0; i < textParents.Count; i++) 
        {
            // 指定された数のUIを表示する
            textParents[i].SetActive(i < activeSelfNum);
        }
    }

    public void PlayAnim()
    {
        foreach (var text in textScores)
        {
            // 4桁でランダムな数字をアニメーション
            text.DOText("0000", 9999, true,ScrambleMode.Numerals).SetEase(Ease.Linear);
        }
    }

    public void StopAnim()
    {
        for (int i = 0; i < scores.Length; i++)
        {
            DOTween.Kill(textScores[i]);
            textScores[i].text = scores[i].ToString();
        }
    }
}
