using HajjSystem.Application.Services.Implementations;
using HajjSystem.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HajjSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPilgrimService,            PilgrimService>();
        services.AddScoped<IUnitService,               UnitService>();
        services.AddScoped<IConfirmationService,       ConfirmationService>();
        services.AddScoped<IMedicalService,            MedicalService>();
        services.AddScoped<IFlightDistributionService, FlightDistributionService>();
        services.AddScoped<ISeasonService,             SeasonService>();
        services.AddScoped<IUserService,               UserService>();
        return services;
    }
}
