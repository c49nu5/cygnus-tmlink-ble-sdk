using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cygnus.Interfaces;
using Cygnus.Models;
using Cygnus.TMLink.API.Maui;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Sample.TMLink.Client.ViewModels
{
    public partial class GaugeViewModel : ObservableObject, IGaugeObserver, ILiveMeasurementObserver
    {
        private readonly ILogger<GaugeViewModel> _logger;
        private readonly IPopupService _popupService;
        private readonly IUserDialogService _userDialogService;
        private readonly IMeasurementConverter _measurementConverter;

        public GaugeViewModel(
            ILogger<GaugeViewModel> logger,
            IPopupService popupService,
            IUserDialogService userDialogService,
            IMeasurementConverter measurementConverter,
            IConnectionInformation connectionInformation)
        {
            _logger = logger;
            _popupService = popupService;
            _userDialogService = userDialogService;
            _measurementConverter = measurementConverter;
            Connection = connectionInformation;
            GetRecordCommand = new RelayCommand<RecordViewModel>(r => GetRecord(r, false));
            GetRecordWithAScansCommand = new RelayCommand<RecordViewModel>(r => GetRecord(r, true));
            CancelRecordTransferCommand = new AsyncRelayCommand(CanceRecordlTransfer);
            DeleteRecordCommand = new RelayCommand<RecordViewModel>(DeleteRecord);
            DeleteAllRecordsCommand = new RelayCommand(DeleteAllRecords);
            NewRecordCommand = new AsyncRelayCommand(NewRecord);
        }

        [ObservableProperty]
        public partial LiveMeasurementViewModel LiveMeasurement { get; set; } = new LiveMeasurementViewModel();

        [ObservableProperty]
        public partial uint BatteryLevel { get; set; }

        public ICommand NewRecordCommand { get; private set; }

        private async Task NewRecord()
        {
            Page? page = App.Current?.Windows[0].Page;
            if (page == null || Gauge == null)
            {
                return;
            }

            var popupResult = await _popupService.ShowPopupAsync<NewRecordViewModel>(page) as IPopupResult<NewRecordViewModel>;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (popupResult?.Result is NewRecordViewModel newRecord && newRecord != null)
                {
                    BlankRecord blankRecord = new()
                    {
                        Name = newRecord.Name,
                        Type = newRecord.Type,
                        Units = MeasurementUnits.Metric,
                        Key = ((int)DateTime.Now.TimeOfDay.TotalSeconds).ToString(),
                        ColumnCount = newRecord.ColumnCount,
                        RowCount = newRecord.RowCount,
                        MeasurementPoints = newRecord.Measurements.Select(point =>
                            new BlankPoint
                            {
                                Key = point.Key,
                                Name = point.Name,
                                ColNumX = point.ColNumX,
                                RowNumY = point.RowNumY,
                                Method = point.Method,
                                ThicknessMinLimit = point.ThicknessMinLimit,
                                ThicknessMaxLimit = point.ThicknessMaxLimit
                            }).ToArray()
                    };

                    await Gauge.NewRecord(blankRecord);

                    await DoUpdate();
                }
            });
        }

        internal IConnectionInformation Connection
        {
            get => field;
            set
            {
                field = value;
                Gauge = value as IGauge; // NB for TM-Link, the connection information is also the gauge itself
            }
        }

        internal IGauge? Gauge
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    if (value != null)
                    {
                        Name = value.Name;
                        Model = value.Model;
                        SerialNumber = value.SerialNumber;
                        FirmwareVersion = value.FirmwareVersion;
                        value.AddObserver(this);
                    }
                }
            }
        }

        [ObservableProperty]
        public partial bool IsConnected { get; set; }

        [ObservableProperty]
        public partial bool IsSubscribedToLive { get; set; }

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;
        
        [ObservableProperty]
        public partial string Model { get; set; } = string.Empty;
    
        [ObservableProperty]
        public partial uint? SerialNumber { get; set; } = null;

        [ObservableProperty]
        public partial Version? FirmwareVersion { get; set; }

        [ObservableProperty]
        public partial IEnumerable<RecordViewModel> RecordList { get; private set; } = [];

        public ObservableCollection<MeasurementViewModel> Measurements { get; private set; } = new ObservableCollection<MeasurementViewModel>();

        public ICommand GetRecordCommand { get; private set; }

        public ICommand GetRecordWithAScansCommand { get; private set; }

        private void GetRecord(RecordViewModel? record, bool withAScans)
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (record?.Name != null && Gauge != null)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var gaugeRecord = await Gauge.GetRecord(record, withAScans);
                    stopwatch.Stop();
                    _logger.LogInformation("GetRecord for {Name} with a-scans ({WithA-Scans}) took {ElapsedMilliseconds} ms", record.Name, withAScans, stopwatch.ElapsedMilliseconds);
                    Measurements.Clear();
                    if (gaugeRecord != null)
                    {
                        foreach (var measurement in gaugeRecord.Measurements)
                        {
                            Measurements.Add(new()
                            {
                                Name = measurement.Name,
                                Thickness = _measurementConverter.GetDisplayedThickness(measurement.Thickness ?? 0, measurement.Units),
                                Velocity = _measurementConverter.GetDisplayedVelocity(measurement.Velocity, measurement.Units),
                                Timestamp = measurement.Time
                            });
                        }
                    }
                }
            });
        }

        public ICommand CancelRecordTransferCommand { get; private set; }

        private Task CanceRecordlTransfer()
        {
            return Gauge?.CancelRecordTransfer() ?? Task.CompletedTask;
        }

        public ICommand DeleteRecordCommand { get; private set; }

        private void DeleteRecord(RecordViewModel? record)
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (record != null && !string.IsNullOrWhiteSpace(record.Name) && Gauge != null)
                    {
                        await Gauge.DeleteRecord(record);
                        await DoUpdate();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Problem deleting record {Name}", record?.Name);
                    await _userDialogService.ShowMessage($"There was a problem deleting the record {record?.Name} - {ex.Message}");
                }
            });
        }

        public ICommand DeleteAllRecordsCommand { get; private set; }

        private void DeleteAllRecords()
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (Gauge == null)
                    {
                        return;
                    }

                    await Gauge.DeleteAllRecords();
                    await DoUpdate();
                }
                catch(Exception ex) 
                {
                    _logger.LogError(ex, "Problem deleting all records");
                    await _userDialogService.ShowMessage($"There was a problem deleting all records - {ex.Message}");
                }
            });
        }

        public override string ToString()
        {
            return $"{Name}:{SerialNumber}";
        }

        internal void RefreshRecordList()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DoUpdate().ConfigureAwait(false);
            });
        }

        private async Task DoUpdate()
        {
            IsConnected = Gauge?.IsConnected ?? false;
            if (Gauge != null)
            {
                var recordList = await Gauge.GetRecordList();
                if (recordList != null)
                {
                    RecordList = recordList.Select(r => new RecordViewModel()
                    {
                        Key = r.Key,
                        Name = r.RecordName,
                        RecordType = r.RecordType,
                        MeasurementCount = r.NumberOfPointsRequired,
                        MeasurementsTaken = r.PointCount,
                        Created = r.Created,
                        Updated = r.Updated
                    });
                }
            }
        }

        internal void Clear()
        {
            IsConnected = Gauge?.IsConnected ?? false;
            RecordList = [];
        }

        partial void OnRecordListChanged(IEnumerable<RecordViewModel> value)
        {
            Measurements.Clear();
        }

        partial void OnIsConnectedChanged(bool value)
        {
            if (!value)
            {
                IsSubscribedToLive = false;
            }
        }

        partial void OnIsSubscribedToLiveChanged(bool value)
        {
            if (value)
            {
                Gauge?.SubscribeToLiveUpdates(this);
            }
            else
            {
                Gauge?.UnsubscribeFromLiveUpdates(this);
            }   
        }

        public void OnLiveMeasurementReceived(LiveMeasurement liveMeasurement)
        {
            LiveMeasurement = new LiveMeasurementViewModel
            {
                GaindB = liveMeasurement.GaindB,
                Index = liveMeasurement.PointIndex,
                IsDeepcoat = liveMeasurement.DeepCoatOn,
                IsFrozen = liveMeasurement.IsFrozen,
                IsStable = liveMeasurement.StableMeasurement,
                IsValid = liveMeasurement.ValidMeasurement,
                Mode = liveMeasurement.Mode,
                SurfaceTemp = liveMeasurement.SurfaceTemperatureCelsius,
                Units = liveMeasurement.Units,
                Thickness = _measurementConverter.GetDisplayedThickness(liveMeasurement.Thickness ?? 0, liveMeasurement.Units),
                Velocity = _measurementConverter.GetDisplayedVelocity(liveMeasurement.Velocity, liveMeasurement.Units),
                HasAScan = liveMeasurement.AScan.Amplitudes?.Length > 0,
            };
        }

        public void OnPropertiesUpdated(IGauge gauge)
        {
            BatteryLevel = gauge.BatteryLevel;
        }
    }
}
