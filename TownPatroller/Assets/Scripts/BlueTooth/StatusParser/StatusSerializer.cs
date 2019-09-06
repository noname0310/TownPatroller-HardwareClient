using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TownPatroller.Bluetooth.StatusIO
{
    class StatusSerializer
    {
        public static string SerializeMotorSpeed(byte R_motorpower, byte L_motorpower, bool R_motorDIR, bool L_motorDIR)
        {
            string msg = GetSinglePacket('f', R_motorpower) + GetSinglePacket('g', L_motorpower) + GetSinglePacket('h', R_motorDIR) + GetSinglePacket('i', L_motorDIR);

            return msg;
        }

        private static string GetSinglePacket(char packettype, byte packetvalue)
        {
            return "{" + packettype + packetvalue.ToString("D") + "}";
        }
        private static string GetSinglePacket(char packettype, bool packetvalue)
        {
            return "{" + packettype + Convert.ToInt32(packetvalue).ToString() + "}";
        }
    }
}
