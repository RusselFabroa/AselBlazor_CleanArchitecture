using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Domain.Entities
{
    public class EnergyRawData
    {
        public int Id { get; set; }
        public int meter_id { get; set; }
        public DateTime timestamp { get; set; }
        public double voltage_one { get; set; }
        public double voltage_two{ get; set; }
        public double consumption { get; set; }

    }
}
