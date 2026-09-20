using System;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using ThreeStarTV;
using Panel = System.Windows.Forms.Panel;

namespace ThreeStarTV
{
    public class CameraCell : Panel
    {
        private readonly int id;
        private readonly AntdUI.Input urlBox;
        private readonly AntdUI.Input remarkBox;
        private readonly AntdUI.Button lockBtn;
        private readonly AntdUI.Button refreshBtn;
        private readonly AntdUI.Button maxBtn;
        private readonly Panel top;
        private readonly Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private readonly Panel videoHost;
        private readonly Panel loadingPanel;
        private readonly System.Windows.Forms.Label loadingLabel;
        private readonly AntdUI.Progress loadingBar;
        private readonly System.Windows.Forms.Timer loadingTimer;
        private readonly System.Diagnostics.Stopwatch loadingSw = new System.Diagnostics.Stopwatch();
        private bool waitingVideo; // true=等待视频起播（m3u8），false=页面加载完即隐藏
        private static Microsoft.Web.WebView2.Core.CoreWebView2Environment sharedEnv;
        private bool webViewReady;
        private bool isMaximized;
        private static string hmiI18nScript;
        private static string hlsJsScript;

        public event Action<CameraCell> ToggleMaximize;

        // 全单元格共享同一 WebView2 环境：复用持久缓存（央视网等网页依赖二次启动走本地缓存），并省去重复创建开销
        private static async System.Threading.Tasks.Task<Microsoft.Web.WebView2.Core.CoreWebView2Environment> GetSharedEnvAsync()
        {
            if (sharedEnv == null)
            {
                var options = new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions(
                    additionalBrowserArguments: "--autoplay-policy=no-user-gesture-required");
                sharedEnv = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, null, options);
            }
            return sharedEnv;
        }

        private static string LoadEmbeddedScript(string suffix)
        {
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                foreach (var name in asm.GetManifestResourceNames())
                {
                    if (name.EndsWith(suffix))
                    {
                        using (var s = asm.GetManifestResourceStream(name))
                        using (var r = new System.IO.StreamReader(s, System.Text.Encoding.UTF8))
                            return r.ReadToEnd();
                    }
                }
            }
            catch { }
            return "";
        }

        private static string LoadHmiI18nScript()
        {
            if (hmiI18nScript == null) hmiI18nScript = LoadEmbeddedScript("hmi-i18n.js");
            return hmiI18nScript;
        }

        private static string LoadHlsJsScript()
        {
            if (hlsJsScript == null) hlsJsScript = LoadEmbeddedScript("hls.min.js");
            return hlsJsScript;
        }

        // 央视网回退页自动点"全屏"：仅对 cctv.com 生效，按钮渲染出来后每秒尝试一次直至点击成功
        private const string AutoFullscreenScript = @"
(function(){
  if (/cctv\.com$|\.cctv\.com$/.test(location.hostname)) {
    var timer = setInterval(function(){
      try {
        var els = document.querySelectorAll('[title*=""全屏""], [class*=""full""], [id*=""full""]');
        for (var i = 0; i < els.length; i++) {
          var b = els[i], t = (b.title || '') + ' ' + (b.className || '') + ' ' + (b.id || '');
          if (/全屏|full/i.test(t) && b.offsetParent !== null) { b.click(); clearInterval(timer); return; }
        }
      } catch (e) {}
    }, 1000);
  }
})();";

        private void PostHmiLang()
        {
            if (!webViewReady || webView.CoreWebView2 == null) return;
            try
            {
                webView.CoreWebView2.PostWebMessageAsJson(
                    "{\"__hmiI18n\":\"lang\",\"lang\":\"" + I18n.Language + "\"}");
            }
            catch { }
        }

        private void OnWebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson;
                if (json == null) return;
                if (json.Contains("__hmiI18n") && json.Contains("ready"))
                    PostHmiLang();
                // 播放器起播后隐藏加载遮罩
                if (json.Contains("__player") && json.Contains("playing"))
                    HideLoading();
            }
            catch { }
        }

        private void OnNavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            // 回退到央视网等真实网页时 Source 为 http(s)，页面加载完成即隐藏；
            // m3u8 内嵌播放器页 Source 为 about:blank，继续等起播消息
            try
            {
                var src = webView.CoreWebView2?.Source ?? "";
                if (!waitingVideo || src.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    HideLoading();
            }
            catch { HideLoading(); }
        }

        private void ShowLoading(bool waitVideo)
        {
            waitingVideo = waitVideo;
            loadingSw.Restart();
            loadingBar.Value = 0;
            loadingLabel.Text = I18n.T("loading");
            loadingPanel.Visible = true;
            loadingPanel.BringToFront();
            loadingTimer.Start();
        }

        private void HideLoading()
        {
            loadingTimer.Stop();
            loadingSw.Stop();
            loadingPanel.Visible = false;
        }

        public CameraCell(int id)
        {
            this.id = id;
            var item = ConfigService.EnsureItem(id);

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            BackColor = ThemeManager.Bg2;
            Padding = new Padding(1);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = ThemeManager.Bg2 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            top = new Panel { Dock = DockStyle.Fill, Height = 30, BackColor = ThemeManager.Bg2 };
            var topGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, BackColor = ThemeManager.Bg2, Margin = new Padding(3, 3, 3, 1) };
            topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            urlBox = new AntdUI.Input
            {
                Dock = DockStyle.Fill,
                Text = item.ip,
                ReadOnly = item.locked,
                PlaceholderText = I18n.T("urlPlaceholder"),
            };
            urlBox.LostFocus += (s, e) => CommitUrl();
            urlBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { CommitUrl(); e.SuppressKeyPress = true; }
            };
            topGrid.Controls.Add(urlBox, 0, 0);

            remarkBox = new AntdUI.Input
            {
                Dock = DockStyle.Fill,
                Text = item.remark,
                ReadOnly = item.locked,
                PlaceholderText = I18n.T("remarkPlaceholder"),
                Margin = new Padding(4, 0, 0, 0),
            };
            remarkBox.LostFocus += (s, e) =>
            {
                var it = ConfigService.EnsureItem(id);
                it.remark = remarkBox.Text;
                ConfigService.SaveCamera();
            };
            topGrid.Controls.Add(remarkBox, 1, 0);

            lockBtn = new AntdUI.Button { Text = "", IconSvg = "", Width = 78, Height = 26, Margin = new Padding(4, 0, 0, 0), DefaultBorderColor = ThemeManager.BtnBorder, BorderWidth = 1 };
            lockBtn.Click += (s, e) => ToggleLock();
            topGrid.Controls.Add(lockBtn, 2, 0);

            refreshBtn = new AntdUI.Button { Text = I18n.T("refresh"), IconSvg = AntIcon.Svg(AntIcon.Reload), Width = 78, Height = 26, Margin = new Padding(4, 0, 0, 0), DefaultBorderColor = ThemeManager.BtnBorder, BorderWidth = 1 };
            refreshBtn.Click += async (s, e) => await ReloadAsync();
            topGrid.Controls.Add(refreshBtn, 3, 0);

            maxBtn = new AntdUI.Button { Text = "", IconSvg = "", Width = 78, Height = 26, Margin = new Padding(4, 0, 0, 0), DefaultBorderColor = ThemeManager.BtnBorder, BorderWidth = 1 };
            maxBtn.Click += (s, e) =>
            {
                isMaximized = !isMaximized;
                UpdateMaxBtn();
                ToggleMaximize?.Invoke(this);
            };
            topGrid.Controls.Add(maxBtn, 4, 0);
            UpdateLockBtn();
            UpdateMaxBtn();

            top.Controls.Add(topGrid);

            webView = new Microsoft.Web.WebView2.WinForms.WebView2 { Dock = DockStyle.Fill };
            webView.CoreWebView2InitializationCompleted += async (s, e) =>
            {
                if (e.IsSuccess)
                {
                    webViewReady = true;
                    webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                    webView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
                    var script = LoadHmiI18nScript();
                    if (!string.IsNullOrEmpty(script))
                    {
                        try { await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script); } catch { }
                    }
                    try { await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(AutoFullscreenScript); } catch { }
                }
                Navigate();
            };

            // 加载遮罩：显示加载进度与用时(ms)，覆盖在视频区上方
            loadingLabel = new System.Windows.Forms.Label
            {
                Text = I18n.T("loading"),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Microsoft YaHei UI", 9f),
            };
            loadingBar = new AntdUI.Progress { Width = 220, Height = 6 };
            loadingPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(235, 17, 18, 23), Visible = false };
            var loadCenter = new TableLayoutPanel { Dock = DockStyle.None, AutoSize = true, BackColor = Color.Transparent };
            loadCenter.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            loadCenter.Controls.Add(loadingLabel, 0, 0);
            loadCenter.Controls.Add(loadingBar, 0, 1);
            loadingPanel.Controls.Add(loadCenter);
            loadingPanel.Resize += (s, e) =>
            {
                loadCenter.Location = new Point((loadingPanel.Width - loadCenter.Width) / 2, (loadingPanel.Height - loadCenter.Height) / 2);
            };
            loadingTimer = new System.Windows.Forms.Timer { Interval = 100 };
            loadingTimer.Tick += (s, e) =>
            {
                var ms = loadingSw.ElapsedMilliseconds;
                loadingLabel.Text = $"{I18n.T("loading")} {ms} ms";
                loadingBar.Value = Math.Min(90, (int)(ms / 100)); // 伪进度，起播后由遮罩隐藏收尾
                // 超过 60s 仍未起播，放弃遮罩避免永久遮挡
                if (ms > 60000) HideLoading();
            };

            videoHost = new Panel { Dock = DockStyle.Fill };
            videoHost.Controls.Add(loadingPanel); // 后加入的遮罩浮于 webView 之上
            videoHost.Controls.Add(webView);

            // Dock 顺序：Fill 先加入，顶部栏后加入
            root.Controls.Add(videoHost, 0, 1);
            root.Controls.Add(top, 0, 0);
            Controls.Add(root);

            I18n.LanguageChanged += OnLanguageChanged;
            ThemeManager.ThemeChanged += ApplyTheme;
            Disposed += (s, e) =>
            {
                I18n.LanguageChanged -= OnLanguageChanged;
                ThemeManager.ThemeChanged -= ApplyTheme;
            };
        }

        private void UpdateLockBtn()
        {
            var locked = ConfigService.EnsureItem(id).locked;
            lockBtn.Text = locked ? I18n.T("unlock") : I18n.T("lock");
            lockBtn.IconSvg = AntIcon.Svg(locked ? AntIcon.Lock : AntIcon.Unlock);
        }

        private void UpdateMaxBtn()
        {
            maxBtn.Text = isMaximized ? I18n.T("restore") : I18n.T("maximize");
            maxBtn.IconSvg = AntIcon.Svg(isMaximized ? AntIcon.Shrink : AntIcon.Expand);
        }

        private void OnLanguageChanged()
        {
            UpdateLockBtn();
            UpdateMaxBtn();
            refreshBtn.Text = I18n.T("refresh");
            refreshBtn.IconSvg = AntIcon.Svg(AntIcon.Reload);
            urlBox.PlaceholderText = I18n.T("urlPlaceholder");
            remarkBox.PlaceholderText = I18n.T("remarkPlaceholder");
            PostHmiLang();
        }

        public void ApplyTheme()
        {
            BackColor = ThemeManager.Bg2;
            top.BackColor = ThemeManager.Bg2;
        }

        private void CommitUrl()
        {
            var item = ConfigService.EnsureItem(id);
            item.ip = urlBox.Text.Trim();
            ConfigService.SaveCamera();
            if (webViewReady && item.locked)
                Navigate();
        }

        private void ToggleLock()
        {
            var item = ConfigService.EnsureItem(id);
            item.locked = !item.locked;
            urlBox.ReadOnly = item.locked;
            remarkBox.ReadOnly = item.locked;
            UpdateLockBtn();
            ConfigService.SaveCamera();
        }

        private void Navigate()
        {
            var item = ConfigService.GetItem(id);
            if (item == null) return;
            var url = (item.ip ?? "").Trim();
            if (url.Length == 0) return;
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                url = "http://" + url;
            try
            {
                if (url.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                {
                    ShowLoading(true); // 等起播消息
                    webView.CoreWebView2?.NavigateToString(BuildHlsPlayerHtml(url));
                }
                else
                {
                    ShowLoading(false); // 页面加载完成即隐藏
                    webView.CoreWebView2?.Navigate(url);
                }
            }
            catch { }
        }

        // WebView2(Chromium) 不支持原生播放 HLS，用内嵌的 hls.js 包装 m3u8 地址（无需外网加载播放器）
        // 直播流持续失败时回退到央视网 CCTV-1 网页播放器
        private static string BuildHlsPlayerHtml(string streamUrl)
        {
            var jsonUrl = System.Text.Json.JsonSerializer.Serialize(streamUrl);
            var jsonFallback = System.Text.Json.JsonSerializer.Serialize(ConfigService.FallbackWebUrl);
            var mutedAttr = ConfigService.App.autoUnmute ? "" : " muted";
            var unmuteJs = ConfigService.App.autoUnmute
                ? "v.addEventListener('canplay', function(){ v.muted = false; });"
                : "";
            return @"<!DOCTYPE html><html><head><meta charset=""utf-8"">
<style>html,body{margin:0;height:100%;background:#000;overflow:hidden}video{width:100%;height:100%}</style>
<script>" + LoadHlsJsScript() + @"</script>
</head><body><video id=""v"" autoplay playsinline" + mutedAttr + @"></video><script>
var url = " + jsonUrl + @";
var fallbackUrl = " + jsonFallback + @";
var retries = 0;
var v = document.getElementById('v');
function fail(){
  retries++;
  if (retries >= 3) { location.replace(fallbackUrl); return; } // 连续失败则回退到央视网网页
  setTimeout(function(){ location.reload(); }, 3000);
}
function start(){
  if (window.Hls && Hls.isSupported()) {
    var h = new Hls({ liveSyncDurationCount: 3 });
    h.loadSource(url); h.attachMedia(v);
    h.on(Hls.Events.ERROR, function(e, data){
      if (data && data.fatal) fail();
    });
  } else if (v.canPlayType('application/vnd.apple.mpegurl')) {
    v.src = url;
  } else {
    fail();
  }
}
" + unmuteJs + @"
v.addEventListener('error', fail);
v.addEventListener('playing', function(){ try { chrome.webview.postMessage({__player:'playing'}); } catch(e){} });
start();
</script></body></html>";
        }

        public async System.Threading.Tasks.Task ReloadAsync()
        {
            if (IsDisposed || Disposing) return;
            if (!webViewReady)
            {
                try
                {
                    var env = await GetSharedEnvAsync();
                    await webView.EnsureCoreWebView2Async(env);
                }
                catch { return; }
            }
            else
            {
                try
                {
                    var cur = (ConfigService.GetItem(id)?.ip ?? "").Trim();
                    ShowLoading(cur.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase));
                    webView.Reload();
                    return;
                }
                catch { }
            }
            Navigate();
        }
    }
}
