using Cygnus.BLE.API.Interfaces;
using Cygnus.Interfaces;

namespace Cygnus.BLE.API.Maui;
public static class ServiceCollectionsExtensions
{
    public static void AddBleMauiServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService, BlePlatformService>();
        services.AddSingleton<IMeasurementDisplaySettingsService, MeasurementDisplaySettingsService>();
        services.AddBleServices();
    }
}
