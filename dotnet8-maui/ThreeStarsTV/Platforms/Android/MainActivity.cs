using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace ThreeStarsTV;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ScreenOrientation = ScreenOrientation.SensorLandscape,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        AppState.Init();
        ApplyWindowFlags();
        AppState.Repo.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Data.ConfigRepository.App))
                RunOnUiThread(ApplyWindowFlags);
        };
    }

    private void ApplyWindowFlags()
    {
        var cfg = AppState.Repo.App;
        var window = Window;
        if (window == null) return;

        // 屏幕常亮
        if (cfg.TopMost)
            window.AddFlags(WindowManagerFlags.KeepScreenOn);
        else
            window.ClearFlags(WindowManagerFlags.KeepScreenOn);

        // 沉浸式全屏
        var decorView = window.DecorView;
        if (cfg.StartMaximized || cfg.GridFullscreen)
        {
            var uiOptions = (int)(SystemUiFlags.HideNavigation
                | SystemUiFlags.Fullscreen
                | SystemUiFlags.ImmersiveSticky
                | SystemUiFlags.LayoutHideNavigation
                | SystemUiFlags.LayoutFullscreen
                | SystemUiFlags.LayoutStable);
            decorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        }
        else
        {
            decorView.SystemUiVisibility = (StatusBarVisibility)(int)SystemUiFlags.LayoutStable;
        }
    }
}
