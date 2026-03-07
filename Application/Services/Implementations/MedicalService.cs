using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HajjSystem.Application.Services.Implementations;

public class MedicalService : IMedicalService
{
    private readonly IRepository<Pilgrim> _pilgrims;
    private readonly IUnitOfWork          _uow;
    private readonly ICurrentUserService  _currentUser;
    private readonly IHajjSettingsAccessor _settings;

    public MedicalService(IRepository<Pilgrim> pilgrims, IUnitOfWork uow, ICurrentUserService currentUser, IHajjSettingsAccessor settings)
    {
        _pilgrims    = pilgrims;
        _uow         = uow;
        _currentUser = currentUser;
        _settings    = settings;
    }

    public async Task<Result> UpdateMedicalAsync(int pilgrimId, int fitResult, string? doctorNote, string? injectionDate)
    {
        var p = await _pilgrims.GetByIdAsync(pilgrimId);
        if (p is null) return Result.Failure("السجل غير موجود");

        p.FitResult  = fitResult;
        p.DoctorNote = doctorNote;
        if (DateTime.TryParse(injectionDate, out var inj))
            p.InjectionDate = inj;

        p.UpdatedBy = _currentUser.UserName;
        p.UpdatedOn = DateTime.Now;
        _pilgrims.Update(p);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<int>> BulkMarkFitAsync(IEnumerable<int> ids)
    {
        int year = _settings.ActiveHajjYear;
        var list = ids.ToList();
        var pilgrims = await _pilgrims.Query()
            .Where(p => list.Contains(p.PilgrimId) && p.HajjYear == year)
            .ToListAsync();

        if (!pilgrims.Any()) return Result.Failure<int>("لم يتم العثور على السجلات");

        using var tx = await _uow.BeginTransactionAsync();
        try
        {
            foreach (var p in pilgrims)
            {
                p.FitResult = HajjConstants.FitResult.DoctorApproved;
                p.UpdatedBy = _currentUser.UserName;
                p.UpdatedOn = DateTime.Now;
                _pilgrims.Update(p);
            }
            await _uow.SaveChangesAsync();
            await tx.CommitAsync();
            return Result.Success(pilgrims.Count);
        }
        catch (Exception ex) { await tx.RollbackAsync(); return Result.Failure<int>(ex.Message); }
    }
}
