using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestHelper : MonoBehaviour
{
    public InputField IP;
    public InputField Port;
    public InputField ID;

    void Start()
    {
        IP.text = "127.0.0.1";
        Port.text = "20310";
        ID.text = "245135";
    }
}
