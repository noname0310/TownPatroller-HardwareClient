﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TPPacket.Serializer;

namespace TownPatroller.SocketClient
{
    interface IClientSender
    {
        void SendPacket(object packet);
    }

    public class SocketClient : TaskQueueManager, IClientSender
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private PacketSerializer packetSerializer;

        public byte[] ReadBuffer;
        public byte[] SendBuffer;

        private Task ClientTask;
        private bool StopTask;

        public SocketClient(Queue<Action> taskQueue, SocketObj _socketObj)
        {
            TaskQueue = taskQueue;
            socketObj = _socketObj;

            ReadBuffer = new byte[1024 * 4];
            SendBuffer = new byte[1024 * 4];
            packetSerializer = new PacketSerializer(1024 * 3, 1024 * 4);
        }

        public bool Connect(string ip, string port)
        {
            StopTask = true;

            tcpClient = new TcpClient();
            try
            {
                IPAddress serverip = IPAddress.Parse(ip);
                int serverport = int.Parse(port);

                IPEndPoint iPEndPoint = new IPEndPoint(serverip, serverport);

                tcpClient.Connect(iPEndPoint);
            }
            catch
            {
                return false;
            }

            networkStream = tcpClient.GetStream();

            Start();
            return true;
        }

        private void Start()
        {
            StopTask = false;
            ClientTask = new Task(new Action(InitSocket));
            ClientTask.Start();
        }

        public void ErrorStop()
        {
            StopTask = true;
        }

        public void Stop()
        {
            StopTask = true;
            PrintlnIGConsole("Disconnected from server");
        }

        private void InitSocket()
        {
            while (!StopTask)
            {
                if (tcpClient.Connected)
                {
                    if (0 < tcpClient.Available)
                    {
                        networkStream.Read(ReadBuffer, 0, ReadBuffer.Length);
                        OnReceiveData(ReadBuffer);
                    }
                }
                else
                {
                    StopTask = true;
                }
            }
            tcpClient.Close();
            networkStream.Close();
            networkStream.Dispose();
        }

        public void SendInitPacket(object packet)
        {
            packetSerializer.SerializeSingle(SendBuffer, packet);
            SendData();
        }

        public void SendPacket(object packet)
        {
            packetSerializer.Serialize(packet);
            while (true)
            {
                int result = packetSerializer.GetSerializedSegment(SendBuffer);

                if (result == 0)
                {
                    break;
                }

                SendData();
            }
            packetSerializer.Clear();
        }
        private void SendData()
        {
            networkStream.Write(SendBuffer, 0, SendBuffer.Length);
            networkStream.Flush();

            for (int i = 0; i < SendBuffer.Length; i++)
            {
                SendBuffer[i] = 0;
            }
        }
    }
}
