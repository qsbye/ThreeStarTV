namespace ThreeStarsTV;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		AppState.Init();
		MainPage = new AppShell();
	}
}
