﻿using System;
using System.Collections.Generic;
using TownPatroller.SocketClient;
using TPPacket.Packet;
using TPPacket.PacketManager;
using TPPacket.Serializer;
using UnityEngine;
using UnityEngine.UI;

public class SocketObj : MonoBehaviour
{
    public InputField IPinputField;
    public InputField PortinputField;
    public InputField IDinputField;
    public Text statuslabel;

    private PacketReciver packetReceiver;
    private Queue<Action> TaskQueue;
    private object lockObject = new object();
    private SocketClient socketClient;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        IPinputField = GameObject.Find("IPInputField").GetComponent<InputField>();
        PortinputField = GameObject.Find("PortInputField").GetComponent<InputField>();
        IDinputField = GameObject.Find("IDInputField").GetComponent<InputField>();
        statuslabel = GameObject.Find("Status").GetComponent<Text>();

        TaskQueue = new Queue<Action>();
        socketClient = new SocketClient(TaskQueue, this);
    }

    public void OnConnectBtnClicked()
    {
        if (socketClient.Connect(IPinputField.text, PortinputField.text))
        {
            ulong ID = Convert.ToUInt64(IDinputField.text);
            packetReceiver = new PacketReciver(ID);
            packetReceiver.OnDataInvoke += PacketReceiver_OnDataInvoke;
            PrintStatusLabel("Connection Pending");
            socketClient.SendInitPacket(new ConnectionPacket(true, ID, true));
        }
        else
        {
            PrintStatusLabel("Connection Failed");
        }
    }

    private void Update()
    {
        while (TaskQueue.Count > 0)
        {
            Action act;

            lock (lockObject)
            {
                act = TaskQueue.Dequeue();
                act.Invoke();
            }
        }
    }

    private void PacketReceiver_OnDataInvoke(ulong Id, BasePacket basePacket)
    {
        Debug.Log(3);
        if (basePacket.packetType == PacketType.ConnectionStat)
        {
            ConnectionPacket cp = (ConnectionPacket)basePacket;
            if(cp.IsConnecting == true)
            {
                PrintStatusLabel("Connected");
            }
            else
            {
                PrintStatusLabel("Already Connected");
            }
        }
    }

    public void OnReceiveData(byte[] Buffer)
    {
        Segment segment = (Segment)PacketDeserializer.Deserialize(Buffer);
        packetReceiver.AddSegment(segment);
        Debug.Log(segment.SegmentCount);
        Debug.Log(segment.CourrentCount);
    }

    public void PrintStatusLabel(string msg)
    {
        if (statuslabel != null)
            statuslabel.text = msg;
    }
}