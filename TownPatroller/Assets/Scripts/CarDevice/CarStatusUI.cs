using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class CarStatusUI : MonoBehaviour
{
    public GameObject FRLED;
    public GameObject FLLED;
    public GameObject BRLED;
    public GameObject BLLED;

    public Text RMtext;
    public Text LMtext;
    public Text FDStext;
    public Text FLHStext;
    public Text FRHStext;
    public Text LDStext;
    public Text RDStext;

    void Start()
    {
        FRLED = GameObject.Find("FRLED");
        FLLED = GameObject.Find("FLLED");
        BRLED = GameObject.Find("BRLED");
        BLLED = GameObject.Find("BLLED");

        RMtext = GameObject.Find("RMtext").GetComponent<Text>();
        LMtext = GameObject.Find("LMtext").GetComponent<Text>();
        FDStext = GameObject.Find("FDStext").GetComponent<Text>();
        FLHStext = GameObject.Find("FLHStext").GetComponent<Text>();
        FRHStext = GameObject.Find("FRHStext").GetComponent<Text>();
        LDStext = GameObject.Find("LDStext").GetComponent<Text>();
        RDStext = GameObject.Find("RDStext").GetComponent<Text>();

        InitLED();
        InitTexts();
    }

    private void InitTexts()
    {
        foreach (var item in this.GetType().GetFields())
        {
            if(item.FieldType == typeof(Text))
            {
                SetText(item, 0);
            }
        }
    }

    private void SetText(FieldInfo textobj, int value)
    {
        (textobj.GetValue(this) as Text).text = textobj.Name.Substring(0, textobj.Name.Length - 4) + " : " + value;
    }

    public void SetText(string textobj, int value)
    {
        FieldInfo fieldInfo = this.GetType().GetField(textobj);
        (fieldInfo.GetValue(this) as Text).text = fieldInfo.Name.Substring(0, fieldInfo.Name.Length - 4) + " : " + value;
    }

    private void InitLED()
    {
        foreach (LEDtype item in Enum.GetValues(typeof(LEDtype)))
        {
            ChangeLEDStatus(item, false);
        }
    }

    public void ChangeLEDStatus(LEDtype lEDtype, bool value)
    {
        GameObject LEDObj;

        switch (lEDtype)
        {
            case LEDtype.FRLED:
                LEDObj = FRLED;
                break;
            case LEDtype.FLLED:
                LEDObj = FLLED;
                break;
            case LEDtype.BRLED:
                LEDObj = BRLED;
                break;
            case LEDtype.BLLED:
                LEDObj = BLLED;
                break;
            default:
                return;
        }

        if (value)
        {
            if (LEDObj.transform.childCount < 2)
                return;

            LEDObj.transform.GetChild(0).gameObject.SetActive(true);
            LEDObj.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            if (LEDObj.transform.childCount < 2)
                return;

            LEDObj.transform.GetChild(0).gameObject.SetActive(false);
            LEDObj.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public enum LEDtype
    {
        FRLED,
        FLLED,
        BRLED,
        BLLED
    }
}
