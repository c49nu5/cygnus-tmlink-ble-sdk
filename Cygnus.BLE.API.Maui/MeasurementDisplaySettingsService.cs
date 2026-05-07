using Cygnus.Models;
using Cygnus.Interfaces;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Cygnus.BLE.API.Maui;

public class MeasurementDisplaySettingsService : IMeasurementDisplaySettingsService
{
    private readonly IPreferences _preferences;
    protected readonly ILogger<MeasurementDisplaySettingsService> _logger;

    private MeasurementUnits? _units;
    private MeasurementResolution? _resolution;

    public MeasurementDisplaySettingsService(ILogger<MeasurementDisplaySettingsService> logger) 
        : this(Preferences.Default, logger)
    {
    }

    internal MeasurementDisplaySettingsService(
        IPreferences preferences,
        ILogger<MeasurementDisplaySettingsService> logger)
    {
        _preferences = preferences;
        _logger = logger;
    }

    public MeasurementUnits Units { get => _units ??= GetSetting(MeasurementUnits.Default); set => SetSetting(_units = value); }
    public MeasurementResolution Resolution { get => _resolution ??= GetSetting(MeasurementResolution.Default); set => SetSetting(_resolution = value); }

    protected T GetSetting<T>(T defaultValue, [CallerMemberName] string propertyName = "")
    {
        if (defaultValue is Enum)
        {
            string? enumValue = _preferences.Get(propertyName, defaultValue.ToString());
            return Enum.TryParse(defaultValue.GetType(), enumValue, out object? result) ? (T)result : defaultValue;
        }

        return _preferences.Get(propertyName, defaultValue);
    }

    protected virtual void SetSetting<T>(T newValue, [CallerMemberName] string propertyName = "")
    {
        if (newValue is Enum enumValue)
        {
            _preferences.Set(propertyName, enumValue.ToString());
            return;
        }

        _preferences.Set(propertyName, newValue);
    }
}
