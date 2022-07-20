﻿
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
            Application.Current.UserAppTheme = AppTheme.Dark;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        }

        MainPage = new AppShell();
    }
}
