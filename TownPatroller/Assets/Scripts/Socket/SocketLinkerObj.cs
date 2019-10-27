using System.Collections;
using System.Collections.Generic;
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
    private bool SendCarStatus;

    private void Start()
    {
        SendCamTexture = false;
        SendCarStatus = true;
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

                    if (cscrp.Cardevice.R_motorDIR != baseCarDivice.r_motorDIR)
                        baseCarDivice.r_motorDIR = cscrp.Cardevice.R_motorDIR;

                    if (cscrp.Cardevice.L_motorDIR != baseCarDivice.l_motorDIR)
                        baseCarDivice.l_motorDIR = cscrp.Cardevice.L_motorDIR;

                    if (cscrp.Cardevice.R_motorpower != baseCarDivice.r_motorpower)
                        baseCarDivice.r_motorpower = cscrp.Cardevice.R_motorpower;

                    if (cscrp.Cardevice.L_motorpower != baseCarDivice.l_motorpower)
                        baseCarDivice.l_motorpower = cscrp.Cardevice.L_motorpower;

                    if (cscrp.Cardevice.RF_LED != baseCarDivice.rf_LED)
                        baseCarDivice.rf_LED = cscrp.Cardevice.RF_LED;

                    if (cscrp.Cardevice.LF_LED != baseCarDivice.lf_LED)
                        baseCarDivice.lf_LED = cscrp.Cardevice.LF_LED;

                    if (cscrp.Cardevice.RB_LED != baseCarDivice.rb_LED)
                        baseCarDivice.rb_LED = cscrp.Cardevice.RB_LED;

                    if (cscrp.Cardevice.LB_LED != baseCarDivice.lb_LED)
                        baseCarDivice.lb_LED = cscrp.Cardevice.LB_LED;
                }
                break;

            case PacketType.CarStatusReceived:
                if(SendCarStatus == true)
                {
                    StartCoroutine(SendCarDataWithDelay());
                }
                break;

            case PacketType.CarGPSSpotStatusChangeReq:
                CarGPSSpotStatusChangeReqPacket cgscrp = (CarGPSSpotStatusChangeReqPacket)basePacket;
                tracerObj.gPSMover.ChangeSpot(cgscrp.gPSMover);
                clientSender.SendPacket(new CarGPSStatusUpdatedPacket());
                break;

            case PacketType.UpdateDataReq:
                DataUpdatePacket dup = (DataUpdatePacket)basePacket;

                switch (dup.modeType)
                {
                    case ModeType.AutoDriveMode:
                        tracerObj.gPSMover.EnableTraceMode = true;
                        baseCarDivice.HalfManualMode = false;
                        break;
                    case ModeType.ManualDriveMode:
                        tracerObj.gPSMover.EnableTraceMode = false;
                        baseCarDivice.HalfManualMode = false;
                        break;
                    case ModeType.HaifManualDriveMode:
                        tracerObj.gPSMover.EnableTraceMode = false;
                        baseCarDivice.HalfManualMode = true;
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
        clientSender.SendPacket(new DataUpdatedPacket(ModeType.ManualDriveMode));
        clientSender.SendPacket(new CarGPSSpotStatusPacket(tracerObj.gPSMover.gPSSpotManager));

        clientSender.SendPacket(new CarStatusPacket(baseCarDivice.GetPacketCarDivice(), GPSCore.Instance.GetGPSsPosition().GetGPSPosition(tracerObj.gPSMover.GetCurrentPositonName()), CompassCore.Instance.AngleFromN));
    }

    private void SendCamData()
    {
        texture2D = TextureToTexture2D(camManager.background.texture);
        clientSender.SendPacket(new CamPacket(texture2D.EncodeToJPG(10)));
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
