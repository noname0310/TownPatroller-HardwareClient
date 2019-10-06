using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TownPatroller.SocketClient
{
    public class TaskQueueManager
    {
        protected Queue<Action> TaskQueue;
        protected SocketObj socketObj;

        protected void PrintlnIGConsole(string msg)
        {
            Action act;
            if (IGConsole.Instance != null)
                act = () => IGConsole.Instance.Main.println(msg);
            else
                act = () => socketObj.PrintStatusLabel(msg);
            TaskQueue.Enqueue(act);
        }

        protected void OnReceiveData(byte[] Buffer)
        {
            byte[] BufferC = new byte[Buffer.Length];
            for (int i = 0; i < Buffer.Length; i++)
            {
                BufferC[i] = Buffer[i];
            }

            Action act = () => socketObj.OnReceiveData(BufferC);
            TaskQueue.Enqueue(act);
        }
    }
}
