using Cygnus.Models;

namespace Sample.TMLink.Client.ViewModels
{
    public class LiveMeasurementViewModel
    {
        public string? Thickness { get; set; }

        public string? Velocity { get; set; }

        public MeasurementUnits Units { get; set; }

        public MeasureMode Mode { get; set; }

        public float GaindB { get; set; }

        public uint Index { get; set; }

        public int SurfaceTemp { get; set; }

        public bool IsDeepcoat { get; set; }

        public bool IsFrozen { get; set; }

        public bool IsStable { get; set; }

        public bool IsValid { get; set; }

        public bool HasAScan { get; set; }
    }
}