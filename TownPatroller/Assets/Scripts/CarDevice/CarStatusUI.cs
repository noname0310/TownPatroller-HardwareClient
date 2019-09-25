using System;
using System.Reflection;
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

    private static string[] CarStatusNum0TO1000 = new string[1002];
    private static string[] CarStatusNumm255TOm0 = new string[257];

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

        InitStatusNum();
        InitLED();
        InitTexts();
    }

    private void InitStatusNum()
    {
        for (int i = 0; i < 1001; i++)
        {
            CarStatusNum0TO1000[i] = i.ToString();
        }

        CarStatusNum0TO1000[1001] = "ERR";

        for(int i = 1; i < 257; i++)
        {
            CarStatusNumm255TOm0[i] = (i - 256).ToString();
        }

        CarStatusNumm255TOm0[0] = "MERR";
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
        if (0 <= value)
        {
            if (1000 < value)
                value = 1001;
            (textobj.GetValue(this) as Text).text = CarStatusNum0TO1000[value];
        }
        else
        {
            if (value < -255)
                value = -256;
            (textobj.GetValue(this) as Text).text = CarStatusNumm255TOm0[value + 256];
        }
    }

    public void SetText(string textobj, int value)
    {
        FieldInfo fieldInfo = this.GetType().GetField(textobj);

        if (0 <= value)
        {
            if (1000 < value)
                value = 1001;
            (fieldInfo.GetValue(this) as Text).text = CarStatusNum0TO1000[value];
        }
        else
        {
            if (value < -255)
                value = -256;
            (fieldInfo.GetValue(this) as Text).text = CarStatusNumm255TOm0[value + 256];
        }
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
