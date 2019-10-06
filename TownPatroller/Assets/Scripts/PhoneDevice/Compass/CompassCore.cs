using UnityEngine;
using UnityEngine.UI;

public class CompassCore : MonoBehaviour
{
    public static CompassCore Instance { get; set; }

    private bool gyroEnabled;
    private Gyroscope gyro;

    public GameObject Compass;
    public GameObject CompassUI;
    private Text RotText;

    private float CompassAngle;
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
    }

    private void OnDestroy()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = false;
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
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;

            IGConsole.Instance.Main.println("Gyroscope enabled");
            return true;
        }
        else
        {
            IGConsole.Instance.Main.println("This divice not supports gyroscope");
        }

        return false;
    }

    private void Update()
    {
        if (gyroEnabled)
        {
            CompassAngle = -gyro.attitude.eulerAngles.z + 90;
            Compass.transform.localRotation = Quaternion.Euler(0f, 0f, CompassAngle);

            AngleFromN = -CompassAngle;
            if (AngleFromN < 0)
                AngleFromN += 360;
            if (360 < (int)AngleFromN || (int)AngleFromN < 0)
                AngleFromN = 361;
            RotText.text = CarStatusNum0TO360[((int)AngleFromN)];
        }
    }
}