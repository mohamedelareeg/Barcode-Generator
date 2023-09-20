using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barcode_Generator.Model
{
    [Serializable]
    public class AppSettings
    {
        public string DeviceIdentifier { get; set; }
        public int MaxBarcodePrints { get; set; }
        public int UsedBarcodePrints { get; set; }
        public DateTime LicenseExpirationDate { get; set; }
    }
}
