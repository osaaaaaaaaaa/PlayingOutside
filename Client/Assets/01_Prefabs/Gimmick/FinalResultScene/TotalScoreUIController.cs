using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;

public class TotalScoreUIController : MonoBehaviour
{
    [SerializeField] AudioClip dramRollSE;
    [SerializeField] AudioClip dramFinishSE;
    SEController seController;

    [SerializeField] List<GameObject> textParents;
    [SerializeField] List<Text> textScores;
    ResultData[] resultData;

    private void Awake()
    {
        seController = GetComponent<SEController>();
        for (int i = 0; i < textParents.Count; i++)
        {
            textParents[i].SetActive(false);
        }
    }

    public void Init(ResultData[] resultData)
    {
        this.resultData = new ResultData[resultData.Length];
        this.resultData = resultData;

        var currentUsers = RoomModel.Instance.JoinedUsers;
        foreach (var result in resultData) 
        {
            textParents[result.joinOrder - 1].SetActive(true);
        }
    }

    /// <summary>
    /// スコアアニメーション再生
    /// </summary>
    public void PlayAnim()
    {
        seController.PlayAudio(dramRollSE, true);

        for (int i = 0; i < textParents.Count; i++)
        {
            if (textParents[i].activeSelf)
            {
                // 4桁でランダムな数字をアニメーション
                textScores[i].DOText("0000", 9999, true, ScrambleMode.Numerals).SetEase(Ease.Linear); 
            }
        }
    }

    /// <summary>
    /// スコアアニメーションを停止する
    /// </summary>
    public void StopAnim()
    {
        seController.PlayAudio(dramFinishSE, true);

        foreach (var result in resultData)
        {
            DOTween.Kill(textScores[result.joinOrder - 1]);
            textScores[result.joinOrder - 1].text = result.score.ToString();
        }
    }
}
