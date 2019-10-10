using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPPacket.Class;

namespace TownPatroller.PhoneDevice.GPS
{
    class GPSsPosition
    {
        public readonly float latitude;
        public readonly float longitude;

        public GPSsPosition(float lati, float longi)
        {
            latitude = lati;
            longitude = longi;
        }

        public GPSPosition GetGPSPosition()
        {
            return new GPSPosition("N/A", latitude, longitude);
        }
    }
}
