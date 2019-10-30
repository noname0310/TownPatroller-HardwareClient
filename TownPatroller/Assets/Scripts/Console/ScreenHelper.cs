using UnityEngine;

public class ScreenHelper : MonoBehaviour
{
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void OnDestroy()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }
}
