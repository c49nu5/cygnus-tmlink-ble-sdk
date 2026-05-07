using Cygnus.Models;

namespace Sample.BLE.Client.ViewModels
{
    public class LiveMeasurementViewModel
    {
        public string? Thickness { get; set; }

        public string? Velocity { get; set; }

        public MeasurementUnits Units { get; set; }

        public UTMode Mode { get; set; }

        public uint BatteryLevel { get; set; }

        public uint GaindB { get; set; }

        public uint Index { get; set; }

        public uint SurfaceTemp { get; set; }

        public bool IsDeepcoat { get; set; }

        public bool IsFrozen { get; set; }

        public string? Stability { get; set; }

        public bool IsValid { get; set; }

        public bool HasAScan { get; set; }
    }
}