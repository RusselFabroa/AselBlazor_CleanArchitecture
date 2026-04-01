using AselDevBlazor.Application.Common;
using AselDevBlazor.Application.Common.Interfaces;
using AselDevBlazor.Application.Features.Attendance.Services;
using AselDevBlazor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Application.Features.Temperature
{
    public class TemperatureService : ITemperatureServices
    {
        private readonly IDbContextFactory _dbContext;
        private readonly ILogger<TemperatureData> _logger;
        public TemperatureService(IDbContextFactory dbContextFactory, ILogger<TemperatureData> logger)
        {
            _dbContext = dbContextFactory;
            _logger = logger;

        }

        public async Task<ServiceResponse<List<TemperatureData>>> GetAllAsync()
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var records = await db.Set<TemperatureData>().ToListAsync();
                var response = new ServiceResponse<List<TemperatureData>>(records, "Successfully retrieved attendance records.", 200);
                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<List<TemperatureData>>($"Failed to retrieve attendance records.{ex.Message}", 500);
                _logger.LogError(ex, "Error retrieving attendance records.");
                return response;
            }

        }

        public async Task<ServiceResponse<List<TemperatureData>>> GetLatestAsync(int limit)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var records = await db.Set<TemperatureData>()
                    .OrderByDescending(e => e.Timestamp)
                    .Take(limit)
                    .ToListAsync();
                var response = new ServiceResponse<List<TemperatureData>>(records, "Successfully retrieved latest attendance records.", 200);
                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<List<TemperatureData>>($"Failed to retrieve latest attendance records.{ex.Message}", 500);
                _logger.LogError(ex, "Error retrieving latest attendance records.");
                return response;
            }
        }

        public async Task<ServiceResponse<TemperatureData?>> GetByIdAsync(int id)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var record = await db.Set<TemperatureData>().FindAsync(id);

                var response = record != null
                    ? new ServiceResponse<TemperatureData?>(record, $"Successfully retrieved attendance record with ID {id}.", 200)
                    : new ServiceResponse<TemperatureData?>($"Attendance record with ID {id} not found.", 404);
                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<TemperatureData?>($"Failed to retrieve attendance record with ID {id}.{ex.Message}", 500);
                _logger.LogError(ex, $"Error retrieving attendance record with ID {id}.");
                return response;
            }

        }

        public async Task<ServiceResponse<IEnumerable<TemperatureData>>> GetByDeviceIdAsync(string deviceId)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var records = await db.Set<TemperatureData>()
                    .Where(e => e.deviceID == deviceId)
                    .ToListAsync();

                if (records.Count == 0)
                {
                    var response = new ServiceResponse<IEnumerable<TemperatureData>>($"No attendance records found for employee ID {deviceId}.", 404);
                    return response;
                }

                return new ServiceResponse<IEnumerable<TemperatureData>>(records, $"Successfully retrieved attendance records for employee ID {deviceId}.", 200);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<IEnumerable<TemperatureData>>($"Failed to retrieve attendance records for employee ID {deviceId}.{ex.Message}", 500);

                _logger.LogError(ex, $"Error retrieving attendance records for employee ID {deviceId}.");
                return response;
            }

        }

        public async Task<ServiceResponse<TemperatureData>> CreateAsync(TemperatureData dto)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var entry = await db.Set<TemperatureData>().AddAsync(dto);
                await db.SaveChangesAsync();
                var response = new ServiceResponse<TemperatureData>(entry.Entity, "Successfully created attendance record.", 201);
                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<TemperatureData>($"Failed to create attendance record.{ex.Message}", 500);
                _logger.LogError(ex, "Error creating attendance record.");
                return response;
            }

        }

        public async Task<ServiceResponse<TemperatureData>> UpdateAsync(int id, TemperatureData dto)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var existing = await db.Set<TemperatureData>().FindAsync(id);
                if (existing == null)
                    throw new KeyNotFoundException($"Attendance record with ID {id} not found.");
              
                existing.deviceID = dto.deviceID;
               
                existing.Timestamp = dto.Timestamp;
              
                db.Set<TemperatureData>().Update(existing);
                await db.SaveChangesAsync();

                var response = new ServiceResponse<TemperatureData>("Successfully updated attendance record.", 200);

                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<TemperatureData>($"Failed to update attendance record with ID {id}.{ex.Message}", 500);
                _logger.LogError(ex, $"Error updating attendance record with ID {id}.");
                return response;
            }

        }

        public async Task<ServiceResponse<TemperatureData>> DeleteAsync(int id)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var existing = await db.Set<TemperatureData>().FindAsync(id);
                if (existing == null)
                    throw new KeyNotFoundException($"Attendance record with ID {id} not found.");
                db.Set<TemperatureData>().Remove(existing);
                await db.SaveChangesAsync();

                var response = new ServiceResponse<TemperatureData>("Successfully deleted attendance record.", 200);

                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<TemperatureData>($"Failed to delete attendance record with ID {id}.{ex.Message}", 500);
                _logger.LogError(ex, $"Error deleting attendance record with ID {id}.");
                return response;
            }

        }
    }
}
