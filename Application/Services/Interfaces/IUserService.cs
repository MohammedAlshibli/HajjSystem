using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Common.Models;
using HajjSystem.Domain.Entities.Identity;

namespace HajjSystem.Application.Services.Interfaces;

public interface IUserService
{
    IQueryable<User> Query();
    Task<User?> GetByUserNameAsync(string userName);
    Task<IEnumerable<User>> GetAllAsync();
    Task<Result> CreateAsync(User user, IEnumerable<int> roleIds, IEnumerable<int> unitIds);
    Task<Result> UpdateAsync(User user, IEnumerable<int> roleIds, IEnumerable<int> unitIds);
    ICollection<int> GetUnitCodesByUserName(string userName);
    Task<IEnumerable<PermissionDto>> GetPermissionsAsync(string userName);
}

public record PermissionDto(string RoleName, string ControllerName, string ActionName, string? Icon, string Description);
