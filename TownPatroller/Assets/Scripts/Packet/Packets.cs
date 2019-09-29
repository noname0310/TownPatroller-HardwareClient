using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TownPatroller.CarDevice;
using TownPatroller.GPSTracer;

namespace TownPatroller.Packet
{
    [Serializable]
    class ConnectionPacket : BasePacket
    {
        public readonly bool IsConnecting;

        public ConnectionPacket()
        {
            packetType = PacketType.ConnectionStat;
        }
        public ConnectionPacket(bool _IsConnecting)
        {
            packetType = PacketType.ConnectionStat;
            IsConnecting = _IsConnecting;
        }
    }

    [Serializable]
    class CamPacket : BasePacket
    {
        public readonly Texture CamFrame;

        public CamPacket()
        {
            packetType = PacketType.CamFrame;
        }
    }

    [Serializable]
    class CarStatusPacket : BasePacket
    {
        public readonly Cardevice cardevice;
        public readonly GPSPosition position;
        public readonly float rotation;
        public readonly GPSSpotManager gPSMover;

        public CarStatusPacket()
        {
            packetType = PacketType.CarStatus;
        }
    }

    [Serializable]
    class DataUpdatePacket : BasePacket
    {
        public readonly ModeType modeType;

        public DataUpdatePacket()
        {
            packetType = PacketType.UpdateDataReq;
        }
    }

    [Serializable]
    class DataUpdatedPacket : BasePacket
    {
        public DataUpdatedPacket()
        {
            packetType = PacketType.UpdateChanged;
        }
    }

    [Serializable]
    class UniversalCommandPacket : BasePacket
    {
        public readonly KeyType keyType;
        public readonly string key;

        public UniversalCommandPacket()
        {
            packetType = PacketType.UniversalCommand;
        }
    }
}
