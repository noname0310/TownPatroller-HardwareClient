using UnityEngine;
using UnityEngine.UI;

public class CompassCore : MonoBehaviour
{
    public static CompassCore Instance { get; set; }

    private bool gyroEnabled;
    private Compass compass;

    public GameObject Compass;
    public GameObject CompassUI;
    private Text RotText;

    public float AngleFromN;

    private static string[] CarStatusNum0TO360 = new string[362];

    private void Start()
    {
        Instance = this;
        Compass = GameObject.Find("CompassIndi");
        CompassUI = GameObject.Find("CompassText");
        RotText = CompassUI.GetComponent<Text>();
        InitStatusNum();
        RotText.text = CarStatusNum0TO360[0];

        gyroEnabled = EnableGyro();

        AngleFromN = 0;
    }

    private void OnDestroy()
    {
        if (SystemInfo.supportsGyroscope)
        {
            compass = Input.compass;
            compass.enabled = false;
            Input.location.Stop();
        }
    }

    private void InitStatusNum()
    {
        for (int i = 0; i < 361; i++)
        {
            CarStatusNum0TO360[i] = i.ToString();
        }

        CarStatusNum0TO360[361] = "ERR";
    }

    private bool EnableGyro()
    {
        compass = Input.compass;
        compass.enabled = true;
        Input.location.Start();

        IGConsole.Instance.Main.println("Compass enabled");
        return true;
    }

    private void Update()
    {
        if (gyroEnabled)
        {
            AngleFromN = Input.compass.trueHeading;
            Compass.transform.localRotation = Quaternion.Euler(0f, 0f, AngleFromN);
            if (AngleFromN < 0)
                AngleFromN += 360;
            if (360 < (int)AngleFromN || (int)AngleFromN < 0)
                AngleFromN = 361;
            RotText.text = CarStatusNum0TO360[(int)AngleFromN];
        }
    }
}