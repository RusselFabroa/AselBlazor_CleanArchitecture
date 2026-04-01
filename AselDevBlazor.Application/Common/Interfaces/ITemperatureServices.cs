using AselDevBlazor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Application.Common.Interfaces
{
    public interface ITemperatureServices
    {
        Task<ServiceResponse<List<TemperatureData>>> GetAllAsync();

        Task<ServiceResponse<List<TemperatureData>>> GetLatestAsync(int limit);

        Task<ServiceResponse<TemperatureData?>> GetByIdAsync(int id);
        Task<ServiceResponse<IEnumerable<TemperatureData>>> GetByDeviceIdAsync(string deviceId);
        Task<ServiceResponse<TemperatureData>> CreateAsync(TemperatureData dto);
        Task<ServiceResponse<TemperatureData>> UpdateAsync(int id, TemperatureData dto);
        Task<ServiceResponse<TemperatureData>> DeleteAsync(int id);


    }
}
