using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using static Shared.Interfaces.Model.Entity.EnumManager;

public class SelectMapUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion

    #region SE関係
    [SerializeField] SEController seController;
    [SerializeField] AudioClip selectSE;
    [SerializeField] AudioClip closeSE;
    [SerializeField] AudioClip buttonSE;
    #endregion


    [SerializeField] RoomDirector roomDirector;
    [SerializeField] GameObject panelBG;
    [SerializeField] GameObject panelSelectBtnsBarriar;
    [SerializeField] Button btnShowPanel;
    [SerializeField] List<Button> btnSelectRelayMaps;
    [SerializeField] List<Button> btnSelectFinalMaps;
    [SerializeField] Text hostName;
    bool isMasterClient;

    public EnumManager.SELECT_RELAY_AREA_ID relayAreaId { get; private set; }
    public EnumManager.SELECT_FINALGAME_AREA_ID finalGameStageId { get; private set; }

    private void Awake()
    {
        int i = 0;
        foreach (var btn in btnSelectRelayMaps)
        {
            ColorBlock colorblock = btn.colors;
            if (i == 0)
            {
                colorblock.normalColor = Color.white;
            }
            else
            {
                colorblock.normalColor = colorblock.disabledColor;
            }
            btn.colors = colorblock;
            i++;
        }

        i = 0;
        foreach (var btn in btnSelectFinalMaps)
        {
            ColorBlock colorblock = btn.colors;
            if ((int)relayAreaId == i)
            {
                colorblock.normalColor = Color.white;
            }
            else
            {
                colorblock.normalColor = colorblock.disabledColor;
            }
            btn.colors = colorblock;
            i++;
        }
    }

    public void UpdateHostNameText()
    {
        if(RoomModel.Instance != null)
        {
            if(RoomModel.Instance.userState == RoomModel.USER_STATE.joined) 
                hostName.text = "現在のホスト：" + RoomModel.Instance.MasterName;

        }
        else
        {
            hostName.text = "現在のホスト：存在しません";
        }
    }

    /// <summary>
    /// UIの表示・非表示
    /// </summary>
    /// <param name="isVisibility"></param>
    public void ToggleUIVisibility(bool isVisibility)
    {
        if (isVisibility) seController.PlayAudio(selectSE);
        else seController.PlayAudio(closeSE);

        UpdateHostNameText();
        panelBG.SetActive(isVisibility);
        Vector3 endScale = isVisibility ? Vector3.one : Vector3.zero;
        Ease setEase = isVisibility ? Ease.OutBack : Ease.InBack;

        foreach (var ui in uiList)
        {
            ui.transform.DOScale(endScale, 0.2f).SetEase(setEase);
        }
    }

    public void SetupInteractableButtons(bool isInteractable)
    {
        isMasterClient = isInteractable;
        panelSelectBtnsBarriar.SetActive(!isInteractable);
    }

    /// <summary>
    /// ボタン操作をできなくする
    /// </summary>
    public void DisableButton()
    {
        isMasterClient = false;
        btnShowPanel.interactable = false;
        ToggleUIVisibility(false);
    }

    /// <summary>
    /// 選択通知が来た時の処理
    /// </summary>
    /// <param name="id"></param>
    public void OnSelectRelayArea(EnumManager.SELECT_RELAY_AREA_ID id)
    {
        relayAreaId = id;
        int i = 0;
        foreach(var btn in btnSelectRelayMaps)
        {
            ColorBlock colorblock = btn.colors;
            if ((int)relayAreaId == i)
            {
                colorblock.normalColor = Color.white;
            }
            else
            {
                colorblock.normalColor = colorblock.disabledColor;
            }
            btn.colors = colorblock;
            i++;
        }
    }

    /// <summary>
    /// 選択通知が来た時の処理
    /// </summary>
    /// <param name="id"></param>
    public void OnSelectFinalMap(EnumManager.SELECT_FINALGAME_AREA_ID id)
    {
        finalGameStageId = id;
        int i = 0;
        foreach (var btn in btnSelectFinalMaps)
        {
            ColorBlock colorblock = btn.colors;
            if ((int)finalGameStageId == i)
            {
                colorblock.normalColor = Color.white;
            }
            else
            {
                colorblock.normalColor = colorblock.disabledColor;
            }
            btn.colors = colorblock;
            i++;
        }
    }

    /// <summary>
    /// カントリーリレーマップの選択処理
    /// </summary>
    /// <param name="index"></param>
    public void SelectRelayMap(int index)
    {
        seController.PlayAudio(buttonSE);

        SELECT_RELAY_AREA_ID areaId = SELECT_RELAY_AREA_ID.Course_Random;
        switch (index)
        {
            case (int)SELECT_RELAY_AREA_ID.Course_Hay:
                areaId = SELECT_RELAY_AREA_ID.Course_Hay;
                break;
            case (int)SELECT_RELAY_AREA_ID.Course_Cow:
                areaId = SELECT_RELAY_AREA_ID.Course_Cow;
                break;
            case (int)SELECT_RELAY_AREA_ID.Course_Plant:
                areaId = SELECT_RELAY_AREA_ID.Course_Plant;
                break;
            case (int)SELECT_RELAY_AREA_ID.Course_Goose:
                areaId = SELECT_RELAY_AREA_ID.Course_Goose;
                break;
        }

        roomDirector.SelectGameMapAsynk(areaId, finalGameStageId);
    }

    /// <summary>
    /// 最終競技マップの選択処理
    /// </summary>
    /// <param name="index"></param>
    public void SelectFinalGameMap(int index)
    {
        seController.PlayAudio(buttonSE);

        SELECT_FINALGAME_AREA_ID areaId = SELECT_FINALGAME_AREA_ID.Stage_Random;
        switch (index)
        {
            case (int)SELECT_FINALGAME_AREA_ID.Stage_Hay:
                areaId = SELECT_FINALGAME_AREA_ID.Stage_Hay;
                break;
            case (int)SELECT_FINALGAME_AREA_ID.Stage_Goose:
                areaId = SELECT_FINALGAME_AREA_ID.Stage_Goose;
                break;
            case (int)SELECT_FINALGAME_AREA_ID.Stage_Chicken:
                areaId = SELECT_FINALGAME_AREA_ID.Stage_Chicken;
                break;
        }

        roomDirector.SelectGameMapAsynk(relayAreaId, areaId);
    }
}
