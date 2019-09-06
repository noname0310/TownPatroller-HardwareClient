using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TownPatroller.CarDevice;

public class ObjectCarDevice : MonoBehaviour
{
    public BaseCarDivice Basecardivice;

    void Start()
    {
        Basecardivice = new BaseCarDivice();
    }
}
