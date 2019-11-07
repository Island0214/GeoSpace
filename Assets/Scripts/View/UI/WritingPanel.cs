using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WritingPanel : MonoBehaviour
{
    RectTransform penWrapper;

    public void Init()
    {
        Clear();

        RectTransform writingPanel = (RectTransform)transform;
        penWrapper = transform.Find("Wrapper").GetComponent<RectTransform>();
        penWrapper.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, writingPanel.rect.width);
        penWrapper.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, writingPanel.rect.height);
    }

    public void Clear()
    {
        gameObject.SetActive(false);
    }

    public void OpenWritingPanel()
    {
        gameObject.SetActive(true);
    }

    public void OnDestroy()
    {
    }

}
