using Sample.TMLink.Client.ViewModels;

namespace Sample.TMLink.Client.Views;

public partial class GaugeView : ContentPage
{
	public GaugeView()
	{
		InitializeComponent();
	}

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        if (args.NavigationType != NavigationType.Push &&
            BindingContext is GaugeViewModel gaugeViewModel)
        {
            gaugeViewModel.Gauge?.Disconnect();
        }

        base.OnNavigatedFrom(args);
    }
}