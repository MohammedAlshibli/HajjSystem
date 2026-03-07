using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HajjSystem.Application.Services.Implementations;

public class ConfirmationService : IConfirmationService
{
    private readonly IRepository<Pilgrim> _pilgrims;
    private readonly IUnitOfWork          _uow;
    private readonly ICurrentUserService  _currentUser;
    private readonly IHajjSettingsAccessor _settings;

    public ConfirmationService(
        IRepository<Pilgrim> pilgrims,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IHajjSettingsAccessor settings)
    {
        _pilgrims    = pilgrims;
        _uow         = uow;
        _currentUser = currentUser;
        _settings    = settings;
    }

    public async Task<Result> ConfirmAsync(int pilgrimId)
    {
        var p = await _pilgrims.GetByIdAsync(pilgrimId);
        if (p is null) return Result.Failure("السجل غير موجود");
        if (p.ConfirmCode == HajjConstants.ConfirmCode.Confirmed)
            return Result.Failure("تم تأكيد هذا الحاج مسبقاً");

        p.ConfirmCode = HajjConstants.ConfirmCode.Confirmed;
        Stamp(p);
        _pilgrims.Update(p);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<int>> ConfirmBulkAsync(IEnumerable<int> ids)
    {
        int year = _settings.ActiveHajjYear;
        var list = ids.ToList();
        var pilgrims = await _pilgrims.Query()
            .Where(p => list.Contains(p.PilgrimId) &&
                         p.HajjYear    == year &&
                         p.ConfirmCode == HajjConstants.ConfirmCode.Pending)
            .ToListAsync();

        if (!pilgrims.Any()) return Result.Failure<int>("لا توجد سجلات في انتظار التأكيد");

        using var tx = await _uow.BeginTransactionAsync();
        try
        {
            foreach (var p in pilgrims) { p.ConfirmCode = HajjConstants.ConfirmCode.Confirmed; Stamp(p); _pilgrims.Update(p); }
            await _uow.SaveChangesAsync();
            await tx.CommitAsync();
            return Result.Success(pilgrims.Count);
        }
        catch (Exception ex) { await tx.RollbackAsync(); return Result.Failure<int>(ex.Message); }
    }

    public async Task<Result> CancelAsync(int pilgrimId, string cancelNote)
    {
        var p = await _pilgrims.GetByIdAsync(pilgrimId);
        if (p is null) return Result.Failure("السجل غير موجود");
        p.ConfirmCode = HajjConstants.ConfirmCode.Cancelled;
        p.CancelNote  = cancelNote;
        Stamp(p);
        _pilgrims.Update(p);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(int pilgrimId)
    {
        var p = await _pilgrims.GetByIdAsync(pilgrimId);
        if (p is null) return Result.Failure("السجل غير موجود");
        p.ConfirmCode = HajjConstants.ConfirmCode.Pending;
        p.CancelNote  = null;
        Stamp(p);
        _pilgrims.Update(p);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<int>> FinalApproveAllAsync()
    {
        int year = _settings.ActiveHajjYear;
        var pilgrims = await _pilgrims.Query()
            .Where(p => p.HajjYear == year && p.ConfirmCode == HajjConstants.ConfirmCode.Confirmed)
            .ToListAsync();
        if (!pilgrims.Any()) return Result.Failure<int>("لا توجد سجلات مؤكدة من الأسلحة");
        return await ApproveList(pilgrims);
    }

    public async Task<Result<int>> FinalApproveSelectedAsync(IEnumerable<int> ids)
    {
        int year = _settings.ActiveHajjYear;
        var list = ids.ToList();
        var pilgrims = await _pilgrims.Query()
            .Where(p => list.Contains(p.PilgrimId) &&
                         p.HajjYear    == year &&
                         p.ConfirmCode == HajjConstants.ConfirmCode.Confirmed)
            .ToListAsync();
        if (!pilgrims.Any()) return Result.Failure<int>("لا توجد سجلات مؤكدة من المحددين");
        return await ApproveList(pilgrims);
    }

    private async Task<Result<int>> ApproveList(List<Pilgrim> pilgrims)
    {
        using var tx = await _uow.BeginTransactionAsync();
        try
        {
            foreach (var p in pilgrims) { p.ConfirmCode = HajjConstants.ConfirmCode.HQApproved; Stamp(p); _pilgrims.Update(p); }
            await _uow.SaveChangesAsync();
            await tx.CommitAsync();
            return Result.Success(pilgrims.Count);
        }
        catch (Exception ex) { await tx.RollbackAsync(); return Result.Failure<int>(ex.Message); }
    }

    public async Task<Result> ReturnToBranchAsync(int pilgrimId, string? returnNote)
    {
        var p = await _pilgrims.GetByIdAsync(pilgrimId);
        if (p is null) return Result.Failure("السجل غير موجود");
        p.ConfirmCode = HajjConstants.ConfirmCode.Pending;
        p.CancelNote  = string.IsNullOrWhiteSpace(returnNote)
                         ? "أُعيد من الإدارة العليا للمراجعة"
                         : returnNote;
        Stamp(p);
        _pilgrims.Update(p);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    private void Stamp(Pilgrim p) { p.UpdatedBy = _currentUser.UserName; p.UpdatedOn = DateTime.Now; }
}
