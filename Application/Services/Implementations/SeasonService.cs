using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Common.Models;
using HajjSystem.Application.DTOs;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HajjSystem.Application.Services.Implementations;

public class SeasonService : ISeasonService
{
    private readonly IRepository<Pilgrim>  _pilgrims;
    private readonly IRepository<Unit>     _units;
    private readonly IHajjSettingsAccessor _settings;

    public SeasonService(
        IRepository<Pilgrim>  pilgrims,
        IRepository<Unit>     units,
        IHajjSettingsAccessor settings)
    {
        _pilgrims = pilgrims;
        _units    = units;
        _settings = settings;
    }

    public async Task<SeasonStatsDto> GetStatsAsync(int year)
    {
        var pilgrims = await _pilgrims.Query()
            .Where(p => p.HajjYear == year)
            .Select(p => new { p.ConfirmCode, p.TypeId, p.UnitId })
            .ToListAsync();

        var units = await _units.Query()
            .Select(u => new { u.UnitId, u.UnitNameAr, u.AllowNumber, u.StandBy })
            .ToListAsync();

        var unitBreakdown = units.Select(u => new UnitBreakdownDto(
            u.UnitId, u.UnitNameAr, u.AllowNumber, u.StandBy,
            pilgrims.Count(p => p.UnitId == u.UnitId),
            pilgrims.Count(p => p.UnitId == u.UnitId && p.ConfirmCode == HajjConstants.ConfirmCode.HQApproved)
        )).ToList();

        return new SeasonStatsDto(
            year,
            pilgrims.Count,
            pilgrims.Count(p => p.ConfirmCode == HajjConstants.ConfirmCode.HQApproved),
            pilgrims.Count(p => p.ConfirmCode is HajjConstants.ConfirmCode.Pending or HajjConstants.ConfirmCode.Confirmed),
            pilgrims.Count(p => p.ConfirmCode == HajjConstants.ConfirmCode.Cancelled),
            pilgrims.Count(p => p.TypeId == HajjConstants.PilgrimType.Regular),
            pilgrims.Count(p => p.TypeId == HajjConstants.PilgrimType.StandBy),
            pilgrims.Count(p => p.TypeId == HajjConstants.PilgrimType.Admin),
            year == _settings.ActiveHajjYear,
            unitBreakdown);
    }

    public async Task<IEnumerable<SeasonStatsDto>> GetHistoryAsync()
    {
        var years = await _pilgrims.Query()
            .Select(p => p.HajjYear)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        var result = new List<SeasonStatsDto>();
        foreach (var y in years)
            result.Add(await GetStatsAsync(y));
        return result;
    }

    public async Task<Result<RolloverResultDto>> PerformRolloverAsync(int newYear)
    {
        int currentYear = _settings.ActiveHajjYear;
        if (newYear <= currentYear)
            return Result.Failure<RolloverResultDto>($"السنة الجديدة ({newYear}) يجب أن تكون أكبر من الحالية ({currentYear})");

        int pending = await _pilgrims.Query()
            .CountAsync(p => p.HajjYear == currentYear &&
                              p.ConfirmCode != HajjConstants.ConfirmCode.HQApproved &&
                              p.ConfirmCode != HajjConstants.ConfirmCode.Cancelled);
        if (pending > 0)
            return Result.Failure<RolloverResultDto>($"يوجد {pending} سجل لم تتم الموافقة النهائية عليه");

        int archived = await _pilgrims.Query().CountAsync(p => p.HajjYear == currentYear);

        _settings.UpdateActiveYear(newYear);

        return Result.Success(new RolloverResultDto(
            $"تم الترحيل بنجاح — الموسم الجديد: {newYear}",
            currentYear, newYear, archived));
    }
}
