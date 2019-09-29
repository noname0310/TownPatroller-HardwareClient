using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TownPatroller.GPSTracer;

public class GPSTracerObj : MonoBehaviour
{
    public GPSMover gPSMover;
    public ObjectCarDevice objectCarDevice;

    void Start()
    {
        objectCarDevice = gameObject.GetComponent<ObjectCarDevice>();

        gPSMover = gameObject.AddComponent<GPSMover>();
        gPSMover._new(objectCarDevice.Basecardivice);
    }
}
