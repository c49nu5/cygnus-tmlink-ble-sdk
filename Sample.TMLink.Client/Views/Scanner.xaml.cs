using Sample.TMLink.Client.ViewModels;
namespace Sample.TMLink.Client.Views;

public partial class Scanner : ContentPage
{
    public Scanner()
    {
        InitializeComponent();
        BindingContext = App.Services?.GetRequiredService<ScannerViewModel>();
    }

    private void GaugeList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
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