using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserScoreController : MonoBehaviour
{
    [SerializeField] List<GameObject> objUserScoreList;
    [SerializeField] List<Sprite> spriteIconList;
    [SerializeField] List<Image> imageIconList;
    [SerializeField] List<Text> textNameList;
    [SerializeField] List<Text> textScoreList;
    [SerializeField] List<Text> textShadowScoreList;

    public void InitUserScoreList(int  joinOrder, int characterId, string userName, int score)
    {
        joinOrder--;    // 調整
        objUserScoreList[joinOrder].SetActive(true);
        imageIconList[joinOrder].sprite = spriteIconList[characterId];
        textNameList[joinOrder].text = userName;
        textScoreList[joinOrder].text = score + "pt";
        textShadowScoreList[joinOrder].text = score + "pt";
    }

    public void UpdateScore(int joinOrder, int score)
    {
        joinOrder--;    // 調整
        textScoreList[joinOrder].text = score + "pt";
        textShadowScoreList[joinOrder].text = score + "pt";
    }
}
