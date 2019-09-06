using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TownPatroller.CarDevice
{
    [Serializable]
    public class Cardevice
    {
        public ushort f_sonardist { get { return F_sonardist; } }
        public ushort rh_sonardist { get { return RH_sonardist; } }
        public ushort lh_sonardist { get { return LH_sonardist; } }
        public ushort rs_sonardist { get { return RS_sonardist; } }
        public ushort ls_sonardist { get { return LS_sonardist; } }
        public byte R_motorpower { get; set; }
        public byte L_motorpower { get; set; }
        public bool R_motorDIR { get; set; }
        public bool L_motorDIR { get; set; }

        protected ushort F_sonardist;
        protected ushort RH_sonardist;
        protected ushort LH_sonardist;
        protected ushort RS_sonardist;
        protected ushort LS_sonardist;

    }
}