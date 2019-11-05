using UnityEngine;
using UnityEngine.UI;

public class CamManager : MonoBehaviour
{
    private bool camAvalible;
    public WebCamTexture CamDevice;
    public Texture defaultBackground;

    public RawImage background;
    public AspectRatioFitter AspectRatioFitter;
    public Text text;

    private uint camIndex = 0;
    private WebCamDevice[] devices;
    private void Start()
    {
        defaultBackground = background.texture;
        devices = WebCamTexture.devices;

        InitCam();
    }

    private void OnDestroy()
    {
        if (CamDevice != null)
        {
            CamDevice.Stop();
        }
    }

    private void Update()
    {
        if (!camAvalible)
            return;

        float ratio = CamDevice.width / (float)CamDevice.height;
        if (AspectRatioFitter.aspectRatio != ratio)
            AspectRatioFitter.aspectRatio = ratio;

        float scaleY = CamDevice.videoVerticallyMirrored ? -1f : 1f;
        if (background.rectTransform.localScale.x != 1f || background.rectTransform.localScale.y != scaleY || background.rectTransform.localScale.z != 1f)
            background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -CamDevice.videoRotationAngle;
        if (background.rectTransform.localEulerAngles.x != 0 || background.rectTransform.localEulerAngles.y != 0 || background.rectTransform.localEulerAngles.z != orient)
            background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }

    private void InitCam()
    {
        if (CamDevice != null)
        {
            if (CamDevice.isPlaying)
            {
                CamDevice.Stop();
            }
        }

        if (devices.Length == 0)
        {
            Debug.Log("No camara detected");
            text.text = "No camara detected";
            camAvalible = false;
            return;
        }

        CamDevice = new WebCamTexture(devices[camIndex].name, 1920, 1080);
        CamDevice.requestedFPS = 15;
        //CamDevice.filterMode = FilterMode.Trilinear;
        CamDevice.Play();
        background.texture = CamDevice;

        camAvalible = true;
    }

    public void SwitchCam()
    {
        camIndex++;

        if (devices.Length - 1 < camIndex)
            camIndex = 0;

        InitCam();
    }
}
