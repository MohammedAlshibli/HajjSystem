using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Domain.Entities;
using HajjSystem.Domain.Enums;
using HajjSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace HajjSystem.Web.Controllers;

/// <summary>
/// Pilgrim registration (was AlhajjMastersController).
/// All business logic delegated to IPilgrimService.
/// </summary>
public class PilgrimsController : BaseController
{
    private readonly IPilgrimService _pilgrimService;
    private readonly IUnitService    _unitService;
    private readonly IHrmsService    _hrmsService;
    private readonly HajjSettings    _settings;

    public PilgrimsController(
        IPilgrimService pilgrimService,
        IUnitService    unitService,
        IHrmsService    hrmsService,
        IOptions<HajjSettings> settings)
    {
        _pilgrimService = pilgrimService;
        _unitService    = unitService;
        _hrmsService    = hrmsService;
        _settings       = settings.Value;
    }

    // ── INDEX ─────────────────────────────────────────────────────────
    [PilgrimageFilter]
    public IActionResult Index() => View();

    // ── READ (DataTable) ──────────────────────────────────────────────
    public async Task<IActionResult> Read()
    {
        int year = _settings.ActiveHajjYear;
        var data = await _pilgrimService.Query()
            .Include(p => p.Unit)
            .Where(p => p.HajjYear    == year &&
                         p.EmployeeStatus == EmployeeStatus.Employee &&
                         p.TypeId != HajjConstants.PilgrimType.Admin)
            .Select(p => new {
                p.PilgrimId, p.FullName, p.ServiceNumber, p.NIC,
                p.RankDesc, p.Region,
                UnitNameAr = p.Unit != null ? p.Unit.UnitNameAr : "",
                p.TypeId, p.FitResult, p.ConfirmCode
            })
            .ToListAsync();
        return Json(data);
    }

    // ── HRMS LOOKUP ───────────────────────────────────────────────────
    public async Task<IActionResult> FetchFromHrms(string serviceNumber)
    {
        if (string.IsNullOrWhiteSpace(serviceNumber) ||
            !Regex.IsMatch(serviceNumber.Trim().ToUpper(), _settings.ServiceNumberPattern))
            return BadRequest("رقم الخدمة العسكرية غير صالح");

        var dto = await _hrmsService.GetEmployeeByServiceNumberAsync(serviceNumber.Trim().ToUpper());
        if (dto is null)
            return BadRequest("لم يتم العثور على الموظف في نظام الموارد البشرية");

        var unit = await _unitService.GetByCodeAsync(int.TryParse(dto.SERVICE, out var sc) ? sc : 0);
        var pilgrim = MapHrmsToEntity(dto, unit);
        return PartialView("_Create", pilgrim);
    }

    // ── BAN CHECK (AJAX) ──────────────────────────────────────────────
    public async Task<IActionResult> CheckBan(string nic)
    {
        var result = await _pilgrimService.CheckPermanentBanAsync(nic);
        return Json(result);
    }

    // ── QUOTA (MY UNITS ONLY) ─────────────────────────────────────────
    public async Task<IActionResult> MyQuota()
    {
        var quotas = await _unitService.GetMyQuotasAsync();
        return Json(quotas);
    }

    // ── CREATE ────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Pilgrim pilgrim)
    {
        if (!ModelState.IsValid)
            return BadRequest("البيانات غير مكتملة");

        var result = await _pilgrimService.RegisterFromHrmsAsync(pilgrim);
        return ServiceResult(result, new { message = "تمت الإضافة بنجاح", id = result.Value?.PilgrimId });
    }

    // ── BULK CREATE (Non-Mod external units) ──────────────────────────
    [HttpPost]
    public async Task<IActionResult> BulkCreate([FromBody] IEnumerable<Pilgrim> pilgrims)
    {
        var result = await _pilgrimService.BulkRegisterNonModAsync(pilgrims);
        return ServiceResult(result, new { message = "تم حفظ البيانات بنجاح" });
    }

    // ── SOFT DELETE ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Delete(int pilgrimId)
    {
        var result = await _pilgrimService.SoftDeleteAsync(pilgrimId);
        return ServiceResult(result);
    }

    // ── UPDATE ────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] Pilgrim pilgrim)
    {
        var result = await _pilgrimService.UpdateAsync(pilgrim);
        return ServiceResult(result);
    }

    // ── Stats (for quota widget on create screen) ─────────────────────
    public async Task<IActionResult> Stats()
    {
        int year = _settings.ActiveHajjYear;
        var unitQuotas = await _unitService.GetMyQuotasAsync();
        return PartialView("_Stats", unitQuotas);
    }

    // ── Helper: map HRMS DTO → Pilgrim entity ─────────────────────────
    private static Pilgrim MapHrmsToEntity(Application.DTOs.HrmsEmployeeDto dto, Domain.Entities.Unit? unit)
    {
        return new Pilgrim
        {
            FullName        = dto.NAME_ARABIC,
            ServiceNumber   = dto.SERVICE_NUMBER,
            EmployeeStatus  = dto.EMP_STAT == "1" ? EmployeeStatus.Pension : EmployeeStatus.Employee,
            NIC             = dto.NIC_NO.All(char.IsDigit) ? dto.NIC_NO : "لايوجد",
            NICExpire       = dto.NIC_EXP_DATE,
            Passport        = dto.PP_NO == "-" ? "لايوجد" : dto.PP_NO,
            PassportExpire  = dto.PP_EXP_DATE,
            DateOfBirth     = dto.DOB_T,
            RankCode        = int.TryParse(dto.RANK_CODE, out var rc) ? rc : 0,
            RankDesc        = dto.RANK_ARABIC,
            HrmsUnitCode    = int.TryParse(dto.UNIT, out var uc) ? uc : 0,
            HrmsUnitDesc    = dto.uniT_ARABIC,
            Unit            = unit,
            UnitId          = unit?.UnitId,
            Region          = dto.REGION_A,
            WilayaCode      = dto.WIL_CODE == "-" ? 0 : int.TryParse(dto.WIL_CODE, out var wc) ? wc : 0,
            WilayaDesc      = dto.WIL_ARABIC,
            VillageCode     = dto.VIL_CODE == "-" ? 0 : int.TryParse(dto.VIL_CODE, out var vc) ? vc : 0,
            VillageDesc     = dto.VIL_ARABIC,
            GSM             = dto.GSM,
            BloodGroup      = dto.Blood_A,
            DateOfEnlistment= dto.DOE_T,
        };
    }
}
