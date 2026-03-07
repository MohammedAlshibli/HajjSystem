using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Common.Models;

namespace HajjSystem.Application.Services.Interfaces;

public interface IMedicalService
{
    Task<Result> UpdateMedicalAsync(int pilgrimId, int fitResult, string? doctorNote, string? injectionDate);
    Task<Result<int>> BulkMarkFitAsync(IEnumerable<int> pilgrimIds);
}
