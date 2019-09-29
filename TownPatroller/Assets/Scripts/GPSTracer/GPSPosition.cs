using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TownPatroller.GPSTracer
{
    [Serializable]
    class GPSPosition
    {
        public float latitude { get; set; }
        public float longitude { get; set; }

        public GPSPosition()
        {

        }

        public GPSPosition(float lati, float longi)
        {
            latitude = lati;
            longitude = longi;
        }
    }
}
