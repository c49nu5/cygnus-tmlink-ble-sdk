namespace Cygnus.TMLink.BLE.VirtualGauge;

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
            Title = "Cygnus TM-Link Virtual Gauge"
        };
        return window;
    }
}
