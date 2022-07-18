using GridGames.ViewModels;
using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AppSettingsPage : ContentPage
	{
        private bool currentShowDarkTheme;

        public AppSettingsPage ()
		{
			InitializeComponent ();

            this.BindingContext = new AppSettingsViewModel();

            var vm = this.BindingContext as AppSettingsViewModel;
            vm.ShowDarkTheme = Preferences.Get("ShowDarkTheme", false);

            currentShowDarkTheme = vm.ShowDarkTheme;
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();

            // Create a new AppShell here to force the new theme colours to be shown
            // on the hamburger button.

            var vm = this.BindingContext as AppSettingsViewModel;
            if (currentShowDarkTheme != vm.ShowDarkTheme)
            {
                // Future: This seems a little heavy-handed just to get the colours
                // to refresh in the app shell. Investigate whether there's a simpler
                // way to achieve the results.
                App.Current.MainPage = new AppShell();
            }
        }
    }
}