using System.ComponentModel;
using ThreeStarsTV.Controls;
using ThreeStarsTV.Data;

namespace ThreeStarsTV.Pages;

public partial class MainPage : ContentPage
{
    private readonly List<PlayerCell> _cells = new();
    private readonly System.Timers.Timer _clockTimer;
    private int _maximizedId = -1;

    public MainPage()
    {
        InitializeComponent();
        TitleLabel.Text = I18n.I18n.Tr("appTitle");
        SettingsBtn.Text = "⚙ " + I18n.I18n.Tr("setting");
        AboutBtn.Text = "ℹ " + I18n.I18n.Tr("about");
        FullscreenBtn.Text = I18n.I18n.Tr("fullscreen");
        ExitFullscreenLabel.Text = I18n.I18n.Tr("exitFullscreen");
        UpdateThemeText();
        UpdateLangText();
        _clockTimer = new System.Timers.Timer(1000) { AutoReset = true };
        _clockTimer.Elapsed += (_, _) => MainThread.BeginInvokeOnMainThread(UpdateClock);
        _clockTimer.Start();
        AppState.Repo.PropertyChanged += OnRepoChanged;
        I18n.I18n.Instance.PropertyChanged += OnI18nChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RebuildGrid();
        ApplyAppConfig();
        WarmupPrefetch();
    }

    private void OnRepoChanged(object? sender, PropertyChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (e.PropertyName == nameof(ConfigRepository.Camera))
                RebuildGrid();
            else if (e.PropertyName == nameof(ConfigRepository.App))
                ApplyAppConfig();
        });
    }

    private void OnI18nChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "Item[]" or "Language")
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TitleLabel.Text = I18n.I18n.Tr("appTitle");
                SettingsBtn.Text = "⚙ " + I18n.I18n.Tr("setting");
                AboutBtn.Text = "ℹ " + I18n.I18n.Tr("about");
                FullscreenBtn.Text = I18n.I18n.Tr("fullscreen");
                ExitFullscreenLabel.Text = I18n.I18n.Tr("exitFullscreen");
                UpdateThemeText();
                UpdateLangText();
            });
        }
    }

    private void UpdateThemeText() =>
        ThemeBtn.Text = AppState.Repo.App.Theme == "dark"
            ? I18n.I18n.Tr("themeLight")
            : I18n.I18n.Tr("themeDark");

    private void UpdateLangText() =>
        LangBtn.Text = I18n.I18n.Instance.Language == "en"
            ? I18n.I18n.Tr("languageZh")
            : I18n.I18n.Tr("languageEn");

    private void UpdateClock()
    {
        TimeLabel.Text = I18n.I18n.Tr("systemTime") + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        if (string.IsNullOrEmpty(VersionLabel.Text))
            VersionLabel.Text = I18n.I18n.Tr("version") + " " + AppInfo.VersionString;
    }

    private void ApplyAppConfig()
    {
        var cfg = AppState.Repo.App;

        // 屏幕常亮
        if (cfg.TopMost)
            DeviceDisplay.Current.KeepScreenOn = true;
        else
            DeviceDisplay.Current.KeepScreenOn = false;

        // 工具栏 / 状态栏 可见性（网格全屏）
        var gf = cfg.GridFullscreen;
        Toolbar.IsVisible = !gf;
        StatusBar.IsVisible = !gf;
        ExitFullscreenBtn.IsVisible = gf;
        // 沉浸式全屏由 MainActivity 中 AppConfig 变化监听处理
    }

    private void RebuildGrid()
    {
        var cam = AppState.Repo.Camera;
        var count = cam.Count;
        var delay = cam.Delay;

        // 数量减少时释放多余单元格
        while (_cells.Count > count)
        {
            var last = _cells[^1];
            last.ReleaseAll();
            _cells.RemoveAt(_cells.Count - 1);
        }
        if (_maximizedId >= count) _maximizedId = -1;

        // 确保有正确数量的单元格
        for (int i = 0; i < count; i++)
        {
            if (i >= _cells.Count)
            {
                var c = new PlayerCell
                {
                    CellId = i,
                    IsMaximized = false,
                };
                c.ToggleMaximizeRequested += OnCellToggleMaximize;
                _cells.Add(c);
            }
            else
            {
                _cells[i].CellId = i;
                _cells[i].IsMaximized = false;
                _cells[i].RefreshFromConfig();
            }
        }

        RefreshLayout();

        // 加入可视树后刷新一次，确保 Entry 文本渲染
        foreach (var c in _cells) c.RefreshFromConfig();

        // 错峰加载
        for (int i = 0; i < count; i++)
        {
            var idx = i;
            var url = cam.Items.FirstOrDefault(x => x.Id == idx)?.Ip ?? "";
            if (idx == 0 || delay == 0)
                _cells[idx].Load(url);
            else
                Task.Delay(idx * delay * 1000).ContinueWith(_ =>
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (idx < _cells.Count) _cells[idx].Load(url);
                    }));
        }
    }

    private void RefreshLayout()
    {
        CellsHost.Children.Clear();
        CellsHost.RowDefinitions.Clear();
        CellsHost.ColumnDefinitions.Clear();

        var count = _cells.Count;
        if (count == 0) return;

        if (_maximizedId >= 0 && _maximizedId < count)
        {
            // 最大化单格模式
            CellsHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            CellsHost.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            var cell = _cells[_maximizedId];
            cell.IsMaximized = true;
            Grid.SetRow(cell, 0);
            Grid.SetColumn(cell, 0);
            CellsHost.Children.Add(cell);
            return;
        }

        var layout = ComputeLayout(count);
        int cols = layout.cols, rows = layout.rows;

        for (int r = 0; r < rows; r++)
            CellsHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        for (int c = 0; c < cols; c++)
            CellsHost.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int i = 0; i < count; i++)
        {
            var row = i / cols;
            var col = i % cols;
            var cell = _cells[i];
            cell.IsMaximized = false;
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);
            CellsHost.Children.Add(cell);
        }
    }

    private static (int cols, int rows) ComputeLayout(int count)
    {
        var layouts = new Dictionary<int, (int, int)>
        {
            [1] = (1, 1), [2] = (2, 1), [4] = (2, 2), [6] = (3, 2),
            [9] = (3, 3), [12] = (4, 3), [16] = (4, 4),
        };
        if (layouts.TryGetValue(count, out var l)) return l;
        int c = (int)Math.Ceiling(Math.Sqrt(count));
        return (c, c);
    }

    private void OnCellToggleMaximize(object? sender, int id)
    {
        if (_maximizedId == id)
            _maximizedId = -1;
        else
            _maximizedId = id;
        RebuildGrid(); // 会重新布局
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("settings");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Settings nav failed: {ex}");
        }
    }

    private async void OnAboutClicked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("about");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"About nav failed: {ex}");
        }
    }

    private void OnThemeClicked(object? sender, EventArgs e)
    {
        var next = AppState.Repo.App.Theme == "dark" ? "light" : "dark";
        AppState.Repo.UpdateApp(a => a.Theme = next);
        UpdateThemeText();
    }

    private void OnLangClicked(object? sender, EventArgs e)
    {
        var next = I18n.I18n.Instance.Language == "en" ? "zh" : "en";
        I18n.I18n.Instance.Language = next;
        AppState.Repo.UpdateApp(a => a.Language = next);
        UpdateLangText();
    }

    private void OnFullscreenClicked(object? sender, EventArgs e)
    {
        AppState.Repo.UpdateApp(a => a.GridFullscreen = true);
    }

    private void OnExitFullscreenTapped(object? sender, TappedEventArgs e)
    {
        AppState.Repo.UpdateApp(a => a.GridFullscreen = false);
    }

    private void WarmupPrefetch()
    {
        Task.Run(async () =>
        {
            foreach (var url in new[] { ConfigRepository.DefaultStreamUrl, ConfigRepository.FallbackWebUrl })
            {
                try
                {
                    using var c = new HttpClient();
                    c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    c.Timeout = TimeSpan.FromSeconds(5);
                    await c.GetAsync(url);
                }
                catch { }
            }
        });
    }
}
