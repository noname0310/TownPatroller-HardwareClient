using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TownPatroller.SocketClient;
using TPPacket.Packet;
using TPPacket.PacketManager;
using TPPacket.Serializer;

public class SocketObj : MonoBehaviour
{
    public delegate void DataInvoked(BasePacket basePacket);
    public event DataInvoked OnDataInvoke;

    public InputField IPinputField;
    public InputField PortinputField;
    public InputField IDinputField;
    public Text statuslabel;

    private PacketReciver packetReceiver;
    private Queue<Action> TaskQueue;
    public SocketClient socketClient;

    ulong ID;

    private object lockObject = new object();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        TaskQueue = new Queue<Action>();
        socketClient = new SocketClient(TaskQueue, this, lockObject);
    }

    public void OnConnectBtnClicked()
    {
        if (IPinputField == null || PortinputField == null || IDinputField == null || statuslabel == null)
        {
            IPinputField = GameObject.Find("IPInputField").GetComponent<InputField>();
            PortinputField = GameObject.Find("PortInputField").GetComponent<InputField>();
            IDinputField = GameObject.Find("IDInputField").GetComponent<InputField>();
            statuslabel = GameObject.Find("Status").GetComponent<Text>();
        }

        if (socketClient.Connect(IPinputField.text, PortinputField.text))
        {
            ID = Convert.ToUInt64(IDinputField.text);
            packetReceiver = new PacketReciver(ID);
            packetReceiver.OnDataInvoke += PacketReceiver_OnDataInvoke;
            PrintStatusLabel("Connection Pending");
            socketClient.SendPacket(new ConnectionPacket(true, ID, true));
        }
        else
        {
            PrintStatusLabel("Connection Failed");
        }
    }

    private void Update()
    {
        while (TaskQueue.Count > 0)
        {
            Action act;

            lock (lockObject)
            {
                act = TaskQueue.Dequeue();
            }
            act.Invoke();
        }
    }

    private void OnApplicationQuit()
    {
        Task task = new Task(() => QuitClient());
        task.Start();
    }

    private void QuitClient()
    {
        socketClient.SendPacket(new ConnectionPacket(false, ID, true));
        socketClient.Stop();
    }

    private void PacketReceiver_OnDataInvoke(ulong Id, BasePacket basePacket)
    {
        if (basePacket.packetType == PacketType.ConnectionStat)
        {
            ConnectionPacket cp = (ConnectionPacket)basePacket;
            if(cp.IsConnecting == true)
            {
                PrintStatusLabel("Connected");
                SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            }
            else
            {
                if (cp.HasError == true)
                {
                    socketClient.ErrorStop();
                    PrintStatusLabel("Already Connected");
                }
                else
                {
                    socketClient.Stop();
                    SceneManager.LoadScene("ConnectScene", LoadSceneMode.Single);
                }
            }
        }
        OnDataInvoke?.Invoke(basePacket);
    }

    public void OnReceiveData(byte[] Buffer)
    {
        packetReceiver.AddSegment(Buffer);
    }

    public void PrintStatusLabel(string msg)
    {
        if (statuslabel != null)
            statuslabel.text = msg;
    }
}
