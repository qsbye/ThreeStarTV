namespace ThreeStarsTV;

/// <summary>应用级单例状态：配置仓库 + 当前语言/主题。</summary>
public static class AppState
{
    private static bool _initialized;
    private static Data.ConfigRepository? _repo;

    public static Data.ConfigRepository Repo => _repo ?? throw new InvalidOperationException("AppState not initialized");

    public static void Init()
    {
        if (_initialized) return;
        _initialized = true;
        _repo = new Data.ConfigRepository();
        I18n.I18n.Instance.Language = _repo.App.Language;
    }
}
