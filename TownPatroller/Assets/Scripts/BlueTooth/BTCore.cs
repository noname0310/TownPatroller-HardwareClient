using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArduinoBluetoothAPI;
using System;
using TownPatroller.Bluetooth.StatusIO;
using TownPatroller.Console;

public class BTCore : MonoBehaviour
{
    public GameObject objectCarDevice;

    BluetoothHelper bluetoothHelper;
    string deviceName;

    public Text Middletext;
    public GameObject MainConsoleContent;
    public GameObject PacketConsoleContent;

    private InGameConsole MainConsole;
    private InGameConsole PacketConsole;

    private StatusDeserializer statusDeserializer;

    string received_message;

    void Start()
    {
        objectCarDevice = GameObject.Find("CarStatusObject");
        MainConsoleContent = GameObject.Find("MainConsoleContent");
        PacketConsoleContent = GameObject.Find("PacketConsoleContent");
        MainConsole = MainConsoleContent.GetComponent<InGameConsole>();
        PacketConsole = PacketConsoleContent.GetComponent<InGameConsole>();

        statusDeserializer = new StatusDeserializer();
        statusDeserializer.OnParsed += StatusDeserializer_OnParsed;
        statusDeserializer.OnParsedError += StatusDeserializer_OnParsedError;

        InitBT();
    }

    private void StatusDeserializer_OnParsedError(char packettype, int value)
    {
        MainConsole.println("ParsedERROR : " + packettype + "  " + value);
    }

    private void StatusDeserializer_OnParsed(char packettype, int value)
    {
        MainConsole.println("PacketParsed : " + packettype + "  " + value);
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
            Middletext.text = "Disconnected";
            MainConsole.println("Disconnected");
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

            bluetoothHelper.setFixedLengthBasedStream(8);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            Middletext.text = ex.Message;
            MainConsole.println(ex.Message);
        }
    }

    void OnMessageReceived()
    {
        received_message = bluetoothHelper.Read();
        Debug.Log(received_message);
        PacketConsole.println(received_message);
        statusDeserializer.AddDeserializeQueue(received_message);
        objectCarDevice.GetComponent<ObjectCarDevice>().Basecardivice.UpdateInfo(received_message);
    }

    void OnConnected()
    {
        try
        {
            bluetoothHelper.StartListening();
            Middletext.text = "Connected";
            MainConsole.println("Connected");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            MainConsole.println(ex.Message);
        }

    }

    void OnConnectionFailed()
    {
        Debug.Log("Connection Failed");
        MainConsole.println("Connection Failed");
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

    void DisconnectBT()
    {
        if (bluetoothHelper == null)
            return;

        if (bluetoothHelper.isConnected())
        {
            bluetoothHelper.Disconnect();
            Middletext.text = "Disconnected";
            MainConsole.println("Disconnected");
        }
    }

    void SendMsg(string msg)
    {
        bluetoothHelper.SendData(msg);
    }

    #endregion

    #region BTSendMovePacket

    void SendMovePacketByKey()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            bluetoothHelper.SendData(StatusSerializer.SerializeMotorSpeed(255, 255, true, true));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            bluetoothHelper.SendData(StatusSerializer.SerializeMotorSpeed(255, 255, false, true));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            bluetoothHelper.SendData(StatusSerializer.SerializeMotorSpeed(255, 255, false, false));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            bluetoothHelper.SendData(StatusSerializer.SerializeMotorSpeed(255, 255, true, false));
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            bluetoothHelper.SendData(StatusSerializer.SerializeMotorSpeed(0, 0, true, true));
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            bluetoothHelper.SendData(StatusSerializer.SerializeMotorSpeed(0, 0, true, true));
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            bluetoothHelper.SendData(StatusSerializer.SerializeMotorSpeed(0, 0, true, true));
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            bluetoothHelper.SendData(StatusSerializer.SerializeMotorSpeed(0, 0, true, true));
        }
    }

    #endregion
}
