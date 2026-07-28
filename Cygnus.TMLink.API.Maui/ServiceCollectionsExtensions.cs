using Cygnus.Interfaces;
using Cygnus.TMLink.API.Interfaces;

namespace Cygnus.TMLink.API.Maui;
public static class ServiceCollectionsExtensions
{
    public static void AddTMLinkMauiServices(this IServiceCollection services)
    {
        services.AddSingleton<TMLinkPlatformService>();
        services.AddSingleton<IPlatformService>(s => s.GetRequiredService<TMLinkPlatformService>());
        services.AddSingleton<IUserDialogService>(s => s.GetRequiredService<TMLinkPlatformService>());
        services.AddSingleton<IMeasurementDisplaySettingsService, MeasurementDisplaySettingsService>();
        services.AddTMLinkAPIServices();
    }
}
