using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cygnus.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.TMLink.Client.ViewModels
{
    public partial class NewRecordViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;

        public NewRecordViewModel(IPopupService popupService)
        {
            _popupService = popupService;
            OKCommand = new RelayCommand(OK);
            CancelCommand = new RelayCommand(Cancel);
            SelectedType = RecordTypes.FirstOrDefault();
        }

        public ICommand OKCommand { get; init; }

        private void OK()
        {
            Page? page = App.Current?.Windows[0].Page;
            if (page == null)
            {
                return;
            }

            Measurements.Clear();
            if (Type == RecordType.Linear)
            {
                GenerateLinearMeasurements();
            }
            else
            {
                GenerateGridMeasurements();
            }

            _popupService.ClosePopupAsync(page, this);
        }

        public ICommand CancelCommand { get; init; }

        private void Cancel()
        {
            Page? page = App.Current?.Windows[0].Page;
            if (page == null)
            {
                return;
            }

            _popupService.ClosePopupAsync(page, (NewRecordViewModel?)null);
        }

        public string Name { get => $"{(Type == RecordType.Linear ? "L" : "G")}R_{DateTime.Now:ddMMMyy}_{DateTime.Now:HHmm}"; }

        public ObservableCollection<NewMeasurementViewModel> Measurements { get; set; } = [];

        public RecordType Type => SelectedType == RecordType.Linear.ToString() ? RecordType.Linear : RecordType.Grid2D;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Name))]
        public partial string? SelectedType { get; set; }

        public IList<string> RecordTypes { get; } = [ RecordType.Linear.ToString(), RecordType.Grid2D.ToString() ];

        public uint ColumnCount { get; set; } = 3;
        
        public uint RowCount { get; set; } = 5;

        private void GenerateLinearMeasurements()
        {
            for (uint i = 0; i < ColumnCount; i++)
            {
                Measurements.Add(new NewMeasurementViewModel()
                {
                    Key = 44352811 + i,
                    Name = $"P{i + 1}",
                    Method = Method.Spot,
                    ThicknessMinLimit = 4000,
                    ThicknessMaxLimit = 24000,
                });
            }

            RowCount = 0;
        }

        private void GenerateGridMeasurements()
        {
            for (uint r = 0; r < RowCount; r++)
            {
                for (uint c= 0; c < ColumnCount; c++)
                {
                    Measurements.Add(new NewMeasurementViewModel()
                    {
                        Key = 44362811 + c * 100 + r,
                        Name = $"C{c+1}.R{r+1}",
                        ColNumX = c,
                        RowNumY = r,
                        Method = Method.Spot,
                        ThicknessMinLimit = 5000,
                        ThicknessMaxLimit = 25000,
                    });
                }
            }
        }

        partial void OnSelectedTypeChanged(string? oldValue, string? newValue)
        {
            if (newValue == null)
            {
                SelectedType = oldValue;
            }
        }
    }
}
