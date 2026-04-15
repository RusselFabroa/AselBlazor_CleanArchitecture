using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Domain.Entities;

public class HREmployeeList
{
    public string empNo { get; set; }
    public string lName { get; set; } = string.Empty;
    public string fName { get; set; } = string.Empty;
    public string mName { get; set; } = string.Empty;
    public string lbrCode { get; set; } = string.Empty;
    public string deptCode { get; set; } = string.Empty;
    public string deptDesc { get; set; } = string.Empty;
    public string jobDesc { get; set; } = string.Empty;
    public string jobGrade { get; set; } = string.Empty;
    public string empStat { get; set; } = string.Empty;
    public string bu { get; set; } = string.Empty;
    public string cstcntrcode { get; set; } = string.Empty;
    public string cstcntrdesc { get; set; } = string.Empty;

    public string GetFullName
    {
        get
        {
            if (!string.IsNullOrEmpty(mName))
            {
                return $"{fName} {mName} {lName}";
            }
            else
            {
                return $"{fName} {lName}";
            }
        }
    }
}
