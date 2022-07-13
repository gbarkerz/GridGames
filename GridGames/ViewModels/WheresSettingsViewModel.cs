using GridGames.ResX;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames.ViewModels
{
    // View model for the Where's WCAG Settings page in the app.
    public class WheresSettingsViewModel : BaseViewModel
    {
        public WheresSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("WheresSettings");
        }

        private bool playSoundOnMatch;
        public bool PlaySoundOnMatch
        {
            get
            {
                return playSoundOnMatch;
            }
            set
            {
                if (playSoundOnMatch != value)
                {
                    SetProperty(ref playSoundOnMatch, value);

                    Preferences.Set("WheresPlaySoundOnMatch", value);
                }
            }
        }

        private bool playSoundOnNotMatch;
        public bool PlaySoundOnNotMatch
        {
            get
            {
                return playSoundOnNotMatch;
            }
            set
            {
                if (playSoundOnNotMatch != value)
                {
                    SetProperty(ref playSoundOnNotMatch, value);

                    Preferences.Set("WheresPlaySoundOnNotMatch", value);
                }
            }
        }

        private bool showBonusQuestion;
        public bool ShowBonusQuestion
        {
            get
            {
                return showBonusQuestion;
            }
            set
            {
                if (showBonusQuestion != value)
                {
                    SetProperty(ref showBonusQuestion, value);

                    Preferences.Set("ShowBonusQuestion", value);
                }
            }
        }

    }
}
