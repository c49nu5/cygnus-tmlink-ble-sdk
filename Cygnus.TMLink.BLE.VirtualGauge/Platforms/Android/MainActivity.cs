using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;

namespace Cygnus.TMLink.BLE.VirtualGauge;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (OperatingSystem.IsAndroidVersionAtLeast(31) && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(Platform.CurrentActivity, [Manifest.Permission.BluetoothConnect], 102);
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(31) && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothAdvertise) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(Platform.CurrentActivity, [Manifest.Permission.BluetoothAdvertise], 102);
        }

        if (!OperatingSystem.IsAndroidVersionAtLeast(31) && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Bluetooth) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(Platform.CurrentActivity, [Manifest.Permission.Bluetooth], 102);
        }
    }
}
