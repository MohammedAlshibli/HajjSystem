using HajjSystem.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HajjSystem.Infrastructure.Services;

/// <summary>
/// Reads/writes ActiveHajjYear in appsettings.json at runtime.
/// Implements IHajjSettingsAccessor from the Application layer.
/// </summary>
public class HajjSettingsAccessor : IHajjSettingsAccessor
{
    private readonly IConfiguration _cfg;
    private readonly string         _settingsPath;
    private int _year;

    public HajjSettingsAccessor(IConfiguration cfg, IWebHostEnvironmentAccessor env)
    {
        _cfg          = cfg;
        _settingsPath = Path.Combine(env.ContentRootPath, "appsettings.json");
        _year         = int.TryParse(_cfg["HajjSettings:ActiveHajjYear"], out var y) ? y : DateTime.Now.Year;
    }

    public int ActiveHajjYear => _year;

    public void UpdateActiveYear(int newYear)
    {
        _year = newYear;

        if (!File.Exists(_settingsPath)) return;
        var text    = File.ReadAllText(_settingsPath);
        var updated = System.Text.RegularExpressions.Regex.Replace(
            text,
            @"(""ActiveHajjYear""\s*:\s*)\d+",
            $"${{1}}{newYear}");
        File.WriteAllText(_settingsPath, updated);
    }
}

/// <summary>Wraps IWebHostEnvironment to avoid Infrastructure depending on Microsoft.AspNetCore.</summary>
public interface IWebHostEnvironmentAccessor
{
    string ContentRootPath { get; }
}
