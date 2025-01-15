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
    AudioSource audioSource;

    [SerializeField] List<GameObject> textParents;
    [SerializeField] List<Text> textScores;
    ResultData[] resultData;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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

    public void PlayAnim()
    {
        audioSource.loop = true;
        audioSource.PlayOneShot(dramRollSE);

        for (int i = 0; i < textParents.Count; i++)
        {
            if (textParents[i].activeSelf)
            {
                // 4���Ń����_���Ȑ������A�j���[�V����
                textScores[i].DOText("0000", 9999, true, ScrambleMode.Numerals).SetEase(Ease.Linear); 
            }
        }
    }

    public void StopAnim()
    {
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.PlayOneShot(dramFinishSE);

        foreach (var result in resultData)
        {
            DOTween.Kill(textScores[result.joinOrder - 1]);
            textScores[result.joinOrder - 1].text = result.score.ToString();
        }
    }
}
