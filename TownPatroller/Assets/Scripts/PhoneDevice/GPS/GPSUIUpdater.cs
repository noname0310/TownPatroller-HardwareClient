using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GPSUIUpdater : MonoBehaviour
{
    public GameObject latitude;
    public GameObject longitude;

    private Text Latitude;
    private Text Longitude;

    private void Start()
    {
        latitude = GameObject.Find("latText");
        longitude = GameObject.Find("longText");

        Latitude = latitude.GetComponent<Text>();
        Longitude = longitude.GetComponent<Text>();
    }

    private void Update()
    {
        Latitude.text = GPSCore.Instance.latitude.ToString();
        Longitude.text = GPSCore.Instance.longitude.ToString();
    }
}
