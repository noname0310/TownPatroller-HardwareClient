using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArduinoBluetoothAPI;
using System;
using TownPatroller.Bluetooth.StatusIO;

public class BTCore : MonoBehaviour
{
    public GameObject objectCarDevice;

    BluetoothHelper bluetoothHelper;
    string deviceName;

    public Text Middletext;
    string received_message;

    void Start()
    {
        InitBT();
        objectCarDevice = GameObject.Find("CarStatusObject");
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

            bluetoothHelper.setFixedLengthBasedStream(3);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            Middletext.text = ex.Message;
        }
    }

    void OnMessageReceived()
    {
        received_message = bluetoothHelper.Read();
        Debug.Log(received_message);
        Middletext.text = received_message;
        objectCarDevice.GetComponent<ObjectCarDevice>().Basecardivice.UpdateInfo(received_message);
    }

    void OnConnected()
    {
        try
        {
            bluetoothHelper.StartListening();
            Middletext.text = "Connected";
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            Middletext.text = ex.Message;
        }

    }

    void OnConnectionFailed()
    {
        Debug.Log("Connection Failed");
        Middletext.text = "Connection Failed";
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
