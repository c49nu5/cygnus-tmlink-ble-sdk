using Sample.TMLink.Client.ViewModels;
namespace Sample.TMLink.Client.Views;

public partial class Scanner : ContentPage
{
    public Scanner()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = App.Services?.GetRequiredService<ScannerViewModel>();
        if (BindingContext is ScannerViewModel scannerViewModel)
        {
            scannerViewModel.SelectedGauge?.Disconnect();
        }
    }

    private void GaugeList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        object? selectedGaugeView = e.CurrentSelection?.ElementAtOrDefault(0);
        if (selectedGaugeView != null)
        {
            Navigation.PushAsync(new GaugeView { BindingContext = selectedGaugeView });
        }
        else
        {
            Navigation.PopToRootAsync();
        }
    }
}