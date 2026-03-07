using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HajjSystem.Web.Controllers;

public class FlightDistributionController : BaseController
{
    private readonly IFlightDistributionService _distSvc;
    private readonly IPilgrimService            _pilgrimSvc;
    private readonly HajjSettings               _settings;

    private readonly HajjSystem.Infrastructure.Data.AppDbContext _db;

    public FlightDistributionController(
        IFlightDistributionService distSvc,
        IPilgrimService            pilgrimSvc,
        IOptions<HajjSettings>     settings,
        HajjSystem.Infrastructure.Data.AppDbContext db)
    {
        _distSvc    = distSvc;
        _pilgrimSvc = pilgrimSvc;
        _settings   = settings.Value;
        _db         = db;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> GetEligible()
    {
        int year = _settings.ActiveHajjYear;
        var assigned = await _db.Passengers
            .Include(p => p.Flight)
            .Where(p => p.HajjYear == year && p.Flight!.ParameterId == HajjConstants.FlightDirection.Departure)
            .Select(p => p.PilgrimId).Distinct().ToListAsync();

        var data = await _pilgrimSvc.Query()
            .Include(p => p.Unit)
            .Where(p => p.HajjYear   == year &&
                         p.FitResult  == HajjConstants.FitResult.DoctorApproved &&
                         p.ConfirmCode == HajjConstants.ConfirmCode.HQApproved)
            .Select(p => new {
                p.PilgrimId, p.FullName, p.ServiceNumber, p.RankDesc, p.TypeId,
                UnitNameAr      = p.Unit != null ? p.Unit.UnitNameAr : "",
                p.BloodGroup,
                IsAdmin         = p.TypeId == HajjConstants.PilgrimType.Admin,
                AlreadyAssigned = assigned.Contains(p.PilgrimId)
            }).ToListAsync();
        return Json(data);
    }

    public async Task<IActionResult> GetDistribution()
    {
        int year = _settings.ActiveHajjYear;
        var data = await _db.Passengers
            .Include(p => p.Flight)
            .Include(p => p.Pilgrim).ThenInclude(m => m!.Unit)
            .Where(p => p.HajjYear == year)
            .Select(p => new {
                p.PassengerId, p.PilgrimId,
                Name    = p.Pilgrim!.FullName,
                Rank    = p.Pilgrim.RankDesc,
                Unit    = p.Pilgrim.Unit != null ? p.Pilgrim.Unit.UnitNameAr : "",
                IsAdmin = p.Pilgrim.TypeId == HajjConstants.PilgrimType.Admin,
                p.FlightId,
                FlightNo  = p.Flight!.FlightNo,
                Direction = p.Flight.ParameterId
            }).ToListAsync();
        return Json(data);
    }

    [HttpPost]
    public async Task<IActionResult> AutoDistribute()
    {
        var r = await _distSvc.AutoDistributeAsync();
        return ServiceResult(r, new { message = r.Value?.Message, f1 = r.Value?.F1Count, f2 = r.Value?.F2Count });
    }

    [HttpPost]
    public async Task<IActionResult> MovePilgrim(int pilgrimId, int targetFlightId) =>
        ServiceResult(await _distSvc.MovePilgrimAsync(pilgrimId, targetFlightId));

    [HttpPost]
    public async Task<IActionResult> ClearAll()
    {
        var r = await _distSvc.ClearAllAsync();
        return ServiceResult(r, new { message = $"تم مسح {r.Value} سجل", count = r.Value });
    }
}
