# 更新日志 / CHANGELOG

> 记录规则：按**小时**记录当日修改，每条注明涉及文件与打包产物，时间取自文件修改时间与构建产物时间戳。
> 版本号规则：`年.月.日`（如 26.9.17 = 2026-09-17）。

---

## 26.9.20　版本 26.9.20.20

### 19 时
- 网址收藏列表 + README 重写（提交 `c5f273b`）
  - [ThreeStarTV/ConfigService.cs](ThreeStarTV/ConfigService.cs)：新增 `UrlFavorite` 与 `UrlFavorites` 列表，持久化到 `UrlFavorites.json`；首次启动默认内置 CCTV-1~13 网页地址 + CCTV-1 m3u8 推流地址
  - [ThreeStarTV/SettingsWindow.cs](ThreeStarTV/SettingsWindow.cs)：设置窗口新增"网址收藏"标签页——表格双击编辑名称/网址、"复制"按钮复制到剪贴板、"删除"按钮、"添加网址"按钮；语言切换同步刷新
  - [ThreeStarTV/I18n.cs](ThreeStarTV/I18n.cs)：新增 favUrls/favAdd/favName/favUrl/favCopy 中英文案
  - README.md：重写为 ThreeStarTV 电视直播播放器定位（启动即播/回退全屏/启动设置/加载提速/网址收藏/构建部署）
  - 验证：Release 构建 0 错误
- 央视网回退页自动全屏（提交 `457a40a`）：CameraCell 注入 `AutoFullscreenScript`（AddScriptToExecuteOnDocumentCreated，仅 cctv.com 域名生效），每秒扫描 DOM 中 title/class/id 含"全屏/full"的可见元素，出现即点击一次后停止；Release 构建 0 错误
- 加载提速与进度显示（提交 `203c3e8`）
  - [ThreeStarTV/App.cs](ThreeStarTV/App.cs)：启动时后台预热预取——提前请求 m3u8 清单与央视网回退页，完成 DNS 解析与 TLS 握手（系统 DNS 缓存 Chromium 可复用）
  - [ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs)：全部单元格共享一个 WebView2 环境（静态 `GetSharedEnvAsync`），默认用户数据目录持久化——央视网页面依赖（JS/CSS）第二次启动起走本地 HTTP 缓存，不再重复下载；`--autoplay-policy` 参数移入共享环境
  - [ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs)：新增加载遮罩——暗色遮罩 + 进度条 + "加载中 xxx ms" 实时计时（100ms 刷新，伪进度条按用时推进）；m3u8 页面等播放器 `playing` 事件（网页 postMessage 通知）后隐藏，普通网页/央视网回退页导航完成即隐藏，60s 超时兜底；刷新按钮同样显示
  - [ThreeStarTV/I18n.cs](ThreeStarTV/I18n.cs)：新增 loading 中英文案
  - 验证：Release 构建 0 错误
- 设置窗口隐藏 JOBX 备份标签页（提交 `64d39c1`）：`SettingsWindow` 用 `showJobxTab = false` 开关控制，BuildJobxPage 及全部 JOBX 功能代码保留，改回 true 即恢复；语言切换的页签名循环带长度保护不受影响
- 关于页面仓库链接改为 github.com/qsbye/ThreeStarTV（提交 `463bc59`）
- 关于页面描述更新（提交 `d688eaf`）：aboutDesc/aboutTech 改为电视直播播放器定位（CCTV-1 直播、hls.js 内嵌、央视网回退、开机启动/自动取消静音/置顶等），中英文同步；Release 构建 0 错误
- m3u8 播放失败回退央视网网页（提交 `3957490`）
  - [ThreeStarTV/ConfigService.cs](ThreeStarTV/ConfigService.cs)：新增 `FallbackWebUrl` 常量（`https://tv.cctv.com/live/cctv1/`）
  - [ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs)：hls.js 播放器致命错误/视频错误时重试 2 次（间隔 3 秒），第 3 次仍失败则 `location.replace` 跳转到央视网 CCTV-1 网页播放器作为回退
  - 验证：Release 构建 0 错误
- 新增启动设置：自动取消静音/开机启动/窗口置顶/启动最大化；hls.js 内嵌（提交 `d7bf3f1`）
  - [ThreeStarTV/Assets/hls.min.js](ThreeStarTV/Assets/hls.min.js)：hls.js@1 内嵌为嵌入资源（约 600KB），m3u8 播放器页不再依赖 CDN；[ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs) 统一 `LoadEmbeddedScript()` 加载嵌入脚本
  - [ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs)：WebView2 环境增加 `--autoplay-policy=no-user-gesture-required`，配合 AppConfig.autoUnmute（默认 true）实现开机即有声音播放
  - [ThreeStarTV/ConfigService.cs](ThreeStarTV/ConfigService.cs)：AppConfig 新增 autoUnmute/topMost/startMaximized/autoStart 四个持久化字段
  - [ThreeStarTV/AutoStartService.cs](ThreeStarTV/AutoStartService.cs)（新增）：开机启动通过 WScript.Shell 在 shell:startup 创建 ThreeStarTV.lnk；开启时仅当不存在才创建，关闭时仅当快捷方式指向本程序才删除，不碰用户其他快捷方式；启动时 SyncWithConfig 自动修复状态不一致
  - [ThreeStarTV/SettingsWindow.cs](ThreeStarTV/SettingsWindow.cs)：软件设置页新增"启动设置"区，4 个复选框（自动取消静音/开机启动/窗口置顶/启动最大化），窗口置顶即时生效；[ThreeStarTV/I18n.cs](ThreeStarTV/I18n.cs) 新增对应中英文案
  - [ThreeStarTV/MainWindow.cs](ThreeStarTV/MainWindow.cs)：启动时应用 startMaximized/topMost；[ThreeStarTV/App.cs](ThreeStarTV/App.cs)：启动时同步开机启动快捷方式
  - 验证：Release 构建 0 错误
- 启动即播放 CCTV-1 直播流（提交 `a250f19`）
  - [ThreeStarTV/ConfigService.cs](ThreeStarTV/ConfigService.cs)：新增 `DefaultStreamUrl` 常量（`https://ldncctvwbcdbd.a.bdydns.com/ldncctvwbcd/cdrmldcctv1_1/index.m3u8`）；首次启动（无 CameraConfig.json）时自动生成默认配置：1 路、地址为该直播流、备注 CCTV-1、无加载延迟
  - [ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs)：`Navigate()` 检测到 `.m3u8` 地址时改用 hls.js 播放器页（NavigateToString 内嵌 HTML，CDN 引入 hls.js@1，静音自动播放，致命错误/播放错误 3 秒后自动重载重试）；普通 URL 仍直接 Navigate
  - 验证：Release 构建 0 错误
  - 注意： muted 静音自动播放（浏览器自动播放策略要求）；已存在旧配置的用户不会覆盖现有配置
- 重命名软件为 **ThreeStarTV**，应用图标替换为 threestar.png（新增 `assets/threestar.ico`，由 `threestar.png` 转换生成）
  - 主程序：`ThreeStarTV/ThreeStarTV.csproj`（AssemblyName/Product）、`ThreeStarTV/I18n.cs`（appTitle 中英文）、`ThreeStarTV/ConfigService.cs`（配置目录 `%LocalAppData%\ThreeStarTV`）、`ThreeStarTV/app.manifest`（程序集标识）、`ThreeStarTV/App.cs`（崩溃提示标题）
  - 启动器：`ThreeStarTV.Launcher/Program.cs`（内嵌资源名、安装路径、进程名、提示文字）、`ThreeStarTV.Launcher/ThreeStarTV.Launcher.csproj`（内嵌 ThreeStarTV.exe）、`ThreeStarTV.Launcher/LauncherForm.Designer.cs`（窗口标题）
  - 打包：`build-with-timestamp.sh` 输出文件名改为 `ThreeStarTV_<时间戳>.exe`
- 验证：主程序 Release 构建通过（0 错误）；本次为源码修改，未生成新的打包产物
- Git 提交：`e4daec0`

---

## 2026-09-17　版本 26.9.17.18

### 重构：UI 框架从 WPF(HandyControls) 迁移到 WinForms + AntdUI

- [ThreeStarTV/MainWindow.cs](ThreeStarTV/MainWindow.cs)、[ThreeStarTV/SettingsWindow.cs](ThreeStarTV/SettingsWindow.cs)、[ThreeStarTV/AboutWindow.cs](ThreeStarTV/AboutWindow.cs)：修复标题栏缺失导致的元素重叠——AntdUI.Window 本身不绘制标题栏，需添加 `AntdUI.PageHeader` 作为标题栏（`ShowButton=true` 显示最小化/最大化/关闭按钮，`DragMove` 默认支持拖动窗口）；主窗口工具栏标题文字移入 PageHeader，功能按钮靠右停靠。
- [ThreeStarTV/ThreeStarTV.csproj](ThreeStarTV/ThreeStarTV.csproj)：移除 `UseWPF` 与 `HandyControls 3.7.0`，新增 `AntdUI 2.4.10`；新增 `RemoveWebView2WpfReference` Target（移除 WebView2 包对 net5.0+ 无条件引用的 `Microsoft.Web.WebView2.Wpf.dll`，消除 MSB3277 WindowsBase 版本冲突警告）。
- [ThreeStarTV/App.cs](ThreeStarTV/App.cs)：WPF Application 改为 WinForms `[STAThread] Main` 入口；保留三处全局异常处理（UI 线程 / 非UI线程 / 未观察 Task 异常）。
- [ThreeStarTV/ThemeManager.cs](ThreeStarTV/ThemeManager.cs)：改为设置 `AntdUI.Config.Mode` + `System.Drawing.Color` 调色板（VS Code 配色不变），保留 `ThemeChanged` 事件。
- [ThreeStarTV/MainWindow.cs](ThreeStarTV/MainWindow.cs)：继承 `AntdUI.Window`（无边框窗口）；工具栏/状态栏/托盘/网格重排布局（TableLayoutPanel），相机网格 1/2/4/6/9/16 布局与错峰加载逻辑不变。
- [ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs)：每路格子改用 WinForms WebView2 控件 + AntdUI Input/Button；HMI 语言跟随注入逻辑（`hmi-i18n.js` 握手协议）原样保留。
- [ThreeStarTV/SettingsWindow.cs](ThreeStarTV/SettingsWindow.cs)：改为 `AntdUI.Window` + Tabs；JOBX 相机表格从 WPF DataGrid 改为 AntdUI Table（单元格编辑/勾选/按钮事件）；备份目录选择仍用 WinForms `FolderBrowserDialog`。
- [ThreeStarTV/AboutWindow.cs](ThreeStarTV/AboutWindow.cs)：改为 `AntdUI.Window`。
- **修复 JOBX "添加相机"无反应**：数据实际已写入配置但表格不刷新——AntdUI.Table 数据绑定要求行模型为 public 类 + public 属性，`JobxRow` 由 private 嵌套类 + 字段改为 public 类 + 属性后表格正常显示/刷新。
- **修复 JOBX "备份"/"选择"（文件夹）按钮无反应**：AntdUI.Table 行索引为 **1-based**（`_rows[0]` 是表头行，`SelectedIndex` 亦从 1 开始），旧代码按 0-based 语义处理导致整体 off-by-one——"备份"按钮把 `SelectedIndex` 直接传给 0-based 的 `BackupCamera(index)` 造成越界（返回失败结果且不写日志，Task 异常又被全局 `UnobservedTaskException` 静默吞掉，表现"无反应"）；表格行内"选择/删除"按钮（`CellButtonClick`）、单元格编辑（`CellEndEdit`）、勾选（`CheckedChanged`）同样偏移失效。统一改为 1-based 语义（`SelectedIndex-1` / `e.RowIndex-1`，CellButtonClick 优先用 `e.Record` 匹配行对象）；备份 Task 内增加 try/catch 写 ERROR 日志避免静默失败。
- **按钮浅蓝色描边**：所有 `TTypeMini.Default` 按钮（工具栏、相机格、JOBX 备份页）统一 `DefaultBorderColor=#91CAFF` + `BorderWidth=1`（AntdUI 2.4.10 无 `BorderColor` 属性，属性名为 `DefaultBorderColor`）；[ThemeManager.cs](ThreeStarTV/ThemeManager.cs) 新增 `BtnBorder` 调色板项。
- **修复页面内容残缺**：AntdUI.Radio 默认 `AutoSizeMode=None` 导致 FlowLayoutPanel 测量高度为 0（显示设置/软件设置页单选按钮不显示）→ 显式 `AutoSizeMode=TAutoSize.Auto`；AntdUI.Label 长文本 AutoSize 测量不准导致关于页版本号/描述/技术栈截断 → 改用原生 WinForms `Label`（AutoSizing+MaximumSize 换行可靠）；JOBX 表格空数据时 `EmptyHeader=true` 显示列头；关于页版本号截断 commit 哈希（`InformationalVersion` 去除 `+` 后缀）。
- [ThreeStarTV/I18n.cs](ThreeStarTV/I18n.cs)：关于页技术栈文案更新为 "WinForms + AntdUI (.NET 8) + WebView2 + FluentFTP"。
- [README.md](README.md)：技术栈描述 WPF/HandyControls → WinForms/AntdUI。
- 版本号 26.9.17.17 → 26.9.17.18。

---

## 2026-09-17　版本 26.9.17.17

### 修复：JOBX 备份选择目录后 DataGrid 报错

- [ThreeStarTV/SettingsWindow.cs](ThreeStarTV/SettingsWindow.cs)：新增 `SafeJobxRefresh()` 方法，先 `CommitEdit` + `CancelEdit` 退出编辑事务再 `Items.Refresh()`；添加/选择目录/删除三处调用全部替换，解决"在 AddNew 或 EditItem 事务过程中不允许 Refresh"报错。

### 改进：URL 与备注同行布局

- [ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs)：网格从 3 行（URL+按钮/备注/WebView）改为 2 行（URL+备注+按钮/WebView）；`top` 网格 5 列：URL(`2*`) | 备注(`1*`) | 锁定 | 刷新 | 最大化，减少垂直空间占用。

### 其它

- 版本号 26.9.17.16 → 26.9.17.17。

---

## 2026-09-17　版本 26.9.17.16

- 新增 [ThreeStarTV/JobxBackupService.cs](ThreeStarTV/JobxBackupService.cs)：基于 FluentFTP 50.1.0 的作业文件备份服务，支持 FTP/FTPS（FTPS 默认启用，实际必须启用 FTPS 才能正常备份 .jobx 文件）+ 信任自签证书；递归下载 `.jobx`/`.jobx.sig`；环形日志（最近 500 条，INFO/WARN/ERROR 分级）。
- [ThreeStarTV/SettingsWindow.cs](ThreeStarTV/SettingsWindow.cs)：新增「JOBX备份」标签页，DataGrid 配置相机（名称/IP/端口/用户名/密码/备份目录/FTPS/信任证书），添加/删除/备份/全部备份/打开目录按钮，日志区 DispatcherTimer 每秒刷新。
- [ThreeStarTV/ConfigService.cs](ThreeStarTV/ConfigService.cs)：新增 `JobxCameraConfig`（`ftps_enabled` 默认 `true`）、`JobxBackupConfig`，加载/保存 `JobxBackupConfig.json`。
- 备份目录列新增「选择」按钮（`DataGridTemplateColumn` + `System.Windows.Forms.FolderBrowserDialog`），点击浏览选择文件夹（WPF 窗口句柄作为 owner，try-catch 防崩溃），写回配置并刷新。初版用 `Microsoft.Win32.OpenFolderDialog`（.NET 8 新增）运行时闪退，改用久经考验的 WinForms 对话框。
- 依赖：[ThreeStarTV/ThreeStarTV.csproj](ThreeStarTV/ThreeStarTV.csproj) 新增 `FluentFTP 50.1.0`。

### 新功能：HMI 语言跟随（WebView2 注入脚本）

- 新增 [ThreeStarTV/Assets/hmi-i18n.js](ThreeStarTV/Assets/hmi-i18n.js)（`EmbeddedResource` 编译期烘焙，不新增运行时资源依赖）：
  - 路径匹配 `/pages/hmi/` 时激活（含顶层 frame 与嵌套 iframe）；默认英文（原文），宿主通过 `chrome.webview` 消息通知语言切换。
  - 只做整串精确匹配替换（字典 ~80 词），不做子串替换，避免破坏作业数据/文件名/数值；MutationObserver 批处理（16ms 合并）框架后续动态渲染；切回英文时按记录原文完整还原。
  - 宿主（C# 侧）协议：子→宿主 `{__hmiI18n:"ready"}`，宿主→子 `{__hmiI18n:"lang",lang:"zh"|"en"}`。
- [ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs)：`CoreWebView2InitializationCompleted` 改为 async，注入脚本（`AddScriptToExecuteOnDocumentCreatedAsync`）→ 挂载 `WebMessageReceived` → 导航；收到 `ready` 握手立即 `PostWebMessageAsJson` 下发当前语言；`OnLanguageChanged` 末尾调用 `PostHmiLang` 实时通知。
- **修复 .NET 版架构适配**：.NET 版每个 CameraCell 的 WebView2 顶层 frame 直接就是相机 HMI 页面（无父页面），移除 `if (window.self === window.top) return;`（Tauri 版保留该检查因顶层 frame 是软件 UI）；通信改为 `chrome.webview` 通道（Tauri 版用标准 `window.postMessage` 父子 frame 通信）。

### 新功能：关于页面

- 新增 [ThreeStarTV/AboutWindow.cs](ThreeStarTV/AboutWindow.cs)：显示应用名/版本号（`AssemblyInformationalVersionAttribute`）/功能描述/技术栈/GitHub 仓库链接（`Hyperlink` + `Process.Start`），关闭按钮，跟随主题色。
- [ThreeStarTV/MainWindow.cs](ThreeStarTV/MainWindow.cs)：工具栏新增「关于」按钮（`AntIcon.InfoCircle` 图标），语言切换时同步刷新。
- [ThreeStarTV/AntIcon.cs](ThreeStarTV/AntIcon.cs)：新增 `InfoCircle` 图标常量。
- [ThreeStarTV/I18n.cs](ThreeStarTV/I18n.cs)：新增 `about`/`aboutDesc`/`aboutTech`/`aboutRepo`/`aboutClose` 中英文键。

### 改进：网格布局 12 → 16

- [ThreeStarTV/MainWindow.cs](ThreeStarTV/MainWindow.cs)：`Layouts` 新增 `[16] = (4, 4)` 预设。
- [ThreeStarTV/ConfigService.cs](ThreeStarTV/ConfigService.cs)：相机数量上限 12 → 16。
- [ThreeStarTV/SettingsWindow.cs](ThreeStarTV/SettingsWindow.cs)：数量选项增加 16。

### 改进：URL/备注输入框水印提示

- [ThreeStarTV/CameraCell.cs](ThreeStarTV/CameraCell.cs)：`AttachPlaceholder` 方法用 `VisualBrush` 实现水印（空内容显示占位文本），URL 框 `192.168.1.10 或 http://...`，备注框 `备注...`；语言切换时同步刷新占位文本。
- [ThreeStarTV/I18n.cs](ThreeStarTV/I18n.cs)：新增 `urlPlaceholder`/`remarkPlaceholder` 中英文键。

### 修复：崩溃产生僵尸进程导致无法重启

- [ThreeStarTV/App.cs](ThreeStarTV/App.cs)：新增全局未处理异常处理（`DispatcherUnhandledException` + `AppDomain.UnhandledException` + `TaskScheduler.UnobservedTaskException`），崩溃时弹错误框后 `Shutdown()` + `Environment.Exit(1)` 优雅退出，释放文件锁，避免产生僵尸进程。
- [ThreeStarTV.Launcher/Program.cs](ThreeStarTV.Launcher/Program.cs)：`ExtractResource` 的 `File.Delete` 改为 `TryDeleteFile`——若 exe 被占用则先 `Process.Kill()` 结束残留 `ThreeStarTV` 进程再重试删除（最多 4 次，间隔 500ms），解决崩溃后重启提示"访问被拒绝"的问题。

### 其它

- [ThreeStarTV/I18n.cs](ThreeStarTV/I18n.cs)：新增 `select`/`selectBackupDir`/`error` 中英文键。
- 版本号 26.9.17.14 → 26.9.17.16。
- 验证：`dotnet build`（主程序）+ `MSBuild`（Launcher）0 警告 0 错误。

---

## 2026-09-17　版本 26.9.17.14（初始提交 a1b5087）

- ThreeStarTV：纯 C# WPF + HandyControls 相机 HMI 查看器，.NET 4 启动器内嵌 .NET 8 运行时，单文件打包。
- 1–12 路相机网格布局、错峰加载、锁定/刷新/最大化、主题切换、中英文、系统托盘、配置持久化。
