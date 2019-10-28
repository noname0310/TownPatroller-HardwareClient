using System.Text;
using System;
using UnityEngine;
using UnityEngine.UI;
using ArduinoBluetoothAPI;
using TownPatroller.Bluetooth;
using TownPatroller.Bluetooth.StatusIO;
using TownPatroller.CarDevice;

public class BTCore : MonoBehaviour
{
    public GameObject objectCarDevice;

    public BluetoothHelper bluetoothHelper;
    string deviceName;

    public Text Middletext;

    private StatusDeserializer statusDeserializer;
    private PingPongManager ppManager;

    private BaseCarDivice carDivice;

    private string received_message;
    private StringBuilder packetbuffer;

    void Start()
    {
        packetbuffer = new StringBuilder();
        InitBT();

        this.gameObject.GetComponent<PingPongObj>().initOBJ();
        ppManager = this.gameObject.GetComponent<PingPongManager>();

        objectCarDevice = GameObject.Find("CarStatusObject");

        statusDeserializer = objectCarDevice.GetComponent<ObjectCarDevice>().Basecardivice.statusparser;
        statusDeserializer.OnParsed += StatusDeserializer_OnParsed;
        statusDeserializer.OnParsedError += StatusDeserializer_OnParsedError;

        carDivice = objectCarDevice.GetComponent<ObjectCarDevice>().Basecardivice;
    }

    private void StatusDeserializer_OnParsedError(char packettype, int value)
    {
        IGConsole.Instance.Main.println("ParsedERROR : " + packettype + "  " + value);
    }

    private void StatusDeserializer_OnParsed(char packettype, int value)
    {
        //IGConsole.Instance.Main.println("PacketParsed : " + packettype + "  " + value);
    }

    void Update()
    {
        ConnectBT();
        SendMovePacketByKey();
    }

    void OnDestroy()
    {
        if (bluetoothHelper != null)
        {
            bluetoothHelper.Disconnect();
            //Middletext.text = "Disconnected";
            //IGConsole.Instance.Main.println("Disconnected");
        }
    }

    #region BaseBT

    void InitBT()
    {
        deviceName = "NONAME-ARDUINO"; //bluetooth should be turned ON;
        try
        {
            bluetoothHelper = BluetoothHelper.GetInstance(deviceName);
            bluetoothHelper.OnConnected += OnConnected;
            bluetoothHelper.OnConnectionFailed += OnConnectionFailed;
            bluetoothHelper.OnDataReceived += OnMessageReceived;

            bluetoothHelper.setFixedLengthBasedStream(1);
        }
        catch (Exception ex)
        {
            Middletext.text = ex.Message;
            IGConsole.Instance.Main.println(ex.Message);
        }
    }

    void OnMessageReceived()
    {
        received_message = bluetoothHelper.Read();
        AddStringToBuffer(received_message);
        objectCarDevice.GetComponent<ObjectCarDevice>().Basecardivice.UpdateInfo(received_message);
        ppManager.CheckEOP(received_message);
    }

    void AddStringToBuffer(string msg)
    {
        packetbuffer.Append(msg);

        if(packetbuffer.Length > 20 || msg == "]")
        {
            IGConsole.Instance.Packet.println(packetbuffer.ToString());
            packetbuffer.Clear();
        }
    }

    void OnConnected()
    {
        try
        {
            bluetoothHelper.StartListening();
            Middletext.text = "Connected";
            IGConsole.Instance.Main.println("Connected");
        }
        catch (Exception ex)
        {
            IGConsole.Instance.Main.println(ex.Message);
        }

    }

    void OnConnectionFailed()
    {
        IGConsole.Instance.Main.println("Connection Failed");
    }

    #endregion

    #region BTcontrol

    void ConnectBT()
    {
        if (!bluetoothHelper.isConnected())
        {
            Middletext.text = "Disconnected";
            if (bluetoothHelper.isDevicePaired())
                bluetoothHelper.Connect(); // tries to connect
        }
    }

    public void DisconnectBT()
    {
        if (bluetoothHelper == null)
            return;

        if (bluetoothHelper.isConnected())
        {
            bluetoothHelper.Disconnect();
            Middletext.text = "Disconnected";
            IGConsole.Instance.Main.println("Disconnected");
        }
    }

    public void SendMsg(string msg)
    {
        if (bluetoothHelper != null)
        {
            if (bluetoothHelper.isConnected())
                bluetoothHelper.SendData(msg);
        }
    }

    #endregion

    #region BTSendMovePacket

    void SendMovePacketByKey()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            ppManager.CommandEnqueue(StatusSerializer.SerializeMotorSpeed(255, 255, true, true));
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            ppManager.CommandEnqueue(StatusSerializer.SerializeMotorSpeed(255, 255, false, true));
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ppManager.CommandEnqueue(StatusSerializer.SerializeMotorSpeed(255, 255, false, false));
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ppManager.CommandEnqueue(StatusSerializer.SerializeMotorSpeed(255, 255, true, false));
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            ppManager.CommandEnqueue(StatusSerializer.SerializeMotorSpeed(0, 0, true, true));
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            ppManager.CommandEnqueue(StatusSerializer.SerializeMotorSpeed(0, 0, true, true));
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            ppManager.CommandEnqueue(StatusSerializer.SerializeMotorSpeed(0, 0, true, true));
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            ppManager.CommandEnqueue(StatusSerializer.SerializeMotorSpeed(0, 0, true, true));
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            carDivice.lf_LED = !carDivice.lf_LED;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            carDivice.rf_LED = !carDivice.rf_LED;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            carDivice.lb_LED = !carDivice.lb_LED;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            carDivice.rb_LED = !carDivice.rb_LED;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (carDivice.lf_LED || carDivice.rf_LED)
            {
                carDivice.lf_LED = false;
                carDivice.rf_LED = false;
            }
            else
            {
                carDivice.lf_LED = true;
                carDivice.rf_LED = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (carDivice.lb_LED || carDivice.rb_LED)
            {
                carDivice.lb_LED = false;
                carDivice.rb_LED = false;
            }
            else
            {
                carDivice.lb_LED = true;
                carDivice.rb_LED = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (carDivice.lf_LED || carDivice.rf_LED || carDivice.lb_LED || carDivice.rb_LED)
            {
                carDivice.lf_LED = false;
                carDivice.rf_LED = false;
                carDivice.lb_LED = false;
                carDivice.rb_LED = false;
            }
            else
            {
                carDivice.lb_LED = true;
                carDivice.rb_LED = true;
                carDivice.lf_LED = true;
                carDivice.rf_LED = true;
            }
        }
    }

    #endregion
}
