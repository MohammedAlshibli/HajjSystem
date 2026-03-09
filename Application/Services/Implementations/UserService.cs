using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using DomainUserService = HajjSystem.Domain.Entities.Identity.UserService;

namespace HajjSystem.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IRepository<User>              _users;
    private readonly IRepository<Role>              _roles;
    private readonly IRepository<UserRole>          _userRoles;
    private readonly IRepository<DomainUserService> _userServices;
    private readonly IUnitOfWork                    _uow;

    public UserService(
        IRepository<User>              users,
        IRepository<Role>              roles,
        IRepository<UserRole>          userRoles,
        IRepository<DomainUserService> userServices,
        IUnitOfWork                    uow)
    {
        _users        = users;
        _roles        = roles;
        _userRoles    = userRoles;
        _userServices = userServices;
        _uow          = uow;
    }

    public IQueryable<User> Query() => _users.Query();

    public Task<User?> GetByUserNameAsync(string userName) =>
        _users.FirstOrDefaultAsync(
            _users.Query()
                .Include(u => u.UserRoles).ThenInclude(r => r.Role)
                    .ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
                .Include(u => u.UserServices)
                .Where(u => u.UserName == userName.ToUpper()));

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _users.ToListAsync(
            _users.Query()
                .Include(u => u.UserRoles)
                .Include(u => u.UserServices));
    }

    public async Task<r> CreateAsync(User user, IEnumerable<int> roleIds, IEnumerable<int> unitIds)
    {
        try
        {
            _users.Add(user);
            await _uow.SaveChangesAsync();

            foreach (var rid in roleIds)
                _userRoles.Add(new UserRole { UserId = user.UserId, RoleId = rid });

            foreach (var uid in unitIds)
                _userServices.Add(new DomainUserService { UserId = user.UserId, ServiceId = uid });

            await _uow.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }

    public async Task<r> UpdateAsync(User user, IEnumerable<int> roleIds, IEnumerable<int> unitIds)
    {
        try
        {
            _users.Update(user);

            var oldRoles = await _userRoles.FindAsync(r => r.UserId == user.UserId);
            _userRoles.RemoveRange(oldRoles);
            foreach (var rid in roleIds)
                _userRoles.Add(new UserRole { UserId = user.UserId, RoleId = rid });

            var oldSvcs = await _userServices.FindAsync(s => s.UserId == user.UserId);
            _userServices.RemoveRange(oldSvcs);
            foreach (var uid in unitIds)
                _userServices.Add(new DomainUserService { UserId = user.UserId, ServiceId = uid });

            await _uow.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }

    public ICollection<int> GetUnitCodesByUserName(string userName) =>
        _users.Query()
            .Include(u => u.UserServices).ThenInclude(s => s.Unit)
            .Where(u => u.UserName == userName.ToUpper())
            .SelectMany(u => u.UserServices.Select(s => s.Unit!.UnitCode))
            .ToList();

    public async Task<IEnumerable<PermissionDto>> GetPermissionsAsync(string userName)
    {
        var users = await _users.ToListAsync(
            _users.Query()
                .Include(u => u.UserRoles).ThenInclude(r => r.Role)
                    .ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
                .Where(u => u.UserName == userName.ToUpper()));

        return users.SelectMany(u => u.UserRoles.SelectMany(ur =>
            ur.Role!.RolePermissions.Select(rp => new PermissionDto(
                ur.Role.Name,
                rp.Permission!.ControllerName,
                rp.Permission.ActionName,
                rp.Permission.Icon,
                rp.Permission.ScreenNameAr))));
    }
}
