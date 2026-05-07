using Sample.BLE.Client.ViewModels;
using Sample.BLE.Client.Views;
using CommunityToolkit.Maui;
using Cygnus.BLE.API.Maui;
using Cygnus.Services;
using Microsoft.Extensions.Logging;

namespace Sample.BLE.Client;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder()
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddTransient<BLEScannerViewModel>();
		builder.Services.AddTransient<GaugeViewModel>();
		builder.Services.AddSingleton<Func<GaugeViewModel>>(s => s.GetRequiredService<GaugeViewModel>);
        builder.Services.AddTransientPopup<NewRecordView, NewRecordViewModel>();
        builder.Services.AddBleMauiServices();
		builder.Services.AddCygnusServices();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
	}

    public static bool IsAndroid => DeviceInfo.Current.Platform == DevicePlatform.Android;

    public static bool IsMacCatalyst => DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst;

    public static bool IsMacOS => DeviceInfo.Current.Platform == DevicePlatform.macOS;
}
