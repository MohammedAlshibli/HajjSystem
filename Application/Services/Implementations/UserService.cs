using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HajjSystem.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IRepository<User>       _users;
    private readonly IRepository<Role>       _roles;
    private readonly IRepository<UserRole>   _userRoles;
    private readonly IRepository<UserService> _userServices;
    private readonly IUnitOfWork             _uow;

    public UserService(
        IRepository<User>       users,
        IRepository<Role>       roles,
        IRepository<UserRole>   userRoles,
        IRepository<UserService> userServices,
        IUnitOfWork             uow)
    {
        _users        = users;
        _roles        = roles;
        _userRoles    = userRoles;
        _userServices = userServices;
        _uow          = uow;
    }

    public IQueryable<User> Query() => _users.Query();

    public Task<User?> GetByUserNameAsync(string userName) =>
        _users.Query()
            .Include(u => u.UserRoles).ThenInclude(r => r.Role)
                .ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.UserServices)
            .FirstOrDefaultAsync(u => u.UserName == userName.ToUpper());

    public Task<IEnumerable<User>> GetAllAsync() => 
        _users.Query().Include(u => u.UserRoles).Include(u => u.UserServices)
            .ToListAsync().ContinueWith(t => (IEnumerable<User>)t.Result);

    public async Task<Result> CreateAsync(User user, IEnumerable<int> roleIds, IEnumerable<int> unitIds)
    {
        try
        {
            _users.Add(user);
            await _uow.SaveChangesAsync();

            foreach (var rid in roleIds)
                _userRoles.Add(new UserRole { UserId = user.UserId, RoleId = rid });

            foreach (var uid in unitIds)
                _userServices.Add(new UserService { UserId = user.UserId, ServiceId = uid });

            await _uow.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }

    public async Task<Result> UpdateAsync(User user, IEnumerable<int> roleIds, IEnumerable<int> unitIds)
    {
        try
        {
            _users.Update(user);

            // Replace roles
            var oldRoles = await _userRoles.FindAsync(r => r.UserId == user.UserId);
            _userRoles.RemoveRange(oldRoles);
            foreach (var rid in roleIds)
                _userRoles.Add(new UserRole { UserId = user.UserId, RoleId = rid });

            // Replace unit services
            var oldSvcs = await _userServices.FindAsync(s => s.UserId == user.UserId);
            _userServices.RemoveRange(oldSvcs);
            foreach (var uid in unitIds)
                _userServices.Add(new UserService { UserId = user.UserId, ServiceId = uid });

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
        return await _users.Query()
            .Include(u => u.UserRoles).ThenInclude(r => r.Role)
                .ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
            .Where(u => u.UserName == userName.ToUpper())
            .SelectMany(u => u.UserRoles.SelectMany(ur =>
                ur.Role!.RolePermissions.Select(rp => new PermissionDto(
                    ur.Role.Name,
                    rp.Permission!.ControllerName,
                    rp.Permission.ActionName,
                    rp.Permission.Icon,
                    rp.Permission.ScreenNameAr))))
            .ToListAsync();
    }
}
