using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HajjSystem.Web.Controllers;

public class SeasonRolloverController : BaseController
{
    private readonly ISeasonService _seasonSvc;
    private readonly HajjSettings   _settings;

    public SeasonRolloverController(ISeasonService seasonSvc, IOptions<HajjSettings> settings)
    {
        _seasonSvc = seasonSvc;
        _settings  = settings.Value;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> Stats(int? year)
    {
        int y = year ?? _settings.ActiveHajjYear;
        return Json(await _seasonSvc.GetStatsAsync(y));
    }

    public async Task<IActionResult> History() =>
        Json(await _seasonSvc.GetHistoryAsync());

    [HttpPost]
    public async Task<IActionResult> Rollover(int newYear)
    {
        var r = await _seasonSvc.PerformRolloverAsync(newYear);
        return ServiceResult(r, new { message = r.Value?.Message, newYear = r.Value?.NewYear });
    }
}
