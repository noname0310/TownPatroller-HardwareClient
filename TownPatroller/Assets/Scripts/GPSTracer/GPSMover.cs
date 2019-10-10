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
        public GPSSpotManager gPSSpotManager;
        private BaseCarDivice carDivice;
        public bool EnableTraceMode { get; set; }

        public void _new(BaseCarDivice baseCarDivice)
        {
            gPSSpotManager = new GPSSpotManager(-1);
            carDivice = baseCarDivice;
            EnableTraceMode = true;

            StartCoroutine(MoveCarAuto());
        }

        public void ChangeSpot(GPSSpotManager gPSSpotManager)
        {

        }

        private IEnumerator MoveCarAuto()
        {
            gPSSpotManager.AddPos(new GPSPosition(null, 121, 312));
            yield return new WaitForSeconds(0.2f);

            while (true)
            {
                yield return null;

                if(EnableTraceMode && gPSSpotManager.GPSPositions.Count != 0)
                {
                    //////////////////////
                }
            }
        }
    }
}
