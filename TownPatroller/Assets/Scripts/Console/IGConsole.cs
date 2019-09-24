using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TownPatroller.Console;

public class IGConsole : MonoBehaviour
{
    public static IGConsole Instance { get; set; }

    public GameObject MainConsoleContent;
    public GameObject PacketConsoleContent;

    public InGameConsole Main;
    public InGameConsole Packet;

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        MainConsoleContent = GameObject.Find("MainConsoleContent");
        PacketConsoleContent = GameObject.Find("PacketConsoleContent");

        Main = MainConsoleContent.GetComponent<InGameConsole>();
        Packet = PacketConsoleContent.GetComponent<InGameConsole>();
    }
}
