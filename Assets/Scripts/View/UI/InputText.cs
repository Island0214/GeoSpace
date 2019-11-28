using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputText : InputBase
{
    string text = "";
    Text textField;

    public void Init(FormText formText)
    {
        Init();
        Refresh(formText);
    }

    public void Init()
    {
        textField = GetComponent<Text>();

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(UIConstants.TextFontSize * text.Length, UIConstants.TextFontSize);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UIConstants.TextFontSize);

        textField.fontSize = (int)UIConstants.TextFontSize;
    }

    public void Refresh(FormText formText)
    {
        text = formText.label;
        textField.text = text;
    }

    public override float PreferredWidth()
    {
        return textField.cachedTextGenerator.GetPreferredWidth(text, textField.GetGenerationSettings(GetComponent<RectTransform>().rect.size));
    }

    public void SetTimer()
    {
        // Destroy(gameObject, 1.5f);
    }
}