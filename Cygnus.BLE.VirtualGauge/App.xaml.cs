namespace Cygnus.BLE.VirtualGauge;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public App(IServiceProvider provider)
	{
		InitializeComponent();
        
        Services = provider;
	}

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Current?.UserAppTheme = AppTheme.Light;
        Window window = new(new AppShell())
        {
            Title = "Cygnus BLE Gauges"
        };
        return window;
    }
}
