using GridGames.ResX;
using GridGames.Services;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames.ViewModels
{
    // View model for the Squares Settings page in the app.
    public class AppSettingsViewModel : BaseViewModel
    {
        public AppSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("AppSettings");
        }
    }
}
