﻿using System.Collections;
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
        IP.text = "noname0310.iptime.org";
        Port.text = "20310";
        ID.text = Random.Range(10000000, 99999999).ToString();
    }
}
