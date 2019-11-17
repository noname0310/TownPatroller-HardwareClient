using System.Collections;
using UnityEngine;
using TPPacket.Class;
using TownPatroller.PhoneDevice.GPS;

public class GPSCore : MonoBehaviour
{
    public static GPSCore Instance { get; set; }

    public float latitude
    {
        get
        {
            return Input.location.lastData.latitude;
        }
    }
    public float longitude
    {
        get
        {
            return Input.location.lastData.longitude;
        }
    }

    public GPSsPosition GetGPSsPosition()
    {
        return new GPSsPosition(Input.location.lastData.latitude, Input.location.lastData.longitude);
    }

    private void Start()
    {
        Instance = this;
        //DontDestroyOnLoad(gameObject);
        StartCoroutine(StartLocationService());
    }

    private void OnDestroy()
    {
        StopCoroutine(StartLocationService());
        Instance = null;
    }

    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            IGConsole.Instance.Main.println("User has not enabled GPS");
            yield break;
        }

        Input.location.Start(0.1f, 0.1f);
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1f);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            IGConsole.Instance.Main.println("GPS: Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            IGConsole.Instance.Main.println("GPS: Unable to determin device location");
            yield break;
        }

        //latitude = Input.location.lastData.latitude;
        //longitude = Input.location.lastData.longitude;

        yield break;
    }
}
