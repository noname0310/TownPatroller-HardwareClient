using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownPatroller.Bluetooth.StatusIO;

namespace TownPatroller.CarDevice
{
    public class BaseCarDivice : Cardevice
    {
        StatusDeserializer statusparser;

        public BaseCarDivice()
        {
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
    }
}
