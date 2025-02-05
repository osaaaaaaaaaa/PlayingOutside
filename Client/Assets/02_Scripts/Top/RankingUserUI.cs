//*********************************************************
// ランクインしているユーザーのUIのスクリプト
// Author:Rui Enomoto
//*********************************************************
using Server.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RankingUserUI : MonoBehaviour
{
    [SerializeField] Text rankText;
    [SerializeField] Image iconImg;
    [SerializeField] Text userNameText;
    [SerializeField] Text rateText;

    public void SetupUI(Sprite characterIcon, RatingRanking user, int rank)
    {
        rankText.text = rank.ToString();
        iconImg.sprite = characterIcon;
        userNameText.text = user.UserName;
        rateText.text = "レート：" + user.Rating;
    }
}
