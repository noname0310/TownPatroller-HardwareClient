using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TownPatroller.GPSTracer
{
    [Serializable]
    class GPSSpotManager
    {
        private List<GPSPosition> GPSPositions { get; set; }

        public GPSSpotManager()
        {
            GPSPositions = new List<GPSPosition>();
        }

        public void AddPos(GPSPosition gPSPosition)
        {
            GPSPositions.Add(gPSPosition);
        }

        public GPSPosition RemoveLastPos()
        {
            if (GPSPositions.Count == 0)
                return null;

            GPSPosition gPSPosition = GPSPositions[0];

            GPSPositions.RemoveAt(0);

            return gPSPosition;
        }

        public void SetMovePos(List<GPSPosition> _gPSPositions)
        {
            GPSPositions = _gPSPositions;
        }
    }
}
