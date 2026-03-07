using HajjSystem.Application.DTOs;

namespace HajjSystem.Application.Services.Interfaces;

public interface IHrmsService
{
    Task<HrmsEmployeeDto?> GetEmployeeByServiceNumberAsync(string serviceNumber);
}
