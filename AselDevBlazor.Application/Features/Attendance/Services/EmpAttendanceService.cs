using AselDevBlazor.Application.Common;
using AselDevBlazor.Application.Common.Interfaces;
using AselDevBlazor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Application.Features.Attendance.Services
{
    public class EmpAttendanceService : IEmpAttendanceService
    {
        private readonly IDbContextFactory _dbContext;
        private readonly ILogger<EmpAttendanceService> _logger;
        public EmpAttendanceService(IDbContextFactory dbContextFactory, ILogger<EmpAttendanceService> logger)
        {
            _dbContext = dbContextFactory;
            _logger = logger;

        }

        public async Task<ServiceResponse<List<EmpAttendanceData>>> GetAllAsync()
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var records = await db.Set<EmpAttendanceData>().ToListAsync();
                var response = new ServiceResponse<List<EmpAttendanceData>>(records, "Successfully retrieved attendance records.",200);
                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<List<EmpAttendanceData>>($"Failed to retrieve attendance records.{ex.Message}",500);
                _logger.LogError(ex, "Error retrieving attendance records.");
                return response;
            }
           
        }

        public async Task<ServiceResponse<EmpAttendanceData?>> GetByIdAsync(int id)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var record = await db.Set<EmpAttendanceData>().FindAsync(id);

                var response = record != null
                    ? new ServiceResponse<EmpAttendanceData?>(record, $"Successfully retrieved attendance record with ID {id}.",200)
                    : new ServiceResponse<EmpAttendanceData?>($"Attendance record with ID {id} not found.",404);
                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<EmpAttendanceData?>($"Failed to retrieve attendance record with ID {id}.{ex.Message}",500);
                _logger.LogError(ex, $"Error retrieving attendance record with ID {id}.");
                return response;
            }
           
        }

        public async Task<ServiceResponse<IEnumerable<EmpAttendanceData>>> GetByEmpIdAsync(string empId)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var records = await db.Set<EmpAttendanceData>()
                    .Where(e => e.EmpId == empId)
                    .ToListAsync();

                if (records.Count == 0)
                {
                    var response = new ServiceResponse<IEnumerable<EmpAttendanceData>>($"No attendance records found for employee ID {empId}.", 404);
                    return response;
                }

                return new ServiceResponse<IEnumerable<EmpAttendanceData>>(records, $"Successfully retrieved attendance records for employee ID {empId}.", 200);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<IEnumerable<EmpAttendanceData>>($"Failed to retrieve attendance records for employee ID {empId}.{ex.Message}", 500);

                _logger.LogError(ex, $"Error retrieving attendance records for employee ID {empId}.");
                return response;
            }
           
        }

        public async Task<ServiceResponse<EmpAttendanceData>> CreateAsync(EmpAttendanceData dto)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var entry = await db.Set<EmpAttendanceData>().AddAsync(dto);
                await db.SaveChangesAsync();
                var response = new ServiceResponse<EmpAttendanceData>(entry.Entity, "Successfully created attendance record.",201); 
                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<EmpAttendanceData>($"Failed to create attendance record.{ex.Message}",500);
                _logger.LogError(ex, "Error creating attendance record.");
                return response;
            }
           
        }

        public async Task<ServiceResponse<EmpAttendanceData>> UpdateAsync(int id, EmpAttendanceData dto)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var existing = await db.Set<EmpAttendanceData>().FindAsync(id);
                if (existing == null)
                    throw new KeyNotFoundException($"Attendance record with ID {id} not found.");
                existing.EmpName = dto.EmpName;
                existing.EmpId = dto.EmpId;
                existing.EmpShift = dto.EmpShift;
                existing.Timestamp = dto.Timestamp;
                existing.AttendanceType = dto.AttendanceType;
                db.Set<EmpAttendanceData>().Update(existing);
                await db.SaveChangesAsync();

                var response = new ServiceResponse<EmpAttendanceData>("Successfully updated attendance record.",200);
                
                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<EmpAttendanceData>($"Failed to update attendance record with ID {id}.{ex.Message}",500);
                _logger.LogError(ex, $"Error updating attendance record with ID {id}.");
                return response;
            }
           
        }

        public async Task<ServiceResponse<EmpAttendanceData>> DeleteAsync(int id)
        {
            try
            {
                await using var db = _dbContext.CreateDbContext("DefaultConnection");
                var existing = await db.Set<EmpAttendanceData>().FindAsync(id);
                if (existing == null)
                    throw new KeyNotFoundException($"Attendance record with ID {id} not found.");
                db.Set<EmpAttendanceData>().Remove(existing);
                await db.SaveChangesAsync();

                var response = new ServiceResponse<EmpAttendanceData>("Successfully deleted attendance record.",200);

                return response;
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse<EmpAttendanceData>($"Failed to delete attendance record with ID {id}.{ex.Message}",500);
                _logger.LogError(ex, $"Error deleting attendance record with ID {id}.");
                return response;
            }
           
        }
    }
}
