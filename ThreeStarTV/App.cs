using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AntdUI;
using ThreeStarTV;

namespace ThreeStarTV
{
    public static class App
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 全局未处理异常处理：崩溃时优雅退出（释放文件锁），避免产生僵尸进程
            Application.ThreadException += (s, args) => CrashExit(args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                CrashExit(args.ExceptionObject as Exception);
            };
            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                args.SetObserved();
            };

            ConfigService.Load();
            I18n.Language = ConfigService.App.language == "en" ? "en" : "zh";
            ThemeManager.Apply(ConfigService.App.theme);
            AutoStartService.SyncWithConfig();
            WarmupPrefetch(); // 后台预热直播流/回退页，提前完成 DNS/TLS 握手

            Application.Run(new MainWindow());
        }

        public static void CrashExit(Exception ex)
        {
            try
            {
                var msg = ex?.ToString() ?? "未知错误 / Unknown error";
                MessageBox.Show(msg, "ThreeStarTV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            Environment.Exit(1);
        }

        /// <summary>启动后台预热：提前请求直播流清单与央视网回退页，完成 DNS 解析与 TLS 握手，缩短首个画面等待。</summary>
        private static void WarmupPrefetch()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                    foreach (var url in new[] { ConfigService.DefaultStreamUrl, ConfigService.FallbackWebUrl })
                    {
                        try
                        {
                            using var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                            req.Headers.UserAgent.ParseAdd("Mozilla/5.0");
                            using var resp = await client.SendAsync(req, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
                            _ = await resp.Content.ReadAsByteArrayAsync(); // 完整读取以复用/关闭连接
                        }
                        catch { /* 预热失败不影响启动 */ }
                    }
                }
                catch { }
            });
        }
    }
}
