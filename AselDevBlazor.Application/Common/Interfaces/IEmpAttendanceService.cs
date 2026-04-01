using AselDevBlazor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AselDevBlazor.Domain.Entities;

namespace AselDevBlazor.Application.Common.Interfaces;

public interface IEmpAttendanceService
{
    Task<ServiceResponse<List<EmpAttendanceData>>> GetAllAsync();
    Task<ServiceResponse<EmpAttendanceData?>> GetByIdAsync(int id);
    Task<ServiceResponse<IEnumerable<EmpAttendanceData>>> GetByEmpIdAsync(string empId);
    Task<ServiceResponse<EmpAttendanceData>> CreateAsync(EmpAttendanceData dto);
    Task<ServiceResponse<EmpAttendanceData>> UpdateAsync(int id, EmpAttendanceData dto);
    Task<ServiceResponse<EmpAttendanceData>> DeleteAsync(int id);
}
