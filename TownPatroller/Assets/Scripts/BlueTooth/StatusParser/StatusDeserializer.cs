using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownPatroller.CarDevice;

namespace TownPatroller.Bluetooth.StatusIO
{
    class StatusDeserializer
    {
        public delegate void ParseEvent(char packettype, int value);
        public event ParseEvent OnParsed;

        private int index = 0;
        private char packettype = '\0';
        private string packetvalue = "";

        public void AddDeserializeQueue(char singlechar)
        {
            switch (index)
            {
                case 0:
                    if (singlechar != '{')
                    {
                        index = -1;
                        packetvalue = "";
                    }

                    break;
                case 1:
                    if ('a' <= singlechar && singlechar <= 'z')
                        packettype = singlechar;
                    else
                    {
                        index = -1;
                        packetvalue = "";
                    }

                    break;
                default:
                    if ('0' <= singlechar && singlechar <= '9')
                        packetvalue += singlechar;
                    else
                    {
                        if (singlechar == '}')
                        {
                            int value;
                            if (int.TryParse(packetvalue, out value))
                            {
                                OnParsed?.Invoke(packettype, value);
                            }
                        }
                        index = -1;
                        packetvalue = "";
                    }

                    break;
            }

            index++;
        }
    }
}
