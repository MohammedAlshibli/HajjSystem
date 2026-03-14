using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Common.Models;
using HajjSystem.Application.DTOs;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Domain.Entities;

namespace HajjSystem.Application.Services.Implementations;

public class PilgrimService : IPilgrimService
{
    private readonly IRepository<Pilgrim>  _pilgrims;
    private readonly IRepository<Unit>     _units;
    private readonly IUnitOfWork           _uow;
    private readonly ICurrentUserService   _currentUser;
    private readonly IHajjSettingsAccessor _settings;

    public PilgrimService(
        IRepository<Pilgrim>  pilgrims,
        IRepository<Unit>     units,
        IUnitOfWork           uow,
        ICurrentUserService   currentUser,
        IHajjSettingsAccessor settings)
    {
        _pilgrims    = pilgrims;
        _units       = units;
        _uow         = uow;
        _currentUser = currentUser;
        _settings    = settings;
    }

    public IQueryable<Pilgrim> Query() => _pilgrims.Query();

    public async Task<Result<Pilgrim>> RegisterFromHrmsAsync(Pilgrim pilgrim)
    {
        int year = _settings.ActiveHajjYear;

        var unit = await _units.GetByIdAsync(pilgrim.UnitId ?? 0);
        if (unit is null)
            return Result.Failure<Pilgrim>("الوحدة غير موجودة");

        // Permanent ban — load entity first, then check in memory
        var banned = await _pilgrims.FirstOrDefaultAsync(
            _pilgrims.Query().Where(p =>
                p.NIC         == pilgrim.NIC &&
                p.ConfirmCode == HajjConstants.ConfirmCode.HQApproved));

        if (banned is not null)
            return Result.Failure<Pilgrim>(
                $"لا يمكن التسجيل — أدّى الفريضة عام {banned.HajjYear} ولا يُسمح بالتكرار");

        bool duplicate = await _pilgrims.AnyAsync(
            _pilgrims.Query().Where(p => p.NIC == pilgrim.NIC && p.HajjYear == year));
        if (duplicate)
            return Result.Failure<Pilgrim>("الموظف مسجل مسبقاً في هذه الدورة");

        using var tx = await _uow.BeginTransactionAsync();
        try
        {
            int consumed = await _pilgrims.CountAsync(_pilgrims.Query()
                .Where(p => p.UnitId   == pilgrim.UnitId &&
                             p.TypeId   == pilgrim.TypeId &&
                             p.HajjYear == year));

            int allowed = pilgrim.TypeId == HajjConstants.PilgrimType.Regular
                ? unit.AllowNumber : unit.StandBy;

            if (consumed >= allowed)
                return Result.Failure<Pilgrim>(
                    $"تجاوزت الحد المسموح به ({allowed}) لهذه الوحدة");

            Stamp(pilgrim);
            pilgrim.HajjYear         = year;
            pilgrim.RegistrationDate = DateTime.Now;
            pilgrim.FitResult        = HajjConstants.FitResult.Pending;
            pilgrim.ConfirmCode      = HajjConstants.ConfirmCode.Pending;

            _pilgrims.Add(pilgrim);
            await _uow.SaveChangesAsync();
            await tx.CommitAsync();
            return Result.Success(pilgrim);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result.Failure<Pilgrim>($"فشل الحفظ: {ex.Message}");
        }
    }

    public async Task<BanCheckDto> CheckPermanentBanAsync(string nic)
    {
        var banned = await _pilgrims.FirstOrDefaultAsync(
            _pilgrims.Query().Where(p =>
                p.NIC         == nic &&
                p.ConfirmCode == HajjConstants.ConfirmCode.HQApproved));

        if (banned is null)
            return new BanCheckDto(false, 0, string.Empty, string.Empty);

        var unit = await _units.GetByIdAsync(banned.UnitId ?? 0);
        string unitName = unit?.ArabicName ?? "—";

        return new BanCheckDto(true, banned.HajjYear, unitName,
            $"أدّى الحج عام {banned.HajjYear} — لا يُسمح بالتسجيل مرة ثانية");
    }

    public async Task<Result> SoftDeleteAsync(int pilgrimId)
    {
        var p = await _pilgrims.GetByIdAsync(pilgrimId);
        if (p is null) return Result.Failure("السجل غير موجود");
        p.IsDeleted = true;
        p.DeletedBy = _currentUser.UserName;
        p.DeletedOn = DateTime.Now;
        StampUpdate(p);
        _pilgrims.Update(p);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(Pilgrim pilgrim)
    {
        StampUpdate(pilgrim);
        _pilgrims.Update(pilgrim);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> BulkRegisterNonModAsync(IEnumerable<Pilgrim> list)
    {
        int year  = _settings.ActiveHajjYear;
        var items = list.ToList();
        if (!items.Any()) return Result.Failure("القائمة فارغة");

        var firstUnitId = items.Select(x => x.UnitId).FirstOrDefault();
        var unit        = firstUnitId.HasValue ? await _units.GetByIdAsync(firstUnitId.Value) : null;
        if (unit is null) return Result.Failure("بيانات الوحدة مفقودة");

        int regularCount = items.Count(c => c.TypeId == HajjConstants.PilgrimType.Regular);
        int standByCount = items.Count(c => c.TypeId == HajjConstants.PilgrimType.StandBy);
        if (regularCount > unit.AllowNumber)
            return Result.Failure($"تجاوزت الحد ({unit.AllowNumber}) للأصليين");
        if (standByCount > unit.StandBy)
            return Result.Failure($"تجاوزت الحد ({unit.StandBy}) للاحتياط");

        using var tx = await _uow.BeginTransactionAsync();
        try
        {
            foreach (var item in items)
            {
                bool exists = await _pilgrims.AnyAsync(
                    _pilgrims.Query().Where(p => p.NIC == item.NIC && p.HajjYear == year));
                if (exists)
                    return Result.Failure($"الموظف برقم هوية {item.NIC} مسجل مسبقاً");

                Stamp(item);
                item.HajjYear         = year;
                item.RegistrationDate = DateTime.Now;
                item.FitResult        = HajjConstants.FitResult.Pending;
                item.ConfirmCode      = HajjConstants.ConfirmCode.Pending;
                _pilgrims.Add(item);
            }
            await _uow.SaveChangesAsync();
            await tx.CommitAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result.Failure($"فشل الحفظ: {ex.Message}");
        }
    }

    private void Stamp(BaseEntity e)
    {
        e.TenantId  = _currentUser.TenantId;
        e.CreatedBy = _currentUser.UserName;
        e.CreatedOn = DateTime.Now;
        e.IsDeleted = false;
    }

    private void StampUpdate(BaseEntity e)
    {
        e.UpdatedBy = _currentUser.UserName;
        e.UpdatedOn = DateTime.Now;
    }
}
