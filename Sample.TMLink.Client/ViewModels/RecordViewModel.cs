using CommunityToolkit.Mvvm.ComponentModel;
using Cygnus.Interfaces;
using Cygnus.Models;

namespace Sample.TMLink.Client.ViewModels
{
    public partial class RecordViewModel : ObservableObject, IFileTransferRequest
    {
        public string Key { get; internal set; } = string.Empty;
        public string Name { get; internal set; } = string.Empty;
        public RecordType RecordType { get; internal set; }
        public uint MeasurementCount { get; internal set; }
        public uint MeasurementsTaken { get; internal set; }
        public DateTime? Created { get; internal set; }
        public DateTime? Updated { get; internal set; }

        [ObservableProperty]
        public partial double PercentageTransferred { get; set; }

        [ObservableProperty]
        public partial FileTransferState Status { get; set; }

    }
}

