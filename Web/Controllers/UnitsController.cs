using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Domain.Entities;
using HajjSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HajjSystem.Web.Controllers;

public class UnitsController : BaseController
{
    private readonly IUnitService _unitSvc;
    private readonly IUnitOfWork  _uow;
    private readonly HajjSettings _settings;

    public UnitsController(IUnitService unitSvc, IUnitOfWork uow, IOptions<HajjSettings> settings)
    {
        _unitSvc  = unitSvc;
        _uow      = uow;
        _settings = settings.Value;
    }

    [PilgrimageFilter]
    public IActionResult Index() => View();

    public async Task<IActionResult> Read()       => Json(await _unitSvc.GetAllQuotasAsync());
    public async Task<IActionResult> MyQuota()    => Json(await _unitSvc.GetMyQuotasAsync());
    public async Task<IActionResult> Quota(int id)=> Json(await _unitSvc.GetQuotaByUnitAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Unit unit)
    {
        StampNew(unit);
        _unitSvc.Add(unit);
        await _uow.SaveChangesAsync();
        return Ok(new { message = "تمت الإضافة", unitId = unit.UnitId });
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] Unit unit)
    {
        StampUpdate(unit);
        _unitSvc.Update(unit);
        await _uow.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int unitId)
    {
        var u = await _unitSvc.GetByIdAsync(unitId);
        if (u is null) return NotFound();
        _unitSvc.Delete(u);
        await _uow.SaveChangesAsync();
        return Ok();
    }
}
