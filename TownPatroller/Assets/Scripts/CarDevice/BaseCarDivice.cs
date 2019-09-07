using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TownPatroller.Bluetooth.StatusIO;

namespace TownPatroller.CarDevice
{
    public class BaseCarDivice : Cardevice
    {
        public StatusDeserializer statusparser;
        private GameObject btCore;

        public BaseCarDivice(GameObject BTcore)
        {
            btCore = BTcore;

            statusparser = new StatusDeserializer();
            statusparser.OnParsed += Statusparser_OnParsed;
        }

        private void Statusparser_OnParsed(char packettype, int value)
        {
            switch (packettype)
            {
                case 'a':
                    if (value <= 1000)
                        F_sonardist = (ushort)value;
                    break;
                case 'b':
                    if (value <= 1000)
                        RH_sonardist = (ushort)value;
                    break;
                case 'c':
                    if (value <= 1000)
                        LH_sonardist = (ushort)value;
                    break;
                case 'd':
                    if (value <= 1000)
                        RS_sonardist = (ushort)value;
                    break;
                case 'e':
                    if (value <= 1000)
                        LS_sonardist = (ushort)value;
                    break;
                case 'f':
                    if (value <= 255)
                        R_motorpower = (byte)value;
                    break;
                case 'g':
                    if (value <= 255)
                        L_motorpower = (byte)value;
                    break;
                case 'h':
                    if (value <= 1)
                        R_motorDIR = Convert.ToBoolean(value);
                    break;
                case 'i':
                    if (value <= 1)
                        L_motorDIR = Convert.ToBoolean(value);
                    break;
                case 'j':
                    if (value <= 1)
                        RF_LED = Convert.ToBoolean(value);
                    break;
                case 'k':
                    if (value <= 1)
                        LF_LED = Convert.ToBoolean(value);
                    break;
                case 'l':
                    if (value <= 1)
                        RB_LED = Convert.ToBoolean(value);
                    break;
                case 'm':
                    if (value <= 1)
                        LB_LED = Convert.ToBoolean(value);
                    break;
                default:
                    break;
            }
        }

        public void UpdateInfo(string msg)
        {
            foreach (var singlechar in msg)
            {
                statusparser.AddDeserializeQueue(singlechar);
            }
        }

        protected override void Set_R_motorpower(byte value)
        {
            string msg = StatusSerializer.SerializeSingleMotorSpeed('f', value);
            btCore.GetComponent<BTCore>().SendMsg(msg);
        }

        protected override void Set_L_motorpower(byte value)
        {
            string msg = StatusSerializer.SerializeSingleMotorSpeed('g', value);
            btCore.GetComponent<BTCore>().SendMsg(msg);
        }

        protected override void Set_R_motorDIR(bool value)
        {
            string msg = StatusSerializer.SerializeSingleMotorSpeed('h', value);
            btCore.GetComponent<BTCore>().SendMsg(msg);
        }

        protected override void Set_L_motorDIR(bool value)
        {
            string msg = StatusSerializer.SerializeSingleMotorSpeed('i', value);
            btCore.GetComponent<BTCore>().SendMsg(msg);
        }

        protected override void Set_RF_LED(bool value)
        {
            string msg = StatusSerializer.SerializeLEDPower('j', value);
            btCore.GetComponent<BTCore>().SendMsg(msg);
        }

        protected override void Set_LF_LED(bool value)
        {
            string msg = StatusSerializer.SerializeLEDPower('k', value);
            btCore.GetComponent<BTCore>().SendMsg(msg);
        }

        protected override void Set_RB_LED(bool value)
        {
            string msg = StatusSerializer.SerializeLEDPower('l', value);
            btCore.GetComponent<BTCore>().SendMsg(msg);
        }

        protected override void Set_LB_LED(bool value)
        {
            string msg = StatusSerializer.SerializeLEDPower('m', value);
            btCore.GetComponent<BTCore>().SendMsg(msg);
        }
    }
}
