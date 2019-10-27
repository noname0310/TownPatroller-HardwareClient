using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPPacket.Class;

namespace TownPatroller.PhoneDevice.GPS
{
    public class GPSsPosition
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

        public GPSPosition GetGPSPosition(string Location)
        {
            return new GPSPosition(Location, latitude, longitude);
        }

        public static GPSsPosition operator +(GPSsPosition p1, GPSsPosition p2)
        {
            return new GPSsPosition(p1.latitude + p2.latitude, p1.longitude + p2.longitude);
        }
        public static GPSsPosition operator -(GPSsPosition p1, GPSsPosition p2)
        {
            return new GPSsPosition(p1.latitude - p2.latitude, p1.longitude - p2.longitude);
        }
    }
    public static class GPSPosHelper
    {
        public static GPSsPosition GetGPSS(this GPSPosition gPSPosition)
        {
            return new GPSsPosition(gPSPosition.latitude, gPSPosition.longitude);
        }
    }
}

