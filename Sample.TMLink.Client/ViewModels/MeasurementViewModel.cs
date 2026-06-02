using CommunityToolkit.Mvvm.ComponentModel;

namespace Sample.TMLink.Client.ViewModels
{
    public partial class MeasurementViewModel : ObservableObject
    {
        public string Name { get; internal set; } = string.Empty;
        public string? Thickness { get; internal set; }
        public string? Velocity { get; internal set; }
        public DateTimeOffset? Timestamp { get; internal set; }
    }
}

