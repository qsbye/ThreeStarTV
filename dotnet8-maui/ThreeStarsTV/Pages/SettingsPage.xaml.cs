using System.ComponentModel;
using ThreeStarsTV.Data;

namespace ThreeStarsTV.Pages;

public partial class SettingsPage : ContentPage
{
    private int _tab;
    private readonly List<Button> _tabButtons = new();
    private static readonly int[] Counts = { 1, 2, 4, 6, 9, 12, 16 };

    public SettingsPage()
    {
        InitializeComponent();
        TitleLabel.Text = I18n.I18n.Tr("systemSettings");
        RebuildTabs();
        I18n.I18n.Instance.PropertyChanged += OnI18nChanged;
        AppState.Repo.PropertyChanged += OnRepoChanged;
    }

    private void OnI18nChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "Item[]" or "Language")
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TitleLabel.Text = I18n.I18n.Tr("systemSettings");
                RebuildTabs();
            });
        }
    }

    private void OnRepoChanged(object? sender, PropertyChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => RenderTab());
    }

    private void RebuildTabs()
    {
        TabBar.Children.Clear();
        _tabButtons.Clear();
        var tabs = new[]
        {
            I18n.I18n.Tr("cameraDisplay"),
            I18n.I18n.Tr("cameraSettings"),
            I18n.I18n.Tr("displaySettings"),
            I18n.I18n.Tr("appSettings"),
            I18n.I18n.Tr("favUrls"),
        };
        for (int i = 0; i < tabs.Length; i++)
        {
            var idx = i;
            var btn = new Button
            {
                Text = tabs[i],
                FontSize = 12,
                Padding = new Thickness(10, 6),
                BackgroundColor = Colors.Transparent,
            };
            btn.Clicked += (s, e) =>
            {
                _tab = idx;
                RefreshTabStyles();
                RenderTab();
            };
            TabBar.Children.Add(btn);
            _tabButtons.Add(btn);
        }
        RefreshTabStyles();
        RenderTab();
    }

    private void RefreshTabStyles()
    {
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            var active = i == _tab;
            _tabButtons[i].TextColor = active
                ? Color.FromArgb("#0078D4")
                : (Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#CCCCCC")
                    : Color.FromArgb("#333333"));
            _tabButtons[i].FontAttributes = active ? FontAttributes.Bold : FontAttributes.None;
        }
    }

    private void RenderTab()
    {
        TabContent.Content = _tab switch
        {
            0 => BuildDisplayPage(),
            1 => BuildCameraPage(),
            2 => BuildThemePage(),
            3 => BuildAppPage(),
            4 => BuildFavoritesPage(),
            _ => null,
        };
    }

    // ---------- 相机显示 ----------
    private View BuildDisplayPage()
    {
        var scroll = new ScrollView();
        var stack = new VerticalStackLayout { Padding = 12, Spacing = 8 };
        var cam = AppState.Repo.Camera;

        stack.Children.Add(new Label
        {
            Text = I18n.I18n.Tr("count"),
            FontSize = 14,
            TextColor = Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#CCCCCC") : Color.FromArgb("#333333"),
        });

        var countGrid = new Grid { ColumnSpacing = 6 };
        for (int i = 0; i < Counts.Length; i++)
        {
            countGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var n = Counts[i];
            var btn = new Button
            {
                Text = n.ToString(),
                FontSize = 12,
                Padding = new Thickness(10, 4),
                BackgroundColor = cam.Count == n ? Color.FromArgb("#0078D4") : Color.FromArgb("#DDDDDD"),
                TextColor = cam.Count == n ? Colors.White : Color.FromArgb("#333333"),
            };
            btn.Clicked += (s, e) => AppState.Repo.UpdateCamera(c => c.Count = n);
            Grid.SetColumn(btn, i);
            countGrid.Children.Add(btn);
        }
        stack.Children.Add(countGrid);

        stack.Children.Add(new Label
        {
            Text = I18n.I18n.Tr("delay"),
            FontSize = 14,
            Margin = new Thickness(0, 16, 0, 0),
            TextColor = Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#CCCCCC") : Color.FromArgb("#333333"),
        });

        var delayEntry = new Entry
        {
            Text = cam.Delay.ToString(),
            Keyboard = Keyboard.Numeric,
            WidthRequest = 140,
            FontSize = 13,
        };
        delayEntry.TextChanged += (s, e) =>
        {
            if (int.TryParse(e.NewTextValue, out var v) && v >= 0)
                AppState.Repo.UpdateCamera(c => c.Delay = v);
        };
        stack.Children.Add(delayEntry);

        scroll.Content = stack;
        return scroll;
    }

    // ---------- 相机设置 ----------
    private View BuildCameraPage()
    {
        var scroll = new ScrollView();
        var stack = new VerticalStackLayout { Padding = 12, Spacing = 10 };
        var cam = AppState.Repo.Camera;
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

        for (int i = 0; i < cam.Count; i++)
        {
            var idx = i;
            var item = cam.Items.FirstOrDefault(x => x.Id == idx);
            var border = new Border
            {
                Padding = 10,
                BackgroundColor = isDark ? Color.FromArgb("#2D2D30") : Color.FromArgb("#F7F7F7"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
            };
            var col = new VerticalStackLayout { Spacing = 4 };
            col.Children.Add(new Label
            {
                Text = $"{I18n.I18n.Tr("camera")} {idx + 1}",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = isDark ? Color.FromArgb("#CCCCCC") : Color.FromArgb("#333333"),
            });

            var urlEntry = new Entry { Text = item?.Ip ?? "", FontSize = 12, IsReadOnly = item?.Locked == true };
            var remarkEntry = new Entry { Text = item?.Remark ?? "", FontSize = 12, IsReadOnly = item?.Locked == true };

            col.Children.Add(new Label { Text = I18n.I18n.Tr("url"), FontSize = 11, TextColor = isDark ? Color.FromArgb("#888888") : Color.FromArgb("#777777") });
            col.Children.Add(urlEntry);
            col.Children.Add(new Label { Text = I18n.I18n.Tr("remark"), FontSize = 11, TextColor = isDark ? Color.FromArgb("#888888") : Color.FromArgb("#777777") });
            col.Children.Add(remarkEntry);

            var row = new HorizontalStackLayout { Spacing = 8 };
            var lockCheck = new CheckBox { IsChecked = item?.Locked == true };
            lockCheck.CheckedChanged += (s, e) =>
                AppState.Repo.UpdateCamera(c =>
                {
                    var it = c.Items.FirstOrDefault(x => x.Id == idx);
                    if (it != null) it.Locked = e.Value;
                });
            row.Children.Add(lockCheck);
            row.Children.Add(new Label { Text = I18n.I18n.Tr("locked"), FontSize = 12, VerticalOptions = LayoutOptions.Center, TextColor = isDark ? Color.FromArgb("#CCCCCC") : Color.FromArgb("#333333") });
            var saveBtn = new Button
            {
                Text = I18n.I18n.Tr("save"),
                FontSize = 12,
                Padding = new Thickness(10, 4),
                BackgroundColor = Color.FromArgb("#0078D4"),
                TextColor = Colors.White,
            };
            saveBtn.Clicked += (s, e) =>
            {
                AppState.Repo.UpdateCamera(c =>
                {
                    var it = c.Items.FirstOrDefault(x => x.Id == idx);
                    if (it != null)
                    {
                        it.Ip = urlEntry.Text?.Trim() ?? "";
                        it.Remark = remarkEntry.Text?.Trim() ?? "";
                    }
                });
            };
            row.Children.Add(saveBtn);
            col.Children.Add(row);
            border.Content = col;
            stack.Children.Add(border);
        }

        scroll.Content = stack;
        return scroll;
    }

    // ---------- 显示设置 ----------
    private View BuildThemePage()
    {
        var scroll = new ScrollView();
        var stack = new VerticalStackLayout { Padding = 12, Spacing = 8 };
        var app = AppState.Repo.App;
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var textColor = isDark ? Color.FromArgb("#CCCCCC") : Color.FromArgb("#333333");

        stack.Children.Add(new Label { Text = I18n.I18n.Tr("themeSetting"), FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = textColor });

        var darkRadio = MakeRadio(I18n.I18n.Tr("themeDark"), app.Theme == "dark", () =>
            AppState.Repo.UpdateApp(a => a.Theme = "dark"), textColor);
        var lightRadio = MakeRadio(I18n.I18n.Tr("themeLight"), app.Theme != "dark", () =>
            AppState.Repo.UpdateApp(a => a.Theme = "light"), textColor);
        stack.Children.Add(darkRadio);
        stack.Children.Add(lightRadio);

        var gfRow = new HorizontalStackLayout { Spacing = 6 };
        var gfCheck = new CheckBox { IsChecked = app.GridFullscreen };
        gfCheck.CheckedChanged += (s, e) =>
            AppState.Repo.UpdateApp(a => a.GridFullscreen = e.Value);
        gfRow.Children.Add(gfCheck);
        gfRow.Children.Add(new Label { Text = I18n.I18n.Tr("gridFullscreen"), FontSize = 13, VerticalOptions = LayoutOptions.Center, TextColor = textColor });
        stack.Children.Add(gfRow);

        scroll.Content = stack;
        return scroll;
    }

    private View MakeRadio(string label, bool isChecked, Action onChecked, Color textColor)
    {
        var row = new HorizontalStackLayout { Spacing = 6 };
        var rb = new RadioButton { IsChecked = isChecked };
        rb.CheckedChanged += (s, e) => { if (e.Value) onChecked(); };
        row.Children.Add(rb);
        row.Children.Add(new Label { Text = label, FontSize = 13, VerticalOptions = LayoutOptions.Center, TextColor = textColor });
        return row;
    }

    // ---------- 软件设置 ----------
    private View BuildAppPage()
    {
        var scroll = new ScrollView();
        var stack = new VerticalStackLayout { Padding = 12, Spacing = 6 };
        var app = AppState.Repo.App;
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var textColor = isDark ? Color.FromArgb("#CCCCCC") : Color.FromArgb("#333333");

        stack.Children.Add(new Label { Text = I18n.I18n.Tr("languageSetting"), FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = textColor });

        var zhRow = new HorizontalStackLayout { Spacing = 6 };
        var zhRb = new RadioButton { IsChecked = I18n.I18n.Instance.Language != "en" };
        zhRb.CheckedChanged += (s, e) => { if (e.Value) { I18n.I18n.Instance.Language = "zh"; AppState.Repo.UpdateApp(a => a.Language = "zh"); } };
        zhRow.Children.Add(zhRb);
        zhRow.Children.Add(new Label { Text = I18n.I18n.Tr("languageZh"), FontSize = 13, VerticalOptions = LayoutOptions.Center, TextColor = textColor });
        stack.Children.Add(zhRow);

        var enRow = new HorizontalStackLayout { Spacing = 6 };
        var enRb = new RadioButton { IsChecked = I18n.I18n.Instance.Language == "en" };
        enRb.CheckedChanged += (s, e) => { if (e.Value) { I18n.I18n.Instance.Language = "en"; AppState.Repo.UpdateApp(a => a.Language = "en"); } };
        enRow.Children.Add(enRb);
        enRow.Children.Add(new Label { Text = I18n.I18n.Tr("languageEn"), FontSize = 13, VerticalOptions = LayoutOptions.Center, TextColor = textColor });
        stack.Children.Add(enRow);

        stack.Children.Add(new Label { Text = I18n.I18n.Tr("startupSettings"), FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = textColor, Margin = new Thickness(0, 12, 0, 0) });

        stack.Children.Add(MakeStartupCheckbox(I18n.I18n.Tr("autoUnmute"), app.AutoUnmute, v => AppState.Repo.UpdateApp(a => a.AutoUnmute = v), textColor));
        stack.Children.Add(MakeStartupCheckbox(I18n.I18n.Tr("autoStart"), app.AutoStart, v => AppState.Repo.UpdateApp(a => a.AutoStart = v), textColor));
        stack.Children.Add(MakeStartupCheckbox(I18n.I18n.Tr("topMost"), app.TopMost, v => AppState.Repo.UpdateApp(a => a.TopMost = v), textColor));
        stack.Children.Add(MakeStartupCheckbox(I18n.I18n.Tr("startMaximized"), app.StartMaximized, v => AppState.Repo.UpdateApp(a => a.StartMaximized = v), textColor));

        scroll.Content = stack;
        return scroll;
    }

    private View MakeStartupCheckbox(string label, bool isChecked, Action<bool> onChanged, Color textColor)
    {
        var row = new HorizontalStackLayout { Spacing = 6 };
        var cb = new CheckBox { IsChecked = isChecked };
        cb.CheckedChanged += (s, e) => onChanged(e.Value);
        row.Children.Add(cb);
        row.Children.Add(new Label { Text = label, FontSize = 13, VerticalOptions = LayoutOptions.Center, TextColor = textColor });
        return row;
    }

    // ---------- 网址收藏 ----------
    private View BuildFavoritesPage()
    {
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
            },
            Padding = 12,
        };
        var app = AppState.Repo;
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var textColor = isDark ? Color.FromArgb("#CCCCCC") : Color.FromArgb("#333333");
        var subColor = isDark ? Color.FromArgb("#888888") : Color.FromArgb("#777777");

        var addBtn = new Button
        {
            Text = "＋ " + I18n.I18n.Tr("favAdd"),
            FontSize = 12,
            Padding = new Thickness(12, 6),
            BackgroundColor = Color.FromArgb("#0078D4"),
            TextColor = Colors.White,
        };
        addBtn.Clicked += (s, e) => app.UpdateFavorites(list => list.Add(new UrlFavorite("CCTV", "")));
        Grid.SetRow(addBtn, 0);
        grid.Children.Add(addBtn);

        var scroll = new ScrollView();
        var list = new VerticalStackLayout { Spacing = 0 };
        var favs = app.Favorites.ToList();
        for (int i = 0; i < favs.Count; i++)
        {
            var idx = i;
            var fav = favs[idx];

            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
                Padding = new Thickness(0, 6),
            };
            var textStack = new VerticalStackLayout();
            var nameLabel = new Label { Text = fav.Name, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = textColor };
            var urlLabel = new Label { Text = fav.Url, FontSize = 11, TextColor = subColor, MaxLines = 1 };
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await EditFavorite(idx, fav);
            textStack.GestureRecognizers.Add(tap);
            textStack.Children.Add(nameLabel);
            textStack.Children.Add(urlLabel);
            Grid.SetColumn(textStack, 0);
            row.Children.Add(textStack);

            var copyBtn = new Button
            {
                Text = I18n.I18n.Tr("favCopy"),
                FontSize = 11,
                Padding = new Thickness(8, 4),
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#0078D4"),
            };
            copyBtn.Clicked += async (s, e) =>
            {
                await Clipboard.SetTextAsync(fav.Url);
                await DisplayAlert("", I18n.I18n.Tr("copied"), I18n.I18n.Tr("ok"));
            };
            Grid.SetColumn(copyBtn, 1);
            row.Children.Add(copyBtn);

            var delBtn = new Button
            {
                Text = "🗑",
                FontSize = 12,
                Padding = new Thickness(8, 4),
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Red,
            };
            delBtn.Clicked += (s, e) =>
                app.UpdateFavorites(l => { if (idx < l.Count) l.RemoveAt(idx); });
            Grid.SetColumn(delBtn, 2);
            row.Children.Add(delBtn);

            list.Children.Add(row);
            list.Children.Add(new BoxView { HeightRequest = 1, Color = isDark ? Color.FromArgb("#3F3F46") : Color.FromArgb("#D0D0D0") });
        }
        scroll.Content = list;
        Grid.SetRow(scroll, 1);
        grid.Children.Add(scroll);
        return grid;
    }

    private async Task EditFavorite(int idx, UrlFavorite fav)
    {
        var name = await DisplayPromptAsync(I18n.I18n.Tr("edit"), I18n.I18n.Tr("favName"), initialValue: fav.Name);
        if (name == null) return;
        var url = await DisplayPromptAsync(I18n.I18n.Tr("edit"), I18n.I18n.Tr("favUrl"), initialValue: fav.Url);
        if (url == null) return;
        AppState.Repo.UpdateFavorites(list =>
        {
            if (idx < list.Count) list[idx] = new UrlFavorite(name.Trim(), url.Trim());
        });
    }

    private void OnBackClicked(object? sender, EventArgs e) =>
        Shell.Current.GoToAsync("..");
}
