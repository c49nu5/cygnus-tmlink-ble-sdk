using Cygnus.TMLink.Protobuf.Interfaces;
using Cygnus.TMLink.Protobuf.Services;
using Cygnus.TMLink.BLE.VirtualGauge.ViewModels;
using Microsoft.Extensions.Logging;
using Shiny;

namespace Cygnus.TMLink.BLE.VirtualGauge;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseShiny() // THIS IS REQUIRED FOR SHINY ON MAUI
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
        builder.Services.AddBluetoothLeHosting();
        builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<IProtobufMessageConverter, ProtobufMessageConverter>();
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
	}

    public static bool IsAndroid => DeviceInfo.Current.Platform == DevicePlatform.Android;

    public static bool IsMacCatalyst => DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst;

    public static bool IsMacOS => DeviceInfo.Current.Platform == DevicePlatform.macOS;
}
