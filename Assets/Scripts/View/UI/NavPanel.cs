using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavPanel : MonoBehaviour
{
    public Action<int> OnShadeButtonClick;
    public Action<int> OnLockButtonClick;
    public Action<int> OnDisplayButtonClick;
    public Action<int> OnCoordinateButtonClick;
    public Action<int> OnGridButtonClick;
    public Action<int> OnSnapButtonClick;
    public Action<int> OnVoiceButtonClick;

    private StatusButton shadeButton;
    private StatusButton lockButton;
    private StatusButton displayButton;
    private StatusButton coordinatButton;
    private StatusButton gridButton;
    private StatusButton snapButton;
    private StatusButton voiceButton;

    public void Init()
    {
        shadeButton = transform.Find("ShadeButton").GetComponent<StatusButton>();
        shadeButton.Init();
        shadeButton.OnStatusChange = (last, current) =>
        {
            if (OnShadeButtonClick != null)
                OnShadeButtonClick(current);
        };
        shadeButton.SetStatus(1);


        lockButton = transform.Find("LockButton").GetComponent<StatusButton>();
        lockButton.Init();
        lockButton.OnStatusChange = (last, current) =>
        {
            if (OnLockButtonClick != null)
                OnLockButtonClick(current);
        };
        lockButton.SetStatus(0);

        displayButton = transform.Find("DisplayButton").GetComponent<StatusButton>();
        displayButton.Init();
        displayButton.OnStatusChange = (last, current) =>
        {
            if (OnDisplayButtonClick != null)
                OnDisplayButtonClick(current);
        };
        displayButton.SetStatus(0);

        coordinatButton = transform.Find("CoordinateButton").GetComponent<StatusButton>();
        coordinatButton.Init();
        coordinatButton.OnStatusChange = (last, current) =>
        {
            if (OnCoordinateButtonClick != null)
                OnCoordinateButtonClick(current);
        };
        coordinatButton.SetStatus(0);
        
        gridButton = transform.Find("GridButton").GetComponent<StatusButton>();
        gridButton.Init();
        gridButton.OnStatusChange = (last, current) =>
        {
            if (OnGridButtonClick != null)
                OnGridButtonClick(current);
        };
        gridButton.SetStatus(0);

        snapButton = transform.Find("SnapButton").GetComponent<StatusButton>();
        snapButton.Init();
        snapButton.OnStatusChange = (last, current) =>
        {
            if (OnSnapButtonClick != null)
                OnSnapButtonClick(current);
        };
        snapButton.SetStatus(0);

        voiceButton = transform.Find("VoiceButton").GetComponent<StatusButton>();
        voiceButton.Init();
        voiceButton.OnStatusChange = (last, current) =>
        {
            if (OnVoiceButtonClick != null)
                OnVoiceButtonClick(current);
        };
        voiceButton.SetStatus(0);

    }

    public void SetDisplayButtonStatus(int status)
    {
        displayButton.SetStatus(status);
    }
    public void SetCoordinateButtonStatus(int status)
    {
        coordinatButton.SetStatus(status);
    }
    public void SetGridButtonStatus(int status)
    {
        gridButton.SetStatus(status);
    }

}
