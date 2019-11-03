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

        private object lockObject; 
        
        public TaskQueueManager(object LockObject)
        {
            lockObject = LockObject;
        }

        protected void PrintlnIGConsole(string msg)
        {
            Action act;
            if (IGConsole.Instance != null)
                act = () => IGConsole.Instance.Main.println(msg);
            else
                act = () => socketObj.PrintStatusLabel(msg);
            lock (lockObject)
            {
                TaskQueue.Enqueue(act);
            }
        }

        protected void OnReceiveData(byte[] Buffer)
        {
            byte[] BufferC = new byte[Buffer.Length];
            for (int i = 0; i < Buffer.Length; i++)
            {
                BufferC[i] = Buffer[i];
            }

            Action act = () => socketObj.OnReceiveData(BufferC);
            lock (lockObject)
            {
                TaskQueue.Enqueue(act);
            }
        }
    }
}
