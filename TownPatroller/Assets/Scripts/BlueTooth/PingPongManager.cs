using System.Collections;
using System.Text;
using UnityEngine;

namespace TownPatroller.Bluetooth
{
    public class PingPongManager : MonoBehaviour
    {
        public delegate void Right2SpeakEvent();
        public event Right2SpeakEvent OnRTSpeak;

        private BTCore bTCore;
        private Coroutine EOPListenLimitEunmer;

        private StringBuilder PacketBuffer;

        private int EOPERRCount;

        public PingPongManager()
        {

        }

        public void _new(BTCore btCore)
        {
            bTCore = btCore;
            PacketBuffer = new StringBuilder("[");

            OnRTSpeak?.Invoke();
            CommandDequeue();
            EOPListenLimitEunmer = StartCoroutine(EOPListenLimiter());

            EOPERRCount = 0;
        }

        public void CommandEnqueue(string cmd)
        {
            PacketBuffer.Append(cmd);
        }

        private void CommandDequeue()
        {
            StartCoroutine(SendPacket());
        }

        public void CheckEOP(string msg)
        {
            foreach (var item in msg)
            {
                if (item == ']')
                {
                    EOPERRCount = 0;
                    StopCoroutine(EOPListenLimitEunmer);//
                    OnRTSpeak?.Invoke();
                    CommandDequeue();
                    EOPListenLimitEunmer = StartCoroutine(EOPListenLimiter());
                    break;
                }
            }
        }
        private IEnumerator SendPacket()
        {
            yield return new WaitForSeconds(0.05f);

            PacketBuffer.Append(']');
            //IGConsole.Instance.Main.println(PacketBuffer.ToString());
            bTCore.SendMsg(PacketBuffer.ToString());

            PacketBuffer.Clear();
            PacketBuffer.Append('[');
            yield break;
        }

        private IEnumerator EOPListenLimiter()
        {
            yield return new WaitForSeconds(3f);
            IGConsole.Instance.Main.println("EOP Delayed more than 3sec!");
            EOPERRCount++;

            if (EOPERRCount >= 3)
            {
                EOPERRCount = 0;
                IGConsole.Instance.Main.println("try to reconnecting");
                bTCore.DisconnectBT();
            }

            //StopCoroutine(EOPListenLimitEunmer);

            OnRTSpeak?.Invoke();
            CommandDequeue();
            EOPListenLimitEunmer = StartCoroutine(EOPListenLimiter());
            yield break;
        }
    }
}
