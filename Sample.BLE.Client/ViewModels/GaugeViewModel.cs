using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cygnus.BLE.API.Interfaces;
using Cygnus.Models;
using Cygnus.Interfaces;
using Cygnus.BLE.Protobuf.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Sample.BLE.Client.ViewModels
{
    public partial class GaugeViewModel : ObservableObject, IGaugeMonitor
    {
        private readonly ILogger<GaugeViewModel> _logger;
        private readonly IPopupService _popupService;
        private readonly IPlatformService _platformService;
        private readonly IMeasurementConverter _measurementConverter;

        public GaugeViewModel(
            ILogger<GaugeViewModel> logger,
            IPopupService popupService,
            IPlatformService platformService,
            IMeasurementConverter measurementConverter)
        {
            _logger = logger;
            _popupService = popupService;
            _platformService = platformService;
            _measurementConverter = measurementConverter;
            GetRecordCommand = new RelayCommand<RecordViewModel>(r => GetRecord(r, false));
            GetRecordWithAScansCommand = new RelayCommand<RecordViewModel>(r => GetRecord(r, true));
            CancelRecordTransferCommand = new AsyncRelayCommand(CanceRecordlTransfer);
            DeleteRecordCommand = new RelayCommand<RecordViewModel>(DeleteRecord);
            DeleteAllRecordsCommand = new RelayCommand(DeleteAllRecords);
            NewRecordCommand = new AsyncRelayCommand(NewRecord);
        }

        [ObservableProperty]
        public partial LiveMeasurementViewModel LiveMeasurement { get; set; }

        public ICommand NewRecordCommand { get; private set; }

        private async Task NewRecord()
        {
            Page? page = App.Current?.Windows[0].Page;
            if (page == null)
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

        [ObservableProperty]
        public required partial IBLEGauge Gauge { get; set; }

        [ObservableProperty]
        public partial bool IsConnected { get; set; }

        [ObservableProperty]
        public partial bool IsSubscribedToLive { get; set; }

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;
        
        [ObservableProperty]
        public partial string Model { get; set; } = string.Empty;
    
        [ObservableProperty]
        public partial string SerialNumber { get; set; } = string.Empty;

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
                if (record?.Name != null)
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
                                Thickness = _measurementConverter.GetDisplayedThickness(measurement.Thickness, measurement.Units),
                                Velocity = _measurementConverter.GetDisplayedVelocity(measurement.Velocity, measurement.Units),
                                Timestamp = measurement.Timestamp
                            });
                        }
                    }
                }
            });
        }

        public ICommand CancelRecordTransferCommand { get; private set; }

        private Task CanceRecordlTransfer()
        {
            return Gauge.CancelRecordTransfer();
        }

        public ICommand DeleteRecordCommand { get; private set; }

        private void DeleteRecord(RecordViewModel? record)
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (record != null && !string.IsNullOrWhiteSpace(record.Name))
                    {
                        await Gauge.DeleteRecord(record);
                        await DoUpdate();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Problem deleting record {Name}", record?.Name);
                    await _platformService.ShowMessage($"There was a problem deleting the record {record?.Name} - {ex.Message}");
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
                    await Gauge.DeleteAllRecords();
                    await DoUpdate();
                }
                catch(Exception ex) 
                {
                    _logger.LogError(ex, "Problem deleting all records");
                    await _platformService.ShowMessage($"There was a problem deleting all records - {ex.Message}");
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
            IsConnected = Gauge.IsConnected;
            var recordList = await Gauge.GetRecordList();
            if (recordList != null)
            {
                RecordList = recordList.Select(r => new RecordViewModel()
                {
                    Key = r.Key,
                    Name = r.Name,
                    RecordType = r.RecordType,
                    MeasurementCount = r.NumberOfPointsRequired,
                    MeasurementsTaken = r.NumberOfPointsTaken,
                    Created = r.Created,
                    Updated = r.Updated
                });
            }
        }

        internal void Clear()
        {
            IsConnected = Gauge.IsConnected;
            RecordList = [];
        }

        partial void OnRecordListChanged(IEnumerable<RecordViewModel> value)
        {
            Measurements.Clear();
        }

        partial void OnGaugeChanged(IBLEGauge value)
        {
            Name = value.Name;
            Model = value.Model;
            SerialNumber = value.SerialNumber;
            FirmwareVersion = value.FirmwareVersion;
            value.AddObserver(this);
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
                Gauge.SubscribeToLiveUpdates();
            }
            else
            {
                Gauge.UnsubscribeFromLiveUpdates();
            }   
        }
        
        public void UpdateLiveMeasurement(LiveMeasurement liveMeasurement)
        {
            LiveMeasurement = new LiveMeasurementViewModel
            {
                BatteryLevel = liveMeasurement.BatteryLevel,
                GaindB = liveMeasurement.GaindB,
                Index = liveMeasurement.Index,
                IsDeepcoat = liveMeasurement.IsDeepcoat,
                IsFrozen = liveMeasurement.IsFrozen,
                IsStable = liveMeasurement.IsStable,
                IsValid = liveMeasurement.IsValid,
                Mode = liveMeasurement.Mode,
                SurfaceTemp = liveMeasurement.SurfaceTemp,
                Units = liveMeasurement.Units,
                Thickness = _measurementConverter.GetDisplayedThickness(liveMeasurement.Thickness, liveMeasurement.Units),
                Velocity = _measurementConverter.GetDisplayedVelocity(liveMeasurement.Velocity, liveMeasurement.Units),
                HasAScan = liveMeasurement.AScan?.AScanPoints?.Length > 0,
            };
        }
    }
}

