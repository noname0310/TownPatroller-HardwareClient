using System;
using UnityEngine;
using TownPatroller.Bluetooth;
using TownPatroller.Bluetooth.StatusIO;

namespace TownPatroller.CarDevice
{
    public class BaseCarDivice : Cardevice
    {
        public StatusDeserializer statusparser;
        private GameObject btCore;
        private CarStatusUI StatusUI;
        public bool HalfManualMode;

        public BaseCarDivice(GameObject BTcore, GameObject statusUI)
        {
            btCore = BTcore;
            StatusUI = statusUI.GetComponent<CarStatusUI>();

            statusparser = new StatusDeserializer();
            statusparser.OnParsed += Statusparser_OnParsed;

            HalfManualMode = false;
        }

        private void Statusparser_OnParsed(char packettype, int value)
        {
            switch (packettype)
            {
                case 'a':
                    if (value <= 1000)
                    {
                        F_sonardist = (ushort)value;
                        StatusUI.SetText("FDStext", value);
                    }
                    break;
                case 'b':
                    if (value <= 1000)
                    {
                        RH_sonardist = (ushort)value;
                        StatusUI.SetText("FRHStext", value);
                    }
                    break;
                case 'c':
                    if (value <= 1000)
                    {
                        LH_sonardist = (ushort)value;
                        StatusUI.SetText("FLHStext", value);
                    }
                    break;
                case 'd':
                    if (value <= 1000)
                    {
                        RS_sonardist = (ushort)value;
                        StatusUI.SetText("RDStext", value);
                    }
                    break;
                case 'e':
                    if (value <= 1000)
                    {
                        LS_sonardist = (ushort)value;
                        StatusUI.SetText("LDStext", value);
                    }
                    break;
                case 'f':
                    if (value <= 255)
                    {
                        R_motorpower = (byte)value;

                        int motorvalue = value;
                        if (!R_motorDIR)
                            motorvalue *= -1;

                        StatusUI.SetText("RMtext", motorvalue);
                    }
                    break;
                case 'g':
                    if (value <= 255)
                    {
                        L_motorpower = (byte)value;

                        int motorvalue = value;
                        if (!L_motorDIR)
                            motorvalue *= -1;

                        StatusUI.SetText("LMtext", motorvalue);
                    }
                    break;
                case 'h':
                    if (value <= 1)
                    {
                        R_motorDIR = Convert.ToBoolean(value);

                        int motorvalue = R_motorpower;
                        if (!R_motorDIR)
                            motorvalue *= -1;

                        StatusUI.SetText("RMtext", motorvalue);
                    }
                    break;
                case 'i':
                    if (value <= 1)
                    {
                        L_motorDIR = Convert.ToBoolean(value);

                        int motorvalue = L_motorpower;
                        if (!L_motorDIR)
                            motorvalue *= -1;

                        StatusUI.SetText("LMtext", motorvalue);
                    }
                    break;
                case 'j':
                    if (value <= 1)
                    {
                        RF_LED = Convert.ToBoolean(value);
                        StatusUI.ChangeLEDStatus(CarStatusUI.LEDtype.FRLED, RF_LED);
                    }
                    break;
                case 'k':
                    if (value <= 1)
                    {
                        LF_LED = Convert.ToBoolean(value);
                        StatusUI.ChangeLEDStatus(CarStatusUI.LEDtype.FLLED, LF_LED);
                    }
                    break;
                case 'l':
                    if (value <= 1)
                    {
                        RB_LED = Convert.ToBoolean(value);
                        StatusUI.ChangeLEDStatus(CarStatusUI.LEDtype.BRLED, RB_LED);
                    }
                    break;
                case 'm':
                    if (value <= 1)
                    {
                        LB_LED = Convert.ToBoolean(value);
                        StatusUI.ChangeLEDStatus(CarStatusUI.LEDtype.BLLED, LB_LED);
                    }
                    break;
                default:
                    break;
            }
        }

        public void UpdateInfo(string msg)
        {
            if (msg == null)
                return;

            foreach (var singlechar in msg)
            {
                statusparser.AddDeserializeQueue(singlechar);
            }
        }

        private bool preinfo_R_motorDIR;
        private bool preinfo_L_motorDIR;

        protected override void Set_R_motorpower(byte value)
        {
            if (HalfManualMode == true)
            {
                if (preinfo_R_motorDIR == true)//front
                {
                    if (rh_sonardist > 10 || lh_sonardist > 10)
                    {
                        return;
                    }
                }
            }

            string msg = StatusSerializer.SerializeSingleMotorSpeed('f', value);
            btCore.GetComponent<PingPongManager>().CommandEnqueue(msg);
        }

        protected override void Set_L_motorpower(byte value)
        {
            if (HalfManualMode == true)
            {
                if (preinfo_L_motorDIR == true)//front
                {
                    if (rh_sonardist > 10 || lh_sonardist > 10)
                    {
                        return;
                    }
                }
            }

            string msg = StatusSerializer.SerializeSingleMotorSpeed('g', value);
            btCore.GetComponent<PingPongManager>().CommandEnqueue(msg);
        }

        protected override void Set_R_motorDIR(bool value)
        {
            preinfo_R_motorDIR = value;
            string msg = StatusSerializer.SerializeSingleMotorSpeed('h', value);
            btCore.GetComponent<PingPongManager>().CommandEnqueue(msg);
        }

        protected override void Set_L_motorDIR(bool value)
        {
            preinfo_L_motorDIR = value;
            string msg = StatusSerializer.SerializeSingleMotorSpeed('i', value);
            btCore.GetComponent<PingPongManager>().CommandEnqueue(msg);
        }

        protected override void Set_RF_LED(bool value)
        {
            string msg = StatusSerializer.SerializeLEDPower('j', value);
            btCore.GetComponent<PingPongManager>().CommandEnqueue(msg);
        }

        protected override void Set_LF_LED(bool value)
        {
            string msg = StatusSerializer.SerializeLEDPower('k', value);
            btCore.GetComponent<PingPongManager>().CommandEnqueue(msg);
        }

        protected override void Set_RB_LED(bool value)
        {
            string msg = StatusSerializer.SerializeLEDPower('l', value);
            btCore.GetComponent<PingPongManager>().CommandEnqueue(msg);
        }

        protected override void Set_LB_LED(bool value)
        {
            string msg = StatusSerializer.SerializeLEDPower('m', value);
            btCore.GetComponent<PingPongManager>().CommandEnqueue(msg);
        }
    }
}
