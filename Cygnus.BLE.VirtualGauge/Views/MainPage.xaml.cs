using Cygnus.BLE.VirtualGauge.ViewModels;

namespace Cygnus.BLE.VirtualGauge.Views;


public partial class MainPage : ContentPage
{
    public MainPage()
    {
        this.InitializeComponent();
        BindingContext = App.Services?.GetService<MainViewModel>();
    }
}
