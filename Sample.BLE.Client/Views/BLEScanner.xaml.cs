using Sample.BLE.Client.ViewModels;
namespace Sample.BLE.Client.Views;

public partial class BLEScanner : ContentPage
{
    public BLEScanner()
    {
        InitializeComponent();
        BindingContext = App.Services?.GetRequiredService<BLEScannerViewModel>();
    }

    private void GaugeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() != null)
        {
            Navigation.PushAsync(new GaugeView { BindingContext = e.CurrentSelection.FirstOrDefault() });
        }
        else
        {
            Navigation.PopAsync();
        }
    }
}