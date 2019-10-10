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

    private void Start()
    {
        SendCamTexture = true;////////////////////////////////////////////////////////////////////////////////////////////////////////////
        CarStatusObject = GameObject.Find("CarStatusObject");

        objcarDevice = CarStatusObject.GetComponent<ObjectCarDevice>();
        tracerObj = CarStatusObject.GetComponent<GPSTracerObj>();

        baseCarDivice = objcarDevice.Basecardivice;
        camManager = GameObject.Find("CamManager").GetComponent<CamManager>();

        socketObj = GameObject.Find("NetworkManager(Clone)").GetComponent<SocketObj>();
        clientSender = socketObj.socketClient;
        socketObj.OnDataInvoke += SocketObj_OnDataInvoke;
        StartCoroutine(SendData());
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
                    StartCoroutine(SendCamFrame());
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
                            texture2D = TextureToTexture2D(camManager.background.texture);
                            clientSender.SendPacket(new CamPacket(texture2D.EncodeToJPG(10)));
                            Destroy(texture2D);
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

                    switch (cscrp.carMember)
                    {
                        case CarMember.r_motorpower:
                            baseCarDivice.r_motorpower = cscrp.bytevalue;
                            break;
                        case CarMember.l_motorpower:
                            baseCarDivice.l_motorpower = cscrp.bytevalue;
                            break;
                        case CarMember.r_motorDIR:
                            baseCarDivice.r_motorDIR = cscrp.boolvalue;
                            break;
                        case CarMember.l_motorDIR:
                            baseCarDivice.l_motorDIR = cscrp.boolvalue;
                            break;
                        case CarMember.rf_LED:
                            baseCarDivice.rf_LED = cscrp.boolvalue;
                            break;
                        case CarMember.lf_LED:
                            baseCarDivice.lf_LED = cscrp.boolvalue;
                            break;
                        case CarMember.rb_LED:
                            baseCarDivice.rb_LED = cscrp.boolvalue;
                            break;
                        case CarMember.lb_LED:
                            baseCarDivice.lb_LED = cscrp.boolvalue;
                            break;
                        default:
                            break;
                    }
                }
                break;

            case PacketType.CarGPSSpotStatusChangeReq:
                CarGPSSpotStatusChangeReqPacket cgscrp = (CarGPSSpotStatusChangeReqPacket)basePacket;
                tracerObj.gPSMover.ChangeSpot(cgscrp.gPSMover);
                break;

            case PacketType.UpdateDataReq:
                DataUpdatePacket dup = (DataUpdatePacket)basePacket;

                switch (dup.modeType)
                {
                    case ModeType.AutoDriveMode:
                        tracerObj.gPSMover.EnableTraceMode = true;
                        break;
                    case ModeType.ManualDriveMode:
                        tracerObj.gPSMover.EnableTraceMode = false;
                        break;
                    case ModeType.HaifManualDriveMode:
                        tracerObj.gPSMover.EnableTraceMode = false;//드라이브 자동보정 기능 넣어야됨
                        break;
                    default:
                        break;
                }
                clientSender.SendPacket(new DataUpdatedPacket(dup.modeType));
                break;

            //case PacketType.UniversalCommand:
               // break;

            default:
                break;
        }
    }

    private IEnumerator SendCamFrame()
    {
        yield return new WaitForSeconds(0.05f);
        texture2D = TextureToTexture2D(camManager.background.texture);
        clientSender.SendPacket(new CamPacket(texture2D.EncodeToJPG(10)));
        Destroy(texture2D);
    }

    private IEnumerator SendData()
    {
        yield return new WaitForSeconds(0.5f);
        texture2D = TextureToTexture2D(camManager.background.texture);////////////////////////////////////////////////////////////////////////////////////////
        clientSender.SendPacket(new CamPacket(texture2D.EncodeToJPG(10)));////////////////////////////////////////////////////////////////////////////////////
        Destroy(texture2D);///////////////////////////////////////////////
        clientSender.SendPacket(new DataUpdatedPacket(ModeType.ManualDriveMode));//미완
        clientSender.SendPacket(new CarGPSSpotStatusPacket(tracerObj.gPSMover.gPSSpotManager));
        while (false)
        {
            yield return new WaitForSeconds(2f);
            clientSender.SendPacket(new CarStatusPacket(baseCarDivice.GetPacketCarDivice(), GPSCore.Instance.GetGPSPosition(), CompassCore.Instance.AngleFromN));
        }
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
