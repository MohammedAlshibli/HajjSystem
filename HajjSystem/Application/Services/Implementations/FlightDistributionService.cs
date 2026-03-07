using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Common.Models;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HajjSystem.Application.Services.Implementations;

public class FlightDistributionService : IFlightDistributionService
{
    private readonly IRepository<Pilgrim>   _pilgrims;
    private readonly IRepository<Flight>    _flights;
    private readonly IRepository<Passenger> _passengers;
    private readonly IUnitOfWork            _uow;
    private readonly ICurrentUserService    _currentUser;
    private readonly IHajjSettingsAccessor  _settings;

    public FlightDistributionService(
        IRepository<Pilgrim>   pilgrims,
        IRepository<Flight>    flights,
        IRepository<Passenger> passengers,
        IUnitOfWork            uow,
        ICurrentUserService    currentUser,
        IHajjSettingsAccessor  settings)
    {
        _pilgrims   = pilgrims;
        _flights    = flights;
        _passengers = passengers;
        _uow        = uow;
        _currentUser= currentUser;
        _settings   = settings;
    }

    /// <summary>
    /// Distribution rules:
    ///  1. Admins → always Departure F1
    ///  2. Fill F1 to half capacity, rest to F2
    ///  3. Departure F1 passengers → Return F2 (mandatory link)
    ///     Departure F2 passengers → Return F1 (mandatory link)
    /// </summary>
    public async Task<Result<DistributionResultDto>> AutoDistributeAsync()
    {
        int year = _settings.ActiveHajjYear;

        var flights = await _flights.Query()
            .Where(f => f.FlightYear == year)
            .OrderBy(f => f.FlightDate)
            .ToListAsync();

        var depFlights = flights.Where(f => f.ParameterId == HajjConstants.FlightDirection.Departure)
            .OrderBy(f => f.FlightDate).ToList();
        var retFlights = flights.Where(f => f.ParameterId == HajjConstants.FlightDirection.Return)
            .OrderBy(f => f.FlightDate).ToList();

        if (depFlights.Count < 2 || retFlights.Count < 2)
            return Result.Failure<DistributionResultDto>("يجب تسجيل رحلتي ذهاب ورحلتي عودة أولاً");

        var dep1 = depFlights[0]; var dep2 = depFlights[1];
        var ret1 = retFlights[0]; var ret2 = retFlights[1];

        // Clear existing assignments
        var existing = await _passengers.Query().Where(p => p.HajjYear == year).ToListAsync();
        _passengers.RemoveRange(existing);

        var pilgrims = await _pilgrims.Query()
            .Include(p => p.Unit)
            .Where(p => p.HajjYear   == year &&
                         p.FitResult  == HajjConstants.FitResult.DoctorApproved &&
                         p.ConfirmCode == HajjConstants.ConfirmCode.HQApproved)
            .OrderByDescending(p => p.TypeId == HajjConstants.PilgrimType.Admin)
            .ThenBy(p => p.RegistrationDate)
            .ToListAsync();

        if (!pilgrims.Any()) return Result.Failure<DistributionResultDto>("لا يوجد حجاج لائقون طبياً");

        int half      = (int)Math.Ceiling(dep1.FlightCapacity / 2.0);
        var dep1List  = new List<Pilgrim>();
        var dep2List  = new List<Pilgrim>();

        foreach (var p in pilgrims)
        {
            if (p.TypeId == HajjConstants.PilgrimType.Admin || dep1List.Count < half)
                dep1List.Add(p);
            else
                dep2List.Add(p);
        }

        using var tx = await _uow.BeginTransactionAsync();
        try
        {
            foreach (var p in dep1List)
            {
                AddPassenger(p.PilgrimId, dep1.FlightId, year);
                AddPassenger(p.PilgrimId, ret2.FlightId, year);
            }
            foreach (var p in dep2List)
            {
                AddPassenger(p.PilgrimId, dep2.FlightId, year);
                AddPassenger(p.PilgrimId, ret1.FlightId, year);
            }
            await _uow.SaveChangesAsync();
            await tx.CommitAsync();
            return Result.Success(new DistributionResultDto(
                $"تم التوزيع: {dep1List.Count} في الرحلة الأولى، {dep2List.Count} في الثانية",
                dep1List.Count, dep2List.Count));
        }
        catch (Exception ex) { await tx.RollbackAsync(); return Result.Failure<DistributionResultDto>(ex.Message); }
    }

    public async Task<Result> MovePilgrimAsync(int pilgrimId, int targetDepFlightId)
    {
        int year = _settings.ActiveHajjYear;

        var pilgrim = await _pilgrims.GetByIdAsync(pilgrimId);
        if (pilgrim is null) return Result.Failure("الحاج غير موجود");

        var allFlights = await _flights.Query()
            .Where(f => f.FlightYear == year).OrderBy(f => f.FlightDate).ToListAsync();

        var depFlights = allFlights.Where(f => f.ParameterId == HajjConstants.FlightDirection.Departure)
            .OrderBy(f => f.FlightDate).ToList();
        var retFlights = allFlights.Where(f => f.ParameterId == HajjConstants.FlightDirection.Return)
            .OrderBy(f => f.FlightDate).ToList();

        if (pilgrim.TypeId == HajjConstants.PilgrimType.Admin &&
            depFlights.Any() && targetDepFlightId != depFlights[0].FlightId)
            return Result.Failure("الحجاج الإداريون يجب أن يبقوا في الرحلة الأولى");

        var target = allFlights.FirstOrDefault(f => f.FlightId == targetDepFlightId);
        if (target is null) return Result.Failure("الرحلة غير موجودة");

        int count = await _passengers.Query()
            .CountAsync(p => p.FlightId == targetDepFlightId && p.HajjYear == year);
        if (count >= target.FlightCapacity)
            return Result.Failure($"الرحلة ممتلئة (السعة: {target.FlightCapacity})");

        int targetRetFlightId = targetDepFlightId == depFlights[0].FlightId
            ? retFlights[1].FlightId : retFlights[0].FlightId;

        var depIds = depFlights.Select(f => f.FlightId).ToList();
        var retIds = retFlights.Select(f => f.FlightId).ToList();

        var depPass = await _passengers.Query()
            .FirstOrDefaultAsync(p => p.PilgrimId == pilgrimId && p.HajjYear == year && depIds.Contains(p.FlightId));
        var retPass = await _passengers.Query()
            .FirstOrDefaultAsync(p => p.PilgrimId == pilgrimId && p.HajjYear == year && retIds.Contains(p.FlightId));

        if (depPass is null) return Result.Failure("السجل غير موجود في قائمة الرحلات");

        depPass.FlightId = targetDepFlightId; Stamp(depPass); _passengers.Update(depPass);
        if (retPass is not null) { retPass.FlightId = targetRetFlightId; Stamp(retPass); _passengers.Update(retPass); }

        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<int>> ClearAllAsync()
    {
        int year = _settings.ActiveHajjYear;
        var all  = await _passengers.Query().Where(p => p.HajjYear == year).ToListAsync();
        _passengers.RemoveRange(all);
        await _uow.SaveChangesAsync();
        return Result.Success(all.Count);
    }

    private void AddPassenger(int pilgrimId, int flightId, int year)
    {
        var p = new Passenger
        {
            PilgrimId   = pilgrimId,
            FlightId    = flightId,
            BusId       = 1,
            ResidenceId = 1,
            HajjYear    = year,
            TenantId    = _currentUser.TenantId,
            CreatedBy   = _currentUser.UserName,
            CreatedOn   = DateTime.Now
        };
        _passengers.Add(p);
    }

    private void Stamp(Passenger p) { p.UpdatedBy = _currentUser.UserName; p.UpdatedOn = DateTime.Now; }
}
