namespace ThreeStarsTV.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        TitleLabel.Text = I18n.I18n.Tr("about");
        AppTitleLabel.Text = I18n.I18n.Tr("appTitle");
        VersionLabel.Text = I18n.I18n.Tr("version") + " " + AppInfo.VersionString;
        DescLabel.Text = I18n.I18n.Tr("aboutDesc");
        TechLabel.Text = I18n.I18n.Tr("aboutTech");
        RepoBtn.Text = I18n.I18n.Tr("aboutRepo");
        CloseBtn.Text = I18n.I18n.Tr("aboutClose");
    }

    private void OnBackClicked(object? sender, EventArgs e) =>
        Shell.Current.GoToAsync("..");

    private async void OnRepoClicked(object? sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://github.com/qsbye/ThreeStarTV");
    }
}
