using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TownPatroller.Console;

public class ConsoleTextManager : MonoBehaviour
{
    public GameObject MainConsoleContent;
    public GameObject PacketConsoleContent;

    public GameObject MainScrollView;
    public GameObject PacketScrollView;

    public Text MainTextPrefab;
    public Text PacketTextPrefab;

    private ScrollRect MainScrollRect;
    private ScrollRect PacketScrollRect;

    private InGameConsole MainConsole;
    private InGameConsole PacketConsole;

    // Start is called before the first frame update
    void Awake()
    {
        MainScrollView = GameObject.Find("Console Scroll View");
        PacketScrollView = GameObject.Find("Packet Scroll View");

        MainScrollRect = MainScrollView.GetComponent<ScrollRect>();
        PacketScrollRect = PacketScrollView.GetComponent<ScrollRect>();

        MainConsoleContent = GameObject.Find("MainConsoleContent");
        PacketConsoleContent = GameObject.Find("PacketConsoleContent");

        DeleteChildObject(MainConsoleContent);
        DeleteChildObject(PacketConsoleContent);

        MainConsoleContent.AddComponent<InGameConsole>();
        MainConsole = MainConsoleContent.GetComponent<InGameConsole>();
        MainConsole._new(MainConsoleContent, MainTextPrefab, MainScrollRect);

        PacketConsoleContent.AddComponent<InGameConsole>();
        PacketConsole = PacketConsoleContent.GetComponent<InGameConsole>();
        PacketConsole._new(PacketConsoleContent, PacketTextPrefab, PacketScrollRect);

        MainConsole.println(" ");
        PacketConsole.println(" ");
    }

    // Update is called once per frame
    void Update()
    {
    }

    void DeleteChildObject(GameObject ParentObject)
    {
        for (int i = 0; i < ParentObject.transform.childCount; i++)
        {
            Destroy(ParentObject.transform.GetChild(i).GetComponent<Text>());
        }
    }
}
