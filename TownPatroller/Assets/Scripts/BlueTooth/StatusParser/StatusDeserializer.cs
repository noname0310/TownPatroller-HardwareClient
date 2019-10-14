using System.Text;

namespace TownPatroller.Bluetooth.StatusIO
{
    public class StatusDeserializer
    {
        public delegate void ParseEvent(char packettype, int value);
        public delegate void ParseP();
        public event ParseEvent OnParsed;
        public event ParseEvent OnParsedError;
        public event ParseP OnParsedSOP;
        public event ParseP OnParsedEOP;

        private int index = 0;
        private char packettype = '\0';
        private StringBuilder packetvalue = new StringBuilder();

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
                        packetvalue.Clear();

                        if (singlechar == '[')
                            OnParsedSOP?.Invoke();

                        else if (singlechar == ']')
                            OnParsedEOP?.Invoke();

                        else if (int.TryParse(packetvalue.ToString(), out outvalue))
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
                        packetvalue.Clear();

                        if (int.TryParse(packetvalue.ToString(), out outvalue))
                        {
                            OnParsedError?.Invoke(packettype, outvalue);
                        }
                    }

                    break;
                default:
                    if ('0' <= singlechar && singlechar <= '9')
                        packetvalue.Append(singlechar);
                    else
                    {
                        if (singlechar == '}')
                        {
                            if (int.TryParse(packetvalue.ToString(), out outvalue))
                            {
                                OnParsed?.Invoke(packettype, outvalue);
                            }
                        }
                        else
                        {
                            if (int.TryParse(packetvalue.ToString(), out outvalue))
                            {
                                OnParsedError?.Invoke(packettype, outvalue);
                            }
                        }
                        index = -1;
                        packetvalue.Clear();
                    }

                    break;
            }

            index++;
        }
    }
}
