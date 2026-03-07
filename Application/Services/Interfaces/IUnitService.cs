using HajjSystem.Application.Common.Models;
using HajjSystem.Application.DTOs;
using HajjSystem.Domain.Entities;

namespace HajjSystem.Application.Services.Interfaces;

public interface IUnitService
{
    IQueryable<Unit> Query();

    Task<IEnumerable<UnitQuotaDto>> GetAllQuotasAsync();
    Task<UnitQuotaDto?> GetQuotaByUnitAsync(int unitId);

    /// <summary>Returns quota only for units the current officer manages.</summary>
    Task<IEnumerable<UnitQuotaDto>> GetMyQuotasAsync();

    Task<Unit?> GetByIdAsync(int unitId);
    Task<Unit?> GetByCodeAsync(int unitCode);
    void Add(Unit unit);
    void Update(Unit unit);
    void Delete(Unit unit);
}
