using GridGames.ResX;

namespace GridGames.ViewModels
{
    // View model for the Where's WCAG Settings page in the app.
    public class SweeperSettingsViewModel : BaseViewModel
    {
        public SweeperSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("SweeperSettings");

            SideLength = Preferences.Get("SideLength", 4);
            FrogCount = Preferences.Get("FrogCount", 2);
        }

        private int sideLength;
        public int SideLength
        {
            get
            {
                return sideLength;
            }
            set
            {
                if (sideLength != value)
                {
                    SetProperty(ref sideLength, value);

                    Preferences.Set("sideLength", value);
                }
            }
        }

        private int frogCount;
        public int FrogCount
        {
            get
            {
                return frogCount;
            }
            set
            {
                if (frogCount != value)
                {
                    SetProperty(ref frogCount, value);

                    Preferences.Set("FrogCount", value);
                }
            }
        }
    }
}
