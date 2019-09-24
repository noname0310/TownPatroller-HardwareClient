using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TownPatroller.Bluetooth;

public class PingPongObj : MonoBehaviour
{
    private BTCore bTCore;

    public void initOBJ()
    {
        bTCore = this.gameObject.GetComponent<BTCore>();
        this.gameObject.AddComponent<PingPongManager>();

        this.gameObject.GetComponent<PingPongManager>()._new(bTCore);
    }
}
