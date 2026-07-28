using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cygnus.Interfaces;
using Cygnus.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Sample.TMLink.Client.ViewModels
{
    public partial class ScannerViewModel : ObservableObject, IConnectionObserver
    {
        private readonly ILogger<ScannerViewModel> _logger;
        private readonly IConnectionService _connectionService;
        private readonly IMeasurementDisplaySettingsService _measurementSettingsService;
        private readonly Func<IConnectionInformation, GaugeViewModel> _gaugeViewFactory;

        public ScannerViewModel(
            ILogger<ScannerViewModel> logger,
            IConnectionService connectionService,
            IMeasurementDisplaySettingsService measurementSettingsService,
            Func<IConnectionInformation,GaugeViewModel> gaugeViewFactory)
        {
            ConnectCommand = new RelayCommand<GaugeViewModel?>(Connect);
            ToggleScanning = new RelayCommand(ToggleScanForGauges);
            SetUnitsCommand = new RelayCommand<MeasurementUnits?>(SetUnits, CanSetUnits);
            SetResolutionCommand = new RelayCommand<MeasurementResolution?>(SetResolution, CanSetResolution);
            _logger = logger;
            _connectionService = connectionService;
            _measurementSettingsService = measurementSettingsService;
            _gaugeViewFactory = gaugeViewFactory;
            _connectionService.AddObserver(this);
        }

        [ObservableProperty]
        public partial ObservableCollection<GaugeViewModel> Gauges { get; set; } = [];

        [ObservableProperty]
        public partial GaugeViewModel? SelectedGauge { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Waiting))]
        [NotifyPropertyChangedFor(nameof(ScanState))]
        [NotifyPropertyChangedFor(nameof(ToggleScanningCmdLabelText))]
        public partial ConnectionState ConnectionState { get; set; }

        public bool Waiting => ConnectionState == ConnectionState.Connecting;
        public string ScanState => ConnectionState == ConnectionState.Connecting ? "Scanning" : "Waiting";
        public string ToggleScanningCmdLabelText => ConnectionState == ConnectionState.Connecting ? "Cancel" : "Start Scan";

        private void DebugMessage(string message)
        {
            Debug.WriteLine(message);
            _logger.LogInformation(message);
        }

        #region Scan & Discover
        public ICommand ToggleScanning { get; init; }

        private void ToggleScanForGauges()
        {
            if (ConnectionState != ConnectionState.Connecting)
            {
                DebugMessage($"Starting Scan");
                Dispatcher.GetForCurrentThread()?.DispatchAsync(async () =>
                {
                    await _connectionService.DiscoverGauges();
                    DebugMessage($"Completed Scan");
                });
            }
            else
            {
                DebugMessage($"Canceling Scanning");
                _connectionService.CancelDiscover();
                ConnectionState = ConnectionState.Disconnected;
            }
        }

        public ICommand ConnectCommand { get; init; }

        private void Connect(GaugeViewModel? value)
        {
            if (value?.Connection != null)
            {
                _connectionService.ConnectToGauge(value.Connection);
                OnPropertyChanged(nameof(Gauges));
            }
        }

        public RelayCommand<MeasurementUnits?> SetUnitsCommand { get; init; }

        private void SetUnits(MeasurementUnits? units)
        {           
            _measurementSettingsService.Units = units ?? MeasurementUnits.Default;
            SetUnitsCommand.NotifyCanExecuteChanged();
        }

        private bool CanSetUnits(MeasurementUnits? units)
        {
            return _measurementSettingsService.Units != units;
        }

        public RelayCommand<MeasurementResolution?> SetResolutionCommand { get; init; }

        private void SetResolution(MeasurementResolution? Resolution)
        {
            _measurementSettingsService.Resolution = Resolution ?? MeasurementResolution.Default;
            SetResolutionCommand.NotifyCanExecuteChanged();
        }

        private bool CanSetResolution(MeasurementResolution? Resolution)
        {
            return _measurementSettingsService.Resolution != Resolution;
        }

        public void GaugeConnected(IGauge? gauge)
        {
            var gaugeViewModel = Gauges.FirstOrDefault(g => g.SerialNumber == gauge?.SerialNumber);
            gaugeViewModel?.Gauge = gauge;
            SelectedGauge = gaugeViewModel;
        }

        partial void OnSelectedGaugeChanged(GaugeViewModel? oldValue, GaugeViewModel? newValue)
        {
            oldValue?.Clear();
            newValue?.RefreshRecordList();
        }

        public void GaugeDiscovered(IConnectionInformation connection)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                GaugeViewModel? gaugeViewModel = Gauges.FirstOrDefault(g => g.SerialNumber == connection.SerialNumber);
                if (gaugeViewModel != null)
                {
                    Gauges.Remove(gaugeViewModel);
                }

                gaugeViewModel = _gaugeViewFactory(connection);
                Gauges.Add(gaugeViewModel);
            });
        }

        public void AddConnectionMessage(string message)
        {
            _logger.LogInformation(message);
        }
        #endregion Scan & Discover
    }
}
