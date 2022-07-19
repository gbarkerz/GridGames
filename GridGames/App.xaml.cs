using GridGames.Styles;

namespace GridGames;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();

        var showDarkTheme = Preferences.Get("ShowDarkTheme", false);
        if (showDarkTheme)
        {
            App.Current.Resources = new DarkTheme();
        }
        else
        {
            App.Current.Resources = new LightTheme();
        }

        MainPage = new AppShell();
    }
}
