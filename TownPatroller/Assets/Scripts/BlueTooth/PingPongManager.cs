using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TownPatroller.Bluetooth
{
    public class PingPongManager : MonoBehaviour
    {
        public delegate void Right2SpeakEvent();
        public event Right2SpeakEvent OnRTSpeak;

        private BTCore bTCore;
        private Queue<string> CommandQueue;
        private Coroutine EOPListenLimitEunmer;

        public PingPongManager()
        {

        }

        public void _new(BTCore btCore)
        {
            bTCore = btCore;
            CommandQueue = new Queue<string>();

            OnRTSpeak?.Invoke();
            CommandDequeue();
            EOPListenLimitEunmer = StartCoroutine(EOPListenLimiter());
        }

        public void CommandEnqueue(string cmd)
        {
            CommandQueue.Enqueue(cmd);
        }

        private void CommandDequeue()
        {
            bTCore.SendMsg("[");
            while (CommandQueue.Count != 0)
            {
                bTCore.SendMsg(CommandQueue.Dequeue());
            }
            bTCore.SendMsg("]");
        }

        public void CheckEOP(string msg)
        {
            foreach (var item in msg)
            {
                if (item == ']')
                {
                    StopCoroutine(EOPListenLimitEunmer);
                    OnRTSpeak?.Invoke();
                    CommandDequeue();
                    EOPListenLimitEunmer = StartCoroutine(EOPListenLimiter());
                    break;
                }
            }
        }

        private IEnumerator EOPListenLimiter()
        {
            yield return new WaitForSeconds(3f);

            OnRTSpeak?.Invoke();
            CommandDequeue();
            EOPListenLimitEunmer = StartCoroutine(EOPListenLimiter());
        }
    }
}
