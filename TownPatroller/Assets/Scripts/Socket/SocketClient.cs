using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TPPacket.Serializer;

namespace TownPatroller.SocketClient
{
    public interface IClientSender
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

        private const int SegmentSize = 1024;

        public SocketClient(Queue<Action> taskQueue, SocketObj _socketObj, object LockObject) : base(LockObject)
        {
            TaskQueue = taskQueue;
            socketObj = _socketObj;

            ReadBuffer = new byte[SegmentSize];
            SendBuffer = new byte[SegmentSize];
            packetSerializer = new PacketSerializer(1012);

            StopTask = true;
        }

        public bool Connect(string ip, string port)
        {
            StopTask = true;

            tcpClient = new TcpClient();
            try
            {
                IPAddress serverip = Dns.GetHostAddresses(ip)[0];
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
            if (StopTask == false)
            {
                StopTask = true;
                PrintlnIGConsole("Disconnected from server");
            }
        }

        private void InitSocket()
        {
            while (!StopTask)
            {
                if (tcpClient.Connected)
                {
                    if (0 < tcpClient.Available)
                    {
                        ReadNetworkStreamToReadBuffer(0, PacketHeaderSize.HeaderSize);
                        HeaderInfo headerInfo = PacketDeserializer.ParseHeader(ReadBuffer);
                        ReadNetworkStreamToReadBuffer(PacketHeaderSize.HeaderSize, headerInfo.SegmentLength);

                        OnReceiveData(ReadBuffer, PacketHeaderSize.HeaderSize + headerInfo.SegmentLength);
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

        public void ReadNetworkStreamToReadBuffer(int offset, int Length)
        {
            int remainingBytes = Length;
            int Offset = offset;

            while (remainingBytes > 0)
            {
                var readBytes = networkStream.Read(ReadBuffer, Offset, remainingBytes);
                if (readBytes == 0)
                {
                    throw new Exception("disconnected");
                }
                Offset += readBytes;
                remainingBytes -= readBytes;
            }
        }

        public void SendPacket(object packet)
        {
            packetSerializer.Serialize(packet);
            for (int i = 0; i < packetSerializer.SegmentCount; i++)
            {
                int fullSegmentLength = packetSerializer.GetSerializedSegment(SendBuffer);

                SendData(fullSegmentLength);
            }
            packetSerializer.Clear();
        }

        private void SendData(int length)
        {
            networkStream.Write(SendBuffer, 0, length);
            networkStream.Flush();
        }
    }
}