using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneSettingUI : SettingUIController
{
    [SerializeField] GameObject panelObj;
    RelayGameDirector relayGameDirector;
    FinalGameDirector finalGameDirector;
    RoomDirector roomDirector;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        relayGameDirector = FindObjectOfType<RelayGameDirector>();
        finalGameDirector = FindObjectOfType<FinalGameDirector>();
        roomDirector = FindObjectOfType<RoomDirector>();
        panelObj.SetActive(false);
    }

    public override void OnSelectButton()
    {
        panelObj.SetActive(true);
        base.OnSelectButton();
    }

    public override void OnBackButton()
    {
        panelObj.SetActive(false);
        base.OnBackButton();
    }

    public void OnLeaveButton()
    {
        if (relayGameDirector) relayGameDirector.LeaveRoom();
        if (finalGameDirector) finalGameDirector.LeaveRoom();
        if (roomDirector) roomDirector.LeaveRoom();
    }
}
