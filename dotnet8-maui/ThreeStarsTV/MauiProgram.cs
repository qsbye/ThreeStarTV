using Microsoft.Extensions.Logging;

namespace ThreeStarsTV;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if ANDROID
        // 桌面模式 WebView：UA + 宽 ViewPort，对标桌面版 WebView2 行为
        const string DesktopUA =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.ModifyMapping(nameof(IWebView), (handler, view, _) =>
        {
            var wv = handler.PlatformView;
            var s = wv.Settings;
            s.UserAgentString = DesktopUA;
            s.UseWideViewPort = true;          // 使用宽视口（桌面布局）
            s.LoadWithOverviewMode = true;     // 缩放至一屏
            s.JavaScriptEnabled = true;
            s.DomStorageEnabled = true;
            s.MediaPlaybackRequiresUserGesture = false; // 自动播放
        });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
