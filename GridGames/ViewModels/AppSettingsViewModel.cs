using GridGames.ResX;
using GridGames.Services;
using GridGames.Styles;
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

        private bool showDarkTheme;
        public bool ShowDarkTheme
        {
            get
            {
                return showDarkTheme;
            }
            set
            {
                if (showDarkTheme != value)
                {
                    SetProperty(ref showDarkTheme, value);

                    Preferences.Set("ShowDarkTheme", value);

                    if (showDarkTheme)
                    {
                        App.Current.Resources = new DarkTheme();
                    }
                    else
                    {
                        App.Current.Resources = new LightTheme();
                    }
                }
            }
        }

        // Use "new" hear to make it unabiguous between the BaseViewModel HideGrid property.
        private bool hideGrid;
        public new bool HideGrid
        {
            get
            {
                return hideGrid;
            }
            set
            {
                if (hideGrid != value)
                {
                    SetProperty(ref hideGrid, value);

                    Preferences.Set("HideGrid", value);
                }
            }
        }

    }
}
