using Cygnus.TMLink.BLE.VirtualGauge.ViewModels;

namespace Cygnus.TMLink.BLE.VirtualGauge.Views;


public partial class MainPage : ContentPage
{
    public MainPage()
    {
        this.InitializeComponent();
        BindingContext = App.Services?.GetService<MainViewModel>();
    }
}
