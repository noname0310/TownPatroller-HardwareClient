using System.Collections;
using UnityEngine;
using TownPatroller.CarDevice;
using TownPatroller.GPSTracer;
using TownPatroller.SocketClient;
using TPPacket.Packet;

public class SocketLinkerObj : MonoBehaviour
{
    public GameObject CarStatusObject;
    public ObjectCarDevice objcarDevice;
    public GPSTracerObj tracerObj;

    private BaseCarDivice baseCarDivice;
    private CamManager camManager;
    private Texture2D texture2D;

    public SocketObj socketObj;
    private IClientSender clientSender;

    private bool SendCamTexture;

    private void Start()
    {
        SendCamTexture = false;
        CarStatusObject = GameObject.Find("CarStatusObject");

        objcarDevice = CarStatusObject.GetComponent<ObjectCarDevice>();
        tracerObj = CarStatusObject.GetComponent<GPSTracerObj>();

        baseCarDivice = objcarDevice.Basecardivice;
        camManager = GameObject.Find("CamManager").GetComponent<CamManager>();

        socketObj = GameObject.Find("NetworkManager(Clone)").GetComponent<SocketObj>();
        clientSender = socketObj.socketClient;
        socketObj.OnDataInvoke += SocketObj_OnDataInvoke;
        StartCoroutine(SendInitData());
    }

    private void OnDestroy()
    {
        socketObj.OnDataInvoke -= SocketObj_OnDataInvoke;
    }

    private void SocketObj_OnDataInvoke(BasePacket basePacket)
    {
        switch (basePacket.packetType)
        {
            //case PacketType.ConnectionStat:
            //break;

            case PacketType.CamReceived:
                if (SendCamTexture == true)
                {
                    SendCamData();
                }
                break;

            case PacketType.CamConfig:
                CamConfigPacket ccp = (CamConfigPacket)basePacket;
                switch (ccp.camaraConfigType)
                {
                    case CamaraConfigType.ChangeCamara:
                        camManager.SwitchCam();
                        break;

                    case CamaraConfigType.SendFrame:
                        if (ccp.enable)
                        {
                            SendCamTexture = true;
                            SendCamData();
                        }
                        else
                            SendCamTexture = false;
                        break;

                    default:
                        break;
                }
                break;

            case PacketType.CarStatusChangeReq:
                if(tracerObj.gPSMover.EnableTraceMode == false)
                {
                    CarStatusChangeReqPacket cscrp = (CarStatusChangeReqPacket)basePacket;

                    if (cscrp.ReqCarDevice.R_motorDIRChanged)
                        baseCarDivice.r_motorDIR = cscrp.ReqCarDevice.R_motorDIR;

                    if (cscrp.ReqCarDevice.L_motorDIRChanged)
                        baseCarDivice.l_motorDIR = cscrp.ReqCarDevice.L_motorDIR;

                    if (cscrp.ReqCarDevice.R_motorpowerChanged)
                        baseCarDivice.r_motorpower = cscrp.ReqCarDevice.R_motorpower;

                    if (cscrp.ReqCarDevice.L_motorpowerChanged)
                        baseCarDivice.l_motorpower = cscrp.ReqCarDevice.L_motorpower;

                    if (cscrp.ReqCarDevice.RF_LEDChanged)
                        baseCarDivice.rf_LED = cscrp.ReqCarDevice.RF_LED;

                    if (cscrp.ReqCarDevice.LF_LEDChanged)
                        baseCarDivice.lf_LED = cscrp.ReqCarDevice.LF_LED;

                    if (cscrp.ReqCarDevice.RB_LEDChanged)
                        baseCarDivice.rb_LED = cscrp.ReqCarDevice.RB_LED;

                    if (cscrp.ReqCarDevice.LB_LEDChanged)
                        baseCarDivice.lb_LED = cscrp.ReqCarDevice.LB_LED;
                }
                break;

            case PacketType.CarStatusReceived:
                StartCoroutine(SendCarDataWithDelay());
                break;

            case PacketType.CarGPSSpotStatusChangeReq:
                CarGPSSpotStatusChangeReqPacket cgscrp = (CarGPSSpotStatusChangeReqPacket)basePacket;
                switch (cgscrp.GPSSpotManagerChangeType)
                {
                    case GPSSpotManagerChangeType.AddSpot:
                        tracerObj.gPSMover.GPSSpotManager.AddPos(cgscrp.GPSPosition);
                        clientSender.SendPacket(new CarGPSSpotStatusPacket(GPSSpotManagerChangeType.AddSpot, cgscrp.GPSPosition));
                        break;
                    case GPSSpotManagerChangeType.RemoveSpot:
                        tracerObj.gPSMover.GPSSpotManager.RemovePos(cgscrp.Index);
                        clientSender.SendPacket(new CarGPSSpotStatusPacket(GPSSpotManagerChangeType.RemoveSpot, cgscrp.Index));
                        break;
                    case GPSSpotManagerChangeType.SetCurrentPos:
                        tracerObj.gPSMover.GPSSpotManager.CurrentMovePosIndex = cgscrp.Index;
                        clientSender.SendPacket(new CarGPSSpotStatusPacket(GPSSpotManagerChangeType.SetCurrentPos, cgscrp.Index));
                        break;
                    case GPSSpotManagerChangeType.OverWrite:
                        tracerObj.gPSMover.ChangeSpotManager(cgscrp.GPSMover);
                        clientSender.SendPacket(new CarGPSSpotStatusPacket(GPSSpotManagerChangeType.OverWrite, cgscrp.GPSMover));
                        break;
                    default:
                        break;
                }
                break;

            case PacketType.UpdateDataReq:
                DataUpdatePacket dup = (DataUpdatePacket)basePacket;

                switch (dup.modeType)
                {
                    case ModeType.AutoDriveMode:
                        tracerObj.gPSMover.EnableTraceMode = true;
                        baseCarDivice.HalfManualMode = false;
                        clientSender.SendPacket(new DataUpdatedPacket(ModeType.AutoDriveMode));
                        break;
                    case ModeType.ManualDriveMode:
                        tracerObj.gPSMover.EnableTraceMode = false;
                        baseCarDivice.HalfManualMode = false;
                        clientSender.SendPacket(new DataUpdatedPacket(ModeType.ManualDriveMode));
                        break;
                    case ModeType.HaifManualDriveMode:
                        tracerObj.gPSMover.EnableTraceMode = false;
                        baseCarDivice.HalfManualMode = true;
                        clientSender.SendPacket(new DataUpdatedPacket(ModeType.HaifManualDriveMode));
                        break;
                    default:
                        break;
                }
                clientSender.SendPacket(new DataUpdatedPacket(dup.modeType));
                break;

            case PacketType.UniversalCommand:
                UniversalCommandPacket ucp = (UniversalCommandPacket)basePacket;
                if(ucp.keyType == KeyType.Command)
                {
                }
                break;

            default:
                break;
        }
    }

    private IEnumerator SendInitData()
    {
        yield return new WaitForSeconds(0.5f);
        clientSender.SendPacket(new DataUpdatedPacket(ModeType.AutoDriveMode));
        clientSender.SendPacket(new CarGPSSpotStatusPacket(GPSSpotManagerChangeType.OverWrite, tracerObj.gPSMover.GPSSpotManager));

        clientSender.SendPacket(new CarStatusPacket(baseCarDivice.GetPacketCarDivice(), GPSCore.Instance.GetGPSsPosition().GetGPSPosition(tracerObj.gPSMover.GetCurrentPositonName()), CompassCore.Instance.AngleFromN));
    }

    private void SendCamData()
    {
        StartCoroutine(SendCamDataWithDelay());
        return;
        //texture2D = TextureToTexture2D(camManager.background.texture);
        //clientSender.SendPacket(new CamPacket(texture2D.EncodeToJPG(10)));
        //Destroy(texture2D);

    }
    private IEnumerator SendCamDataWithDelay()
    {
        yield return new WaitForSeconds(0.01f);
        texture2D = TextureToTexture2D(camManager.background.texture);
        clientSender.SendPacket(new CamPacket(texture2D.EncodeToJPG(50)));
        Destroy(texture2D);
    }

    private IEnumerator SendCarDataWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        clientSender.SendPacket(new CarStatusPacket(baseCarDivice.GetPacketCarDivice(), GPSCore.Instance.GetGPSsPosition().GetGPSPosition(tracerObj.gPSMover.GetCurrentPositonName()), CompassCore.Instance.AngleFromN));
    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return texture2D;
    }
}
