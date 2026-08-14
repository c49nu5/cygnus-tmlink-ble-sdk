using Sample.TMLink.Client.ViewModels;
using Sample.TMLink.Client.Views;
using CommunityToolkit.Maui;
using Cygnus.TMLink.API.Maui;
using Cygnus.Services;
using Microsoft.Extensions.Logging;
using Cygnus.Interfaces;
using System.Globalization;

namespace Sample.TMLink.Client;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		CultureInfo.CurrentUICulture = CultureInfo.InstalledUICulture;
		var builder = MauiApp.CreateBuilder()
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddTransient<ScannerViewModel>();
		builder.Services.AddTransient<GaugeViewModel>();
		builder.Services.AddSingleton<Func<IConnectionInformation, GaugeViewModel>>(s => c => ActivatorUtilities.CreateInstance<GaugeViewModel>(s, c));
        builder.Services.AddTransientPopup<NewRecordView, NewRecordViewModel>();

        builder.Services.AddTMLinkMauiServices();
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
