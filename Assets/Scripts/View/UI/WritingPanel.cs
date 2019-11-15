using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WritingPanel : MonoBehaviour
{
    RectTransform penWrapper;
    StatusButton lockButton;
    RecognizePanel recognizePanel;

    public void Init(GeoUI geoUI)
    {
        lockButton = GameObject.Find("LockButton").GetComponent<StatusButton>();
        lockButton.SetStatus(0);
        Clear();

        RectTransform writingPanel = (RectTransform)transform;
        penWrapper = transform.Find("Wrapper").GetComponent<RectTransform>();
        penWrapper.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, writingPanel.rect.width);
        penWrapper.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, writingPanel.rect.height);

        transform.GetComponentInChildren<PenBehaviour>().Init(geoUI);
        recognizePanel = geoUI.recognizePanel;
    }

    public void Clear()
    {
        gameObject.SetActive(false);
        if (recognizePanel != null)
            recognizePanel.Clear();
        lockButton.SetStatus(1);
    }

    public void OpenWritingPanel()
    {
        gameObject.SetActive(true);
        recognizePanel.showRecognizePanel();
    }

    public void OnDestroy()
    {
    }

}
