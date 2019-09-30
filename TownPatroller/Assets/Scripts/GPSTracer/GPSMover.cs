using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TownPatroller.CarDevice;
using TPPacket.Class;

namespace TownPatroller.GPSTracer
{
    public class GPSMover : MonoBehaviour
    {
        private GPSSpotManager gPSMover;
        private BaseCarDivice carDivice;
        private GPSCore gPSCore;
        private CompassCore compassCore;
        public bool EnableTraceMode { get; set; }

        public void _new(BaseCarDivice baseCarDivice)
        {
            gPSMover = new GPSSpotManager();
            carDivice = baseCarDivice;
            gPSCore = GPSCore.Instance;
            compassCore = CompassCore.Instance;
            EnableTraceMode = true;

            StartCoroutine(MoveCarAuto());
        }

        private IEnumerator MoveCarAuto()
        {
            yield return new WaitForSeconds(0.2f);

            while (true)
            {
                yield return null;

                if(EnableTraceMode)
                {
                    //////////////////////
                }
            }
        }
    }
}
