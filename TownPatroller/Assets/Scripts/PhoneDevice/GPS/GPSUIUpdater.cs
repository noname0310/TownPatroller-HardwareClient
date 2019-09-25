using System.Collections;
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

        StartCoroutine(UpdateGPSUI());
    }

    private IEnumerator UpdateGPSUI()
    {
        yield return new WaitForSeconds(0.2f);

        while (true)
        {
            Latitude.text = GPSCore.Instance.latitude.ToString();
            Longitude.text = GPSCore.Instance.longitude.ToString();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
