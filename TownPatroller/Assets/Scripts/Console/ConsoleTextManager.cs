using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TownPatroller.Console;

public class ConsoleTextManager : MonoBehaviour
{
    public GameObject MainConsoleContent;
    public GameObject PacketConsoleContent;
    public Text MainTextPrefab;
    public Text PacketTextPrefab;

    private InGameConsole MainConsole;
    private InGameConsole PacketConsole;

    // Start is called before the first frame update
    void Start()
    {
        MainConsoleContent = GameObject.Find("MainConsoleContent");
        PacketConsoleContent = GameObject.Find("PacketConsoleContent");

        MainConsole = new InGameConsole(MainConsoleContent, MainTextPrefab);
        PacketConsole = new InGameConsole(PacketConsoleContent, PacketTextPrefab);

        for (int i = 0; i < 10; i++)
            MainConsole.println("1234567891234567891234567891234sjdhskjfhskdlfmlflkfjbrejfeiojfejioj");
    }

    // Update is called once per frame
    void Update()
    {
    }
}
