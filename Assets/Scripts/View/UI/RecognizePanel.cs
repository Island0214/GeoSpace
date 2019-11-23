using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecognizePanel : MonoBehaviour
{
    InputField input;

    public void Init()
    {
        input = transform.Find("InputField").GetComponent<InputField>();
        Clear();
    }

    public void AddWord(string str)
    {
        input.text += str;
    }

    public void Clear()
    {
        gameObject.SetActive(false);
        input.text = "";
    }

    public void showRecognizePanel()
    {
        gameObject.SetActive(true);
    }

    public string GetWords()
    {
        return input.text;
    }

    public void OnDestroy()
    {
    }
}
