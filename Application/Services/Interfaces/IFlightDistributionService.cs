using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Common.Models;

namespace HajjSystem.Application.Services.Interfaces;

public interface IFlightDistributionService
{
    Task<Result<DistributionResultDto>> AutoDistributeAsync();
    Task<Result> MovePilgrimAsync(int pilgrimId, int targetDepFlightId);
    Task<Result<int>> ClearAllAsync();
}

public record DistributionResultDto(string Message, int F1Count, int F2Count);
