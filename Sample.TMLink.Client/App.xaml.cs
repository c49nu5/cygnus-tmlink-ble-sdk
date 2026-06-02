namespace Sample.TMLink.Client;

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
        Window window = new(new NavigationPage(new Views.Scanner()))
        {
            Title = "Cygnus TM-Link Gauges"
        };
        return window;
    }
}
