using CommunityToolkit.Mvvm.ComponentModel;

namespace Sample.BLE.Client.ViewModels
{
    public partial class MeasurementViewModel : ObservableObject
    {
        public string Name { get; internal set; } = string.Empty;
        public string? Thickness { get; internal set; }
        public string? Velocity { get; internal set; }
        public DateTime? Timestamp { get; internal set; }
    }
}

