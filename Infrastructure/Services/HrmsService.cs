using HajjSystem.Application.DTOs;
using HajjSystem.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace HajjSystem.Infrastructure.Services;

public class HrmsService : IHrmsService
{
    private readonly HttpClient    _http;
    private readonly IConfiguration _cfg;

    public HrmsService(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg  = cfg;
    }

    public async Task<HrmsEmployeeDto?> GetEmployeeByServiceNumberAsync(string serviceNumber)
    {
        var baseUrl  = _cfg["HajjSettings:HrmsBaseUrl"]          ?? string.Empty;
        var endpoint = _cfg["HajjSettings:HrmsEmployeeEndpoint"] ?? string.Empty;
        var url      = $"{baseUrl}{endpoint}{serviceNumber}";

        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<HrmsEmployeeDto>(json);
    }
}
