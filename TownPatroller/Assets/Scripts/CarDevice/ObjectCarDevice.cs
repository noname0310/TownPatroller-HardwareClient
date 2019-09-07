using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TownPatroller.CarDevice;

public class ObjectCarDevice : MonoBehaviour
{
    public BaseCarDivice Basecardivice;
    public GameObject btCore;

    void Start()
    {
        btCore = GameObject.Find("BTManager");
        Basecardivice = new BaseCarDivice(btCore);
    }
}
