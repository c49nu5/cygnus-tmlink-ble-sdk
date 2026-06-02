using Cygnus.Models;

namespace Sample.TMLink.Client.ViewModels
{
    public class NewMeasurementViewModel
    {
        public uint Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public uint ColNumX { get; set; }
        public uint RowNumY { get; set; }
        public Method Method { get; set; } = Method.Spot;
        public uint ThicknessMinLimit { get; set; }
        public uint ThicknessMaxLimit { get; set; }
    }
}