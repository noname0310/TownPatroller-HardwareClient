﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.UI;
using ArduinoBluetoothAPI;
using TownPatroller.Bluetooth;
using TownPatroller.Bluetooth.StatusIO;
using TownPatroller.Console;
using TownPatroller.CarDevice;

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
        MainConsoleContent = GameObject.Find("MainConsoleContent");
        PacketConsoleContent = GameObject.Find("PacketConsoleContent");
        MainConsole = MainConsoleContent.GetComponent<InGameConsole>();
        PacketConsole = PacketConsoleContent.GetComponent<InGameConsole>();

        statusDeserializer = objectCarDevice.GetComponent<ObjectCarDevice>().Basecardivice.statusparser;
        statusDeserializer.OnParsed += StatusDeserializer_OnParsed;
        statusDeserializer.OnParsedError += StatusDeserializer_OnParsedError;

        carDivice = objectCarDevice.GetComponent<ObjectCarDevice>().Basecardivice;
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

            bluetoothHelper.setFixedLengthBasedStream(1);
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
        AddStringToBuffer(received_message);
        objectCarDevice.GetComponent<ObjectCarDevice>().Basecardivice.UpdateInfo(received_message);
        ppManager.CheckEOP(received_message);
    }

    void AddStringToBuffer(string msg)
    {
        packetbuffer.Append(msg);

        if(packetbuffer.Length > 8)
        {
            PacketConsole.println(packetbuffer.ToString());
            packetbuffer.Clear();
        }
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
