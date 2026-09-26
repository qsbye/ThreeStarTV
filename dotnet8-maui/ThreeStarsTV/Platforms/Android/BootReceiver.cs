using Android.App;
using Android.Content;

namespace ThreeStarsTV;

[BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = false)]
[IntentFilter(new[] { Intent.ActionBootCompleted })]
public class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent?.Action != Intent.ActionBootCompleted) return;
        AppState.Init();
        if (!AppState.Repo.App.AutoStart) return;
        var launch = new Intent(context, typeof(MainActivity))
            .AddFlags(ActivityFlags.NewTask);
        try
        {
            context.StartActivity(launch);
        }
        catch { }
    }
}
