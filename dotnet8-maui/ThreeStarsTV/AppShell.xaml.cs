namespace ThreeStarsTV;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("settings", typeof(Pages.SettingsPage));
		Routing.RegisterRoute("about", typeof(Pages.AboutPage));
	}
}
