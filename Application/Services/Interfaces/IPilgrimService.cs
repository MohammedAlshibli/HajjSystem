using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Common.Models;
using HajjSystem.Application.DTOs;
using HajjSystem.Domain.Entities;

namespace HajjSystem.Application.Services.Interfaces;

public interface IPilgrimService
{
    IQueryable<Pilgrim> Query();

    Task<Result<Pilgrim>> RegisterFromHrmsAsync(Pilgrim pilgrim);
    Task<Result> SoftDeleteAsync(int pilgrimId);
    Task<Result> UpdateAsync(Pilgrim pilgrim);
    Task<Result> BulkRegisterNonModAsync(IEnumerable<Pilgrim> pilgrims);

    Task<BanCheckDto> CheckPermanentBanAsync(string nic);
}

public record BanCheckDto(bool Banned, int Year, string UnitName, string Message);
