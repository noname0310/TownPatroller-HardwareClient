using UnityEngine;
using TownPatroller.CarDevice;

public class ObjectCarDevice : MonoBehaviour
{
    public BaseCarDivice Basecardivice;
    public GameObject btCore;
    public GameObject StatusUI;

    void Awake()
    {
        btCore = GameObject.Find("BTManager");
        StatusUI = GameObject.Find("CarStatusPanel");
        Basecardivice = new BaseCarDivice(btCore, StatusUI);
    }
}
