using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Domain.Entities
{
    public class EmpAttendanceData
    {
        public int Id { get; set; }
        public string EmpName { get; set; } = "John";
        public string EmpId { get; set; } = "DS";
        public string EmpShift { get; set; } = "DS";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public byte AttendanceType { get; set; } = 1;

        public string GetAttendanceValue()
        {
            return AttendanceType switch
            {
                1 => "Present",
                2 => "Absent",
                3 => "Late",
                _ => "Unknown"
            };
        }
    }
}
