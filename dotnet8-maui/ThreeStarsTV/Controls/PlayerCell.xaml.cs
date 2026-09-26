using System.ComponentModel;
using System.Diagnostics;
using System.Timers;

namespace ThreeStarsTV.Controls;

public partial class PlayerCell : ContentView
{
    public static readonly BindableProperty CellIdProperty =
        BindableProperty.Create(nameof(CellId), typeof(int), typeof(PlayerCell), 0,
            propertyChanged: (b, _, _) => ((PlayerCell)b).RefreshFromConfig());

    public static readonly BindableProperty IsMaximizedProperty =
        BindableProperty.Create(nameof(IsMaximized), typeof(bool), typeof(PlayerCell), false,
            propertyChanged: (b, _, _) => ((PlayerCell)b).UpdateMaxButton());

    public event EventHandler<int>? ToggleMaximizeRequested;

    private readonly System.Timers.Timer _loadingTimer;
    private readonly Stopwatch _loadingWatch = new();
    private string _currentUrl = "";
    private bool _locked;
    private bool _isLoading;
    private string? _hlsHtml;

    public int CellId
    {
        get => (int)GetValue(CellIdProperty);
        set => SetValue(CellIdProperty, value);
    }

    public bool IsMaximized
    {
        get => (bool)GetValue(IsMaximizedProperty);
        set => SetValue(IsMaximizedProperty, value);
    }

    public PlayerCell()
    {
        InitializeComponent();
        _loadingTimer = new System.Timers.Timer(100) { AutoReset = true };
        _loadingTimer.Elapsed += OnLoadingTick;
        UrlEntry.Completed += (s, e) => CommitUrl();
        RemarkEntry.Completed += (s, e) => CommitRemark();
        UrlEntry.Unfocused += (s, e) => CommitUrl();
        RemarkEntry.Unfocused += (s, e) => CommitRemark();
        I18n.I18n.Instance.PropertyChanged += OnI18nChanged;
        UpdateTexts();
    }

    private void OnI18nChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "Item[]" or "Language")
            MainThread.BeginInvokeOnMainThread(UpdateTexts);
    }

    private void UpdateTexts()
    {
        var i18n = I18n.I18n.Instance;
        UrlEntry.Placeholder = i18n.T("urlPlaceholder");
        RemarkEntry.Placeholder = i18n.T("remarkPlaceholder");
        LockBtn.Text = _locked ? i18n.T("unlock") : i18n.T("lock");
        RefreshBtn.Text = "⟳ " + i18n.T("refresh");
        UpdateMaxButton();
        EmptyHint.Text = i18n.T("emptyCellHint");
    }

    private void UpdateMaxButton()
    {
        MaxBtn.Text = I18n.I18n.Instance.T(IsMaximized ? "restore" : "maximize");
    }

    public void RefreshFromConfig()
    {
        var item = AppState.Repo.Camera.Items.FirstOrDefault(i => i.Id == CellId);
        var url = item?.Ip ?? "";
        var remark = item?.Remark ?? "";
        _locked = item?.Locked ?? false;
        if (UrlEntry.Text != url) UrlEntry.Text = url;
        if (RemarkEntry.Text != remark) RemarkEntry.Text = remark;
        UrlEntry.IsReadOnly = _locked;
        RemarkEntry.IsReadOnly = _locked;
        UpdateTexts();
    }

    private void CommitUrl()
    {
        var text = UrlEntry.Text?.Trim() ?? "";
        var item = AppState.Repo.Camera.Items.FirstOrDefault(i => i.Id == CellId);
        if (item == null || item.Ip == text) return;
        AppState.Repo.UpdateCamera(cam =>
        {
            var it = cam.Items.FirstOrDefault(x => x.Id == CellId);
            if (it != null) it.Ip = text;
        });
        Load(text);
    }

    private void CommitRemark()
    {
        var text = RemarkEntry.Text?.Trim() ?? "";
        var item = AppState.Repo.Camera.Items.FirstOrDefault(i => i.Id == CellId);
        if (item == null || item.Remark == text) return;
        AppState.Repo.UpdateCamera(cam =>
        {
            var it = cam.Items.FirstOrDefault(x => x.Id == CellId);
            if (it != null) it.Remark = text;
        });
    }

    private void OnLockClicked(object? sender, EventArgs e)
    {
        AppState.Repo.UpdateCamera(cam =>
        {
            var it = cam.Items.FirstOrDefault(x => x.Id == CellId);
            if (it != null) it.Locked = !it.Locked;
        });
        RefreshFromConfig();
    }

    private void OnRefreshClicked(object? sender, EventArgs e) => Reload();

    private void OnMaxClicked(object? sender, EventArgs e) =>
        ToggleMaximizeRequested?.Invoke(this, CellId);

    public void Load(string url) => LoadInternal(Normalize(url));

    public void Reload() => LoadInternal(_currentUrl);

    public void ReleaseAll()
    {
        _loadingTimer.Stop();
        try { Web.Handler?.DisconnectHandler(); } catch { }
    }

    private void LoadInternal(string url)
    {
        _currentUrl = url;

        if (string.IsNullOrEmpty(url))
        {
            StopPlayback();
            Web.IsVisible = false;
            EmptyHint.IsVisible = true;
            HideLoading();
            return;
        }

        Web.IsVisible = true;
        EmptyHint.IsVisible = false;

        if (url.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
        {
            PlayHls(url);
        }
        else
        {
            PlayWeb(url);
        }
    }

    // ---------- HLS via embedded hls.js HTML ----------
    private void PlayHls(string url)
    {
        ShowLoading();
        var html = BuildHlsHtml(url);
        Web.Source = new HtmlWebViewSource
        {
            Html = html,
            BaseUrl = "file:///android_asset/", // 让 hls.html 能引用同目录的 hls.min.js
        };
    }

    private string BuildHlsHtml(string url)
    {
        _hlsHtml ??= LoadEmbeddedHls();
        var jsUrl = System.Text.Json.JsonSerializer.Serialize(url);
        var autoUnmute = AppState.Repo.App.AutoUnmute;
        return _hlsHtml
            .Replace("__URL__", jsUrl)
            .Replace("__MUTED__", autoUnmute ? "false" : "true");
    }

    private static string LoadEmbeddedHls()
    {
        using var stream = FileSystem.OpenAppPackageFileAsync("hls.html").GetAwaiter().GetResult();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // ---------- Web playback ----------
    private void PlayWeb(string url)
    {
        ShowLoading();
        Web.Source = new UrlWebViewSource { Url = url };
    }

    private void OnWebNavigated(object? sender, WebNavigatedEventArgs e)
    {
        HideLoading();
        if (e.Url?.Contains("cctv.com", StringComparison.OrdinalIgnoreCase) == true)
        {
            InjectAutoFullscreen();
        }
    }

    private void InjectAutoFullscreen()
    {
        const string script = """
        (function(){
          if (/cctv\.com$|\.cctv\.com$/.test(location.hostname)) {
            var timer = setInterval(function(){
              try {
                var els = document.querySelectorAll('[title*="全屏"], [class*="full"], [id*="full"]');
                for (var i = 0; i < els.length; i++) {
                  var b = els[i], t = (b.title || '') + ' ' + (b.className || '') + ' ' + (b.id || '');
                  if (/全屏|full/i.test(t) && b.offsetParent !== null) { b.click(); clearInterval(timer); return; }
                }
              } catch (e) {}
            }, 1000);
          }
        })();
        """;
        try { Web.EvaluateJavaScriptAsync(script); } catch { }
    }

    // ---------- 加载遮罩 ----------
    private void ShowLoading()
    {
        _isLoading = true;
        _loadingWatch.Restart();
        LoadingText.Text = $"{I18n.I18n.Instance.T("loading")} 0 ms";
        LoadingBar.Progress = 0;
        LoadingMask.IsVisible = true;
        _loadingTimer.Start();
    }

    private void HideLoading()
    {
        _isLoading = false;
        _loadingTimer.Stop();
        LoadingMask.IsVisible = false;
    }

    private void OnLoadingTick(object? sender, ElapsedEventArgs e)
    {
        if (!_isLoading) return;
        var ms = _loadingWatch.ElapsedMilliseconds;
        if (ms > 60000)
        {
            MainThread.BeginInvokeOnMainThread(HideLoading);
            return;
        }
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadingText.Text = $"{I18n.I18n.Instance.T("loading")} {ms} ms";
            LoadingBar.Progress = Math.Min(90, ms / 100) / 100.0;
        });
    }

    private void StopPlayback()
    {
        try { Web.Source = new HtmlWebViewSource { Html = "<html><body style='background:#000'></body></html>" }; }
        catch { }
    }

    private static string Normalize(string url)
    {
        var u = (url ?? "").Trim();
        if (u.Length == 0) return "";
        if (!u.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            u = "http://" + u;
        return u;
    }
}
