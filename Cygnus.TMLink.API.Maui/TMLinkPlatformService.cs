using Cygnus.TMLink.API.Interfaces;
using InTheHand.Bluetooth;
using Microsoft.Extensions.Logging;

namespace Cygnus.TMLink.API.Maui;

public class TMLinkPlatformService : IPlatformService, IUserDialogService
{
    protected readonly ILogger<TMLinkPlatformService> _logger;

    public TMLinkPlatformService(
        ILogger<TMLinkPlatformService> logger)
    {
        _logger = logger;
    }

    public virtual async Task<bool> CheckBluetoothConfiguration()
    {
        if (!await Bluetooth.GetAvailabilityAsync())
        {
            await ShowMessage("Bluetooth is not ON.\nPlease turn on Bluetooth and try again.");
            return false;
        }

        _logger.LogInformation("Verifying Bluetooth permissions..");
        var permissionResult = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
        if (permissionResult != PermissionStatus.Granted)
        {
            _logger.LogInformation("Requesting Bluetooth permissions..");
            permissionResult = await Permissions.RequestAsync<Permissions.Bluetooth>();
        }

        _logger.LogInformation($"Result of requesting Bluetooth permissions: '{permissionResult}'");
        if (permissionResult != PermissionStatus.Granted)
        {
            _logger.LogInformation("Permissions not available, direct user to settings screen.");
            await ShowMessage("Permission denied. Not scanning.");
            AppInfo.ShowSettingsUI();
            return false;
        }

        return true;
    }

    public virtual Task ShowMessage(string message, string cancel = "")
    {
        _logger.LogInformation(message);
        Page? page = Application.Current?.Windows.ElementAtOrDefault(0)?.Page;
        if (page == null)
        {
            return Task.FromResult(false);
        }

        return page.Dispatcher.DispatchAsync(() =>
        {
            if (string.IsNullOrWhiteSpace(cancel))
            {
                return page.DisplayAlertAsync("Gauge Scanner", message, "OK");
            }

            return page.DisplayAlertAsync("Gauge Scanner", message, "OK", cancel);
        });
    }
}
