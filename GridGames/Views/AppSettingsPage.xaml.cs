using GridGames.ViewModels;

namespace GridGames
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AppSettingsPage : ContentPage
	{
        public AppSettingsPage ()
		{
			InitializeComponent ();

            this.BindingContext = new AppSettingsViewModel();

            var vm = this.BindingContext as AppSettingsViewModel;
            vm.ShowDarkTheme = Preferences.Get("ShowDarkTheme", false);
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}