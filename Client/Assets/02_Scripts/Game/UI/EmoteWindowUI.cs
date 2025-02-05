//*********************************************************
// �L�����N�^�[�̃G���[�g�E�C���h�E��\���E��\������X�N���v�g
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
