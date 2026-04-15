using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Domain.Entities;

public class Manhour
{
    public string? EMPNO { get; set; } = null;
    public string? LNAME { get; set; } = null;
    public string? FNAME { get; set; } = null;
    public string? DEPTCODE { get; set; } = null;
    public string? LBRCODE { get; set; } = null;
    public string? POSITION { get; set; } = null;
    public string? SHIFT { get; set; } = null;
    public DateTime? LOGDATE { get; set; }
    public int? WW { get; set; } = null;
    public int? DTYPE { get; set; } = null;
    public DateTime? LOGIN { get; set; } = null;
    public DateTime? LOGOUT { get; set; } = null;
    public DateTime? RFIDIN { get; set; } = null;
    public DateTime? RFIDOUT { get; set; } = null;
    public int? REGULARHOUR { get; set; } = null;
    public int? OVERTIME { get; set; } = null;
    public string? JOBGRADE { get; set; } = null;
    public string? JOBCODE { get; set; } = null;
    public string? JOBDESC { get; set; } = null;
    public string? LCODE { get; set; } = null;
    public int? LHOURS { get; set; } = null;
}
