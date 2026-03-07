using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Common.Models;

namespace HajjSystem.Application.Services.Interfaces;

public interface IConfirmationService
{
    Task<Result> ConfirmAsync(int pilgrimId);
    Task<Result<int>> ConfirmBulkAsync(IEnumerable<int> pilgrimIds);
    Task<Result> CancelAsync(int pilgrimId, string cancelNote);
    Task<Result> RestoreAsync(int pilgrimId);

    // HQ (Central) actions
    Task<Result<int>> FinalApproveAllAsync();
    Task<Result<int>> FinalApproveSelectedAsync(IEnumerable<int> pilgrimIds);
    Task<Result> ReturnToBranchAsync(int pilgrimId, string? returnNote);
}
