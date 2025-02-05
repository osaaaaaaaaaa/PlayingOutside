//*********************************************************
// キャラクターのエモートウインドウを表示・非表示するスクリプト
// Author:Rui Enomoto
//*********************************************************
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmoteWindowUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button btnBack;
    [SerializeField] List<Button> buttonEmotes;
    public List<Button> ButtonEmotes { get { return buttonEmotes; } }

    private void Awake()
    {
        ToggleWindowVisibility(false);
    }

    public void ToggleWindowVisibility(bool isVisibility)
    {
        panel.SetActive(isVisibility);
        btnBack.gameObject.SetActive(isVisibility);
        foreach (var btn in buttonEmotes)
        {
            btn.gameObject.SetActive(isVisibility);
        }
    }
}
