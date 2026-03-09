using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.DTOs;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Domain.Entities;

namespace HajjSystem.Application.Services.Implementations;

public class UnitService : IUnitService
{
    private readonly IRepository<Unit>     _units;
    private readonly IRepository<Pilgrim>  _pilgrims;
    private readonly ICurrentUserService   _currentUser;
    private readonly IHajjSettingsAccessor _settings;

    public UnitService(
        IRepository<Unit>     units,
        IRepository<Pilgrim>  pilgrims,
        ICurrentUserService   currentUser,
        IHajjSettingsAccessor settings)
    {
        _units       = units;
        _pilgrims    = pilgrims;
        _currentUser = currentUser;
        _settings    = settings;
    }

    public IQueryable<Unit> Query() => _units.Query();

    public async Task<IEnumerable<UnitQuotaDto>> GetAllQuotasAsync()
    {
        var units = await _units.ToListAsync(_units.Query().OrderBy(u => u.UnitOrder));
        return await BuildQuotaDtos(units);
    }

    public async Task<UnitQuotaDto?> GetQuotaByUnitAsync(int unitId)
    {
        var unit = await _units.GetByIdAsync(unitId);
        if (unit is null) return null;
        var list = await BuildQuotaDtos(new[] { unit });
        return list.FirstOrDefault();
    }

    public async Task<IEnumerable<UnitQuotaDto>> GetMyQuotasAsync()
    {
        IQueryable<Unit> query = _currentUser.IsSysAdmin
            ? _units.Query().OrderBy(u => u.UnitOrder)
            : _units.Query().Where(u => u.TenantId == _currentUser.TenantId).OrderBy(u => u.UnitOrder);

        var units = await _units.ToListAsync(query);
        return await BuildQuotaDtos(units);
    }

    private async Task<IEnumerable<UnitQuotaDto>> BuildQuotaDtos(IEnumerable<Unit> units)
    {
        int year    = _settings.ActiveHajjYear;
        var unitIds = units.Select(u => u.UnitId).ToList();

        // Load pilgrims then group in memory — avoids projected IQueryable<T> type mismatch
        var pilgrims = await _pilgrims.ToListAsync(
            _pilgrims.Query().Where(p =>
                p.HajjYear == year &&
                p.UnitId   != null &&
                unitIds.Contains(p.UnitId!.Value)));

        return units.Select(u =>
        {
            int rUsed = pilgrims.Count(p => p.UnitId == u.UnitId && p.TypeId == HajjConstants.PilgrimType.Regular);
            int sUsed = pilgrims.Count(p => p.UnitId == u.UnitId && p.TypeId == HajjConstants.PilgrimType.StandBy);
            return new UnitQuotaDto(
                u.UnitId, u.UnitNameAr, u.AllowNumber, u.StandBy,
                rUsed, sUsed,
                Math.Max(0, u.AllowNumber - rUsed),
                Math.Max(0, u.StandBy    - sUsed),
                u.AllowNumber > 0 ? (int)((double)rUsed / u.AllowNumber * 100) : 0,
                u.StandBy     > 0 ? (int)((double)sUsed / u.StandBy     * 100) : 0,
                rUsed >= u.AllowNumber,
                sUsed >= u.StandBy);
        });
    }

    public Task<Unit?> GetByIdAsync(int unitId)  => _units.GetByIdAsync(unitId);
    public Task<Unit?> GetByCodeAsync(int code)  => _units.FirstOrDefaultAsync(_units.Query().Where(u => u.UnitCode == code));
    public void Add(Unit unit)    => _units.Add(unit);
    public void Update(Unit unit) => _units.Update(unit);
    public void Delete(Unit unit) => _units.Remove(unit);
}
