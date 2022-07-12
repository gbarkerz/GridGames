using GridGames.ViewModels;
using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WheresGameSettingsPage : ContentPage
    {
        public WheresGameSettingsPage()
        {
            InitializeComponent();

            this.BindingContext = new WheresSettingsViewModel();

            var vm = this.BindingContext as WheresSettingsViewModel;
            vm.PlaySoundOnMatch = Preferences.Get("WheresPlaySoundOnMatch", true);
            vm.PlaySoundOnNotMatch = Preferences.Get("WheresPlaySoundOnNotMatch", true);
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}