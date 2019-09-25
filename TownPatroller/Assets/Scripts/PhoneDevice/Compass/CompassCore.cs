using UnityEngine;
using UnityEngine.UI;

public class CompassCore : MonoBehaviour
{
    private bool gyroEnabled;
    private Gyroscope gyro;

    public GameObject Compass;
    public GameObject CompassUI;
    private Text RotText;

    private float CompassAngle;
    private float AngleFromN;

    private void Start()
    {
        Compass = GameObject.Find("CompassIndi");
        CompassUI = GameObject.Find("CompassText");
        RotText = CompassUI.GetComponent<Text>();
        RotText.text = "0";

        gyroEnabled = EnableGyro();
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
            RotText.text = ((int)AngleFromN).ToString();
        }
    }
}