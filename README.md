# ThreeStarTV

电视直播播放软件：启动即自动播放 CCTV-1 直播，插上/装上就能放电视。同一套功能提供两个平台版本：

| 版本 | 位置 | 技术 | 分发形态 |
|---|---|---|---|
| **Windows 桌面版** | `ThreeStarTV/`、`ThreeStarTV.Launcher/` | .NET 8 WinForms + WebView2 | 单个便携式 exe（内嵌运行时安装包） |
| **Android 版** | `android/` | Kotlin + Jetpack Compose + Media3 | APK，兼容 Android 10+ |

## 功能

- **启动即播**：打开应用自动播放 CCTV-1，无需任何操作
  - Windows：默认播放 m3u8/HLS 高清流（内嵌 hls.js 播放器，无需联网加载播放器）
  - Android：默认打开央视网 CCTV-1 网页；m3u8 地址则由 Media3 ExoPlayer 原生播放，其他网址由 WebView 加载
- **失败自动回退**：HLS 直播流连续 3 次失败，自动切换到央视网网页播放器（`tv.cctv.com/live/cctv1`），并自动点击网页"全屏"按钮
- **1–16 路播放网格**：每路可填任意网页或 m3u8 地址，支持备注/锁定/刷新/单格最大化，多路错峰加载
- **网格全屏模式**（Android）：隐藏工具栏与状态栏，播放网格铺满整块屏幕，右上角悬浮"退出全屏"按钮；状态持久化，重启后保持
- **傻瓜式播放设置**（全部持久化）：
  - 自动取消静音（默认开，开机即有声音）
  - 开机启动（Windows：shell:startup 快捷方式；Android：`BOOT_COMPLETED` 自启）
  - 窗口置顶（Windows）/ 屏幕常亮（Android）
  - 启动时最大化 / 沉浸式全屏
- **加载提速**：启动后台预热预取（DNS/TLS 握手前置）；加载遮罩实时显示"加载中 xxx ms"与进度条；Windows 端全单元格共享 WebView2 环境，央视网页面二次启动走本地缓存
- **网址收藏**：默认内置 CCTV-1~13 网页地址与 m3u8 推流地址，支持编辑、复制到剪贴板、添加/删除
- 中英文界面、明亮/暗黑主题；Windows 端关闭时最小化到系统托盘
- 配置以 JSON 持久化，两端字段名一致，配置文件可互导

## 目录结构

```
.
├── ThreeStarTV/            # Windows 主程序（.NET 8 WinForms）
├── ThreeStarTV.Launcher/   # Windows 启动器（.NET Framework 4.8）
├── android/                # Android 版（Kotlin + Compose）
├── assets/                 # 图标与内嵌的 .NET 运行时安装包
└── build-with-timestamp.sh # Windows 单文件打包脚本
```

---

## Windows 桌面版

### 技术栈

| 部分 | 技术 |
|---|---|
| 主程序 `ThreeStarTV/` | .NET 8 WinForms（纯 C#），AntdUI 2.4.10，Microsoft.Web.WebView2，内嵌 hls.js@1 |
| 启动器 `ThreeStarTV.Launcher/` | .NET Framework 4.8，内嵌运行时安装包与主程序 exe 资源 |
| 其他 | FluentFTP（JOBX 备份功能，界面已隐藏保留）、WScript.Shell（开机启动快捷方式） |

启动器工作流程：检测 `Microsoft.WindowsDesktop.App 8.x` → 缺失则静默安装内嵌运行时 → 覆盖释放主程序到 `%LOCALAPPDATA%\ThreeStarTV\App\` 并启动。

### 构建

需要：.NET SDK 8+、MSBuild（Visual Studio 或 .NET Framework 4.8 自带）。

```bash
bash build-with-timestamp.sh
```

产物：`dist/ThreeStarTV_<yyyyMMddHHmm>.exe`（单文件，直接拷贝运行）。

### 部署注意事项

- 主程序依赖 **WebView2 运行时**；部分精简版 Win10 可能缺失，首次运行需联网下载，或另行部署 WebView2 离线包
- 启动器内嵌 .NET 8 运行时安装包，完全离线也能完成运行时安装
- 播放直播流与央视网回退页需要目标机器可访问对应域名
- exe 旁放置 `portable.txt` 即启用便携模式（配置存 exe 目录），否则存 `%LOCALAPPDATA%\ThreeStarTV\`

---

## Android 版

### 兼容性

- **minSdk 29（Android 10）**，targetSdk / compileSdk 36
- 横屏运行（`sensorLandscape`，支持正反横屏自动旋转）
- 测试机：`XPL0219C06017003`（TAS-AN00，Android 12）

### 技术栈

| 部分 | 技术 |
|---|---|
| 语言 / UI | Kotlin 2.3，Jetpack Compose（Compose BOM 2026.03，Material 3） |
| 播放 | AndroidX Media3 1.11（ExoPlayer + HLS 扩展）原生播放 m3u8；WebView 加载网页 |
| 导航 | Navigation3（NavDisplay） |
| 配置 | kotlinx.serialization 读写 JSON，字段名与桌面版一致 |
| 构建 | Gradle + AGP 9，JDK 17 |

### 构建与安装

需要：JDK 17、Android SDK（可用 [android CLI](https://developer.android.com/tools/agents/android-cli?hl=zh-cn) 管理）。

```bash
cd android

# Debug APK
./gradlew :app:assembleDebug

# 安装到已连接设备
adb install -r app/build/outputs/apk/debug/app-debug.apk
```

Release APK：`./gradlew :app:assembleRelease`。

### 运行测试

```bash
./gradlew :app:testDebugUnitTest   # JVM 单元测试：i18n、配置 JSON 字段名兼容
```

### 配置存储

配置位于应用私有目录（`/data/data/com.qsbye.threestartv/files/`，无需任何权限）：

- `CameraConfig.json`：网格数量、错峰延时、每路地址/备注/锁定状态
- `AppConfig.json`：语言、主题、自动取消静音、屏幕常亮、启动全屏、开机自启、网格全屏模式
- `UrlFavorites.json`：网址收藏列表

可通过以下命令查看（调试用）：

```bash
adb shell run-as com.qsbye.threestartv cat files/AppConfig.json
```
