namespace Mizan.App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new ContentPage { Title = "Mizan" };
	}
}
