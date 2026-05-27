using CommunityToolkit.Mvvm.ComponentModel;
using Cygnus.BLE.Protobuf.Interfaces;
using Cygnus.BLE.VirtualGauge.Models;
using Shiny.BluetoothLE.Hosting;
using System.Data;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using static Cygnus.BLE.Interfaces.Constants;

namespace Cygnus.BLE.VirtualGauge.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly GaugeDataModel _gaugeData = new();
    private readonly IBleHostingManager _hostingManager;

    private byte[]? readCharacteristicValue;
    private Protobuf.V1.CommandType? _commandType;

    private IGattCharacteristic? _notifyReadyCharacteristic;

    private IDisposable? _liveNotifierSub;
    private IGattCharacteristic? _notifyLiveCharacteristic;
    private readonly IProtobufMessageConverter _protobufMessageConverter;

    public MainViewModel(IBleHostingManager hostingManager, IProtobufMessageConverter protobufMessageConverter)
    {
        _hostingManager = hostingManager;
        _protobufMessageConverter = protobufMessageConverter;
    }

    [ObservableProperty]
    public partial string LocalName { get; set; } = "Virtual Cygnus 1Ex";
    [ObservableProperty]
    public partial string LastWriteValue { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LastWriteTime { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LastReadValue { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LastReadTime { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int Subscribers { get; set; }
    [ObservableProperty]
    public partial string SubscribersLastValue { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int LiveSubscribers { get; set; }
    [ObservableProperty]
    public partial string LiveThickness { get; set; } = "3234";
    [ObservableProperty]
    public partial string LastLiveNotificationTime { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LastFrozenReadTime { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsLiveMeasurementFrozen { get; set; }
    [ObservableProperty]
    public partial bool IsAdvertising { get; set; }

    void BuildService(IGattServiceBuilder serviceBuilder)
    {
        serviceBuilder.AddCharacteristic(
            TMLinkWriteCommandCharacteristicId,
            cb =>
            {
                cb.SetWrite(request =>
                {
                    var command = _protobufMessageConverter.FromZippedProtobuf<Protobuf.V1.Command>(request.Data);
                    _commandType = command.commandType;
                    Protobuf.V1.Message? message = new()
                    {
                        commandType = command.commandType,
                    };
                    LastWriteValue = $"Command {command.commandType}";
                    LastWriteTime = DateTime.Now.ToString();

                    switch (_commandType)
                    {
                        case Protobuf.V1.CommandType.GetRecordList:
                            {
                                message.recordList = _gaugeData.RecordList;
                                LastReadValue = $"Record list {message.recordList.Items.Count}";
                                break;
                            }

                        case Protobuf.V1.CommandType.GetRecord:
                            {
                                message.record = _gaugeData.Records.FirstOrDefault(r => r.Name == command.Name);
                                _gaugeData.CurrentMeasurementIndex = 0;
                                LastReadValue = $"Record {message.record?.Name}";
                                break;
                            }

                        case Protobuf.V1.CommandType.GetRecordPoint:
                            {
                                message.recordPoint = _gaugeData.GetRecordPoint(command.Name, false);
                                LastReadValue = $"Record point {message.recordPoint?.colNumX}";
                                break;
                            }

                        case Protobuf.V1.CommandType.GetRecordPointAScan:
                            {
                                message.recordPoint = _gaugeData.GetRecordPoint(command.Name, true);
                                LastReadValue = $"Record point {message.recordPoint?.colNumX}";
                                break;
                            }

                        case Protobuf.V1.CommandType.GetBScanList:
                            {
                                message.bscanList = _gaugeData.BScanList;
                                LastReadValue = $"BScan list {message.bscanList.Items.Count}";
                                break;
                            }

                        case Protobuf.V1.CommandType.GetBScan:
                            {
                                _gaugeData.CurrentMeasurementIndex = 0;
                                message.Bscan = _gaugeData.BScans.FirstOrDefault(r => r.Name == command.Name);
                                LastReadValue = $"BScan {message.Bscan?.Name}";
                                break;
                            }

                        case Protobuf.V1.CommandType.GetBScanPoint:
                            {
                                message.bscanPoint = _gaugeData.GetBScanPoint(command.Name, false);
                                LastReadValue = $"BScan point {message.bscanPoint?.scanPointNum}";
                                break;
                            }

                        case Protobuf.V1.CommandType.GetBScanPointAScan:
                            {
                                message.bscanPoint = _gaugeData.GetBScanPoint(command.Name, true);
                                LastReadValue = $"BScan point {message.bscanPoint?.scanPointNum}";
                                break;
                            }
                        case Protobuf.V1.CommandType.GetGaugeInfo:
                            {
                                message.gaugeInfo = _gaugeData.GaugeInfo;
                                LastReadValue = $"Gauge info {message.gaugeInfo?.serialNumber}";
                                break;
                            }
                        case Protobuf.V1.CommandType.DeleteRecord:
                            {
                                int removed = _gaugeData.RecordList.Items.RemoveAll(r => r.Name == command.Name);
                                LastWriteValue = $"Deleted {removed} records {command.Name}";
                                message = null;
                                break;
                            }
                        case Protobuf.V1.CommandType.DeleteAllRecords:
                            {
                                _gaugeData.RecordList.Items.Clear();
                                LastWriteValue = $"Deleted all records";
                                message = null;
                                break;
                            }
                        case Protobuf.V1.CommandType.DeleteBScan:
                            {
                                _gaugeData.BScanList.Items.RemoveAll(r => r.Name == command.Name);
                                LastWriteValue = $"Deleted BScan {command.Name}";
                                message = null;
                                break;
                            }
                        case Protobuf.V1.CommandType.DeleteAllBScans:
                            {
                                _gaugeData.BScanList.Items.Clear();
                                LastWriteValue = $"Deleted all BScans";
                                message = null;
                                break;
                            }
                        case Protobuf.V1.CommandType.CancelRecordTransfer:
                            {
                                LastWriteValue = $"Cancelled record transfer";
                                message = null;
                                break;
                            }
                        case Protobuf.V1.CommandType.NewRecord:
                            {
                                _gaugeData.AddRecord(command.newRecord);
                                LastWriteValue = $"Added new record {command.newRecord.Name}";
                                message = null;
                                break;
                            }
                            case Protobuf.V1.CommandType.AddRecordPoints:
                            {
                                _gaugeData.AddRecordPoints(command.addRecordPoints);
                                LastWriteValue = $"Added {command.addRecordPoints.Mpoints.Count} points to record {command.addRecordPoints.Name}";
                                message = null;
                                break;
                            }
                    }

                    readCharacteristicValue = _protobufMessageConverter.ToZippedProtobuf(message);

                    Task.Delay(200).ContinueWith(t =>
                    {
                        MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            if (_notifyReadyCharacteristic != null)
                            {
                                var messageReady = new Protobuf.V1.NotifyMessage { commandType = _commandType.Value, readDataAvailable = message != null };
                                _notifyReadyCharacteristic.Notify(_protobufMessageConverter.ToProtobuf(messageReady), _notifyReadyCharacteristic.SubscribedCentrals.ToArray());
                                SubscribersLastValue = $"{_commandType} {DateTime.Now}";
                            }
                        });
                    });

                    return Task.FromResult(GattState.Success);
                });
            }
        );

        serviceBuilder.AddCharacteristic(
            TMLinkReadMessageCharacteristicId,
            cb =>
            {
                cb.SetRead(request =>
                {
                    LastReadTime = DateTime.Now.ToString();
                    byte[] chunk = readCharacteristicValue?.Skip(request.Offset).ToArray() ?? [];
                    return Task.FromResult(GattResult.Success(chunk));
                });
            }
        );

        _notifyReadyCharacteristic = serviceBuilder.AddCharacteristic(
            TMLinkNotifyMessageCharacteristicId,
            cb => cb.SetNotification(cs =>
            {
                IGattCharacteristic push = cs.Characteristic;
                var c = push.SubscribedCentrals.Count;
                MainThread.InvokeOnMainThreadAsync(() => Subscribers = c);
                return Task.CompletedTask;
            })
        );

        _notifyLiveCharacteristic = serviceBuilder.AddCharacteristic(
            TMLinkLiveCharacteristicId,
            cb => cb.SetNotification(cs =>
            {
                IGattCharacteristic notifier = cs.Characteristic;
                var c = notifier.SubscribedCentrals.Count;
                MainThread.InvokeOnMainThreadAsync(() => LiveSubscribers = c);
                if (c == 0)
                {
                    _liveNotifierSub?.Dispose();
                }
                else
                {
                    _liveNotifierSub = Observable
                        .Interval(TimeSpan.FromMilliseconds(150), Scheduler.Default)
                        .Select(_ => Observable.FromAsync<DateTime?>(async () =>
                        {
                            if (!IsLiveMeasurementFrozen)
                            {
                                Protobuf.V1.NotifyLiveMeasurement measurement = _gaugeData.CreateLiveMeasurement(Protobuf.V1.LiveMeasurementType.Live, LiveThickness);
                                await notifier.Notify(_protobufMessageConverter.ToProtobuf(measurement), notifier.SubscribedCentrals.ToArray());
                                return DateTime.Now;
                            }

                            return null;
                        }))
                        .Subscribe(x =>
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                DateTime? dateTime = (await x);
                                if (dateTime.HasValue)
                                {
                                    LastLiveNotificationTime = dateTime.Value.ToString();
                                }
                            })
                        );
                }

                return Task.CompletedTask;
            })
        );

        serviceBuilder.AddCharacteristic(
            TMLinkFrozenCharacteristicId,
            cb =>
            {
                cb.SetRead(request =>
                {
                    LastFrozenReadTime = DateTime.Now.ToString();
                    Protobuf.V1.FrozenLiveMeasurement measurement = _gaugeData.CreateFrozenLiveMeasurement(LiveThickness);
                    return Task.FromResult(GattResult.Success(_protobufMessageConverter.ToZippedProtobuf(measurement)));
                });
            }
        );
    }

    void BuildDeviceInformationService(IGattServiceBuilder serviceBuilder)
    {
        serviceBuilder.AddCharacteristic(
            DeviceModelCharacteristicId,
            cb =>
            {
                cb.SetRead(request =>
                {
                    byte[] deviceModel = Encoding.UTF8.GetBytes("C1Ex Virtual");
                    return Task.FromResult(GattResult.Success(deviceModel));
                });
            }
        );

        serviceBuilder.AddCharacteristic(
            SerialNumberCharacteristicId,
            cb =>
            {
                cb.SetRead(request =>
                {
                    byte[] serialNumber = Encoding.UTF8.GetBytes(_gaugeData.GaugeInfo.serialNumber.ToString());
                    return Task.FromResult(GattResult.Success(serialNumber));
                });
            }
        );

        serviceBuilder.AddCharacteristic(
            FirmwareRevisionCharacteristicId,
            cb =>
            {
                cb.SetRead(request =>
                {
                    byte[] firmwareRevision = Encoding.UTF8.GetBytes("1.4.11");
                    return Task.FromResult(GattResult.Success(firmwareRevision));
                });
            }
        );

        serviceBuilder.AddCharacteristic(
            SoftwareVersionCharacteristicId,
            cb =>
            {
                cb.SetRead(request =>
                {
                    byte[] protobufVersion = Encoding.UTF8.GetBytes("1");
                    return Task.FromResult(GattResult.Success(protobufVersion));
                });
            }
        );

        serviceBuilder.AddCharacteristic(
            ManufacturerNameCharacteristicId,
            cb =>
            {
                cb.SetRead(request =>
                {
                    byte[] manufacturerName = Encoding.UTF8.GetBytes("Cygnus Instruments");
                    return Task.FromResult(GattResult.Success(manufacturerName));
                });
            }
        );
    }

    private void BuildGenericAccessService(IGattServiceBuilder serviceBuilder)
    {
        serviceBuilder.AddCharacteristic(
            DeviceNameCharacteristicId,
            cb =>
            {
                cb.SetRead(request =>
                {
                    LastReadTime = DateTime.Now.ToString();
                    byte[] deviceName = Encoding.UTF8.GetBytes(LocalName);
                    return Task.FromResult(GattResult.Success(deviceName));
                });
            }
        );
    }

    partial void OnIsAdvertisingChanged(bool value)
    {
        Task.Run(async () =>
        { 
            _hostingManager.ClearServices();        

            if (_hostingManager.IsAdvertising)
            {
                _hostingManager.StopAdvertising();
            }
            else
            {
                //await _hostingManager.AddService(
                //    GenericAccessServiceId,
                //    true,
                //    BuildGenericAccessService
                //);

                await _hostingManager.AddService(
                    TMLinkServiceId,
                    true,
                    BuildService
                );

                await _hostingManager.AddService(
                    DeviceInformationServiceId,
                    true,
                    BuildDeviceInformationService
                );

                await _hostingManager.StartAdvertising(new AdvertisementOptions
                {
                    LocalName = LocalName,                    
                    ServiceUuids = [TMLinkServiceId, DeviceInformationServiceId]
                });
            } 
        });
    }

    partial void OnIsLiveMeasurementFrozenChanged(bool value)
    {
        if (value)
        {
            Task.Run(async () =>
            {
                var notifyLiveCharacteristic = _notifyLiveCharacteristic;
                if (notifyLiveCharacteristic != null)
                {
                    Protobuf.V1.NotifyLiveMeasurement measurement = _gaugeData.CreateLiveMeasurement(Protobuf.V1.LiveMeasurementType.Frozen, LiveThickness);
                    await notifyLiveCharacteristic.Notify(_protobufMessageConverter.ToProtobuf(measurement), notifyLiveCharacteristic.SubscribedCentrals.ToArray());
                }
            });
        }
    }
}
