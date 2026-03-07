using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Common.Models;
using HajjSystem.Application.DTOs;

namespace HajjSystem.Application.Services.Interfaces;

public interface ISeasonService
{
    Task<SeasonStatsDto>   GetStatsAsync(int year);
    Task<IEnumerable<SeasonStatsDto>> GetHistoryAsync();
    Task<Result<RolloverResultDto>> PerformRolloverAsync(int newYear);
}

public record RolloverResultDto(string Message, int OldYear, int NewYear, int ArchivedPilgrims);
