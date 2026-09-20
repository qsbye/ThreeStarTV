using System;
using System.IO;
using System.Windows.Forms;

namespace ThreeStarTV
{
    /// <summary>开机启动：在 shell:startup 目录维护本程序的快捷方式。开启时仅当不存在才创建；关闭时仅当快捷方式指向本程序时才删除，避免误删用户自己的快捷方式。</summary>
    public static class AutoStartService
    {
        private const string ShortcutName = "ThreeStarTV.lnk";

        private static string StartupDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));

        private static string ShortcutPath => Path.Combine(StartupDir, ShortcutName);

        /// <summary>快捷方式当前是否有效存在且指向本程序。</summary>
        public static bool IsEnabled()
        {
            try
            {
                if (!File.Exists(ShortcutPath)) return false;
                return string.Equals(GetShortcutTarget(ShortcutPath),
                    Application.ExecutablePath, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        /// <summary>确保开机启动状态与配置一致（启动时调用，修复被手动改动的情况）。</summary>
        public static void SyncWithConfig()
        {
            if (ConfigService.App.autoStart) Enable(); else Disable();
        }

        public static void Enable()
        {
            try
            {
                if (IsEnabled()) return; // 已存在且指向本程序，不重复创建
                CreateShortcut(ShortcutPath, Application.ExecutablePath);
            }
            catch { }
        }

        public static void Disable()
        {
            try
            {
                if (!File.Exists(ShortcutPath)) return;
                // 只删除指向本程序的快捷方式，用户自建的其他快捷方式不动
                if (string.Equals(GetShortcutTarget(ShortcutPath),
                    Application.ExecutablePath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(ShortcutPath);
                }
            }
            catch { }
        }

        private static void CreateShortcut(string lnkPath, string target)
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic shell = Activator.CreateInstance(shellType);
            try
            {
                var shortcut = shell.CreateShortcut(lnkPath);
                shortcut.TargetPath = target;
                shortcut.WorkingDirectory = Path.GetDirectoryName(target);
                shortcut.Save();
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
            }
        }

        private static string GetShortcutTarget(string lnkPath)
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return null;
            dynamic shell = Activator.CreateInstance(shellType);
            try
            {
                var shortcut = shell.CreateShortcut(lnkPath);
                return shortcut.TargetPath as string;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
