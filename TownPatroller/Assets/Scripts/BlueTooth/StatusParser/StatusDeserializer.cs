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
        public event ParseEvent OnParsedError;

        private int index = 0;
        private char packettype = '\0';
        private string packetvalue = "";

        private int outvalue;

        public void AddDeserializeQueue(string msg)
        {
            foreach (var item in msg)
            {
                AddDeserializeQueue(item);
            }
        }

        public void AddDeserializeQueue(char singlechar)
        {
            switch (index)
            {
                case 0:
                    if (singlechar != '{')
                    {
                        index = -1;
                        packetvalue = "";

                        if (int.TryParse(packetvalue, out outvalue))
                        {
                            OnParsedError?.Invoke(packettype, outvalue);
                        }
                    }

                    break;
                case 1:
                    if ('a' <= singlechar && singlechar <= 'z')
                        packettype = singlechar;
                    else
                    {
                        index = -1;
                        packetvalue = "";

                        if (int.TryParse(packetvalue, out outvalue))
                        {
                            OnParsedError?.Invoke(packettype, outvalue);
                        }
                    }

                    break;
                default:
                    if ('0' <= singlechar && singlechar <= '9')
                        packetvalue += singlechar;
                    else
                    {
                        if (singlechar == '}')
                        {
                            if (int.TryParse(packetvalue, out outvalue))
                            {
                                OnParsed?.Invoke(packettype, outvalue);
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
