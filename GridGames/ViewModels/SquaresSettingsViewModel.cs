using GridGames.ResX;

namespace GridGames.ViewModels
{
    // View model for the Squares Settings page in the app.
    public class SquareSettingsViewModel : BaseViewModel
    {
        public SquareSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("SquaresSettings");
        }

        private bool showNumbers;
        public bool ShowNumbers
        {
            get
            {
                return showNumbers;
            }
            set
            {
                if (showNumbers != value)
                {
                    SetProperty(ref showNumbers, value);

                    Preferences.Set("ShowNumbers", value);
                }
            }
        }

        private int numberSizeIndex;
        public int NumberSizeIndex
        {
            get
            {
                return numberSizeIndex;
            }
            set
            {
                if (numberSizeIndex != value)
                {
                    SetProperty(ref numberSizeIndex, value);

                    Preferences.Set("NumberSizeIndex", value);
                }
            }
        }

        private bool showPicture;
        public bool ShowPicture
        {
            get
            {
                return showPicture;
            }
            set
            {
                if (showPicture != value)
                {
                    SetProperty(ref showPicture, value);

                    Preferences.Set("ShowPicture", value);
                }
            }
        }

        private string picturePathSquares;
        public string PicturePathSquares
        {
            get
            {
                return picturePathSquares;
            }
            set
            {
                if (picturePathSquares != value)
                {
                    SetProperty(ref picturePathSquares, value);

                    Preferences.Set("PicturePathSquares", value);
                }
            }
        }

        private string pictureName;
        public string PictureName
        {
            get
            {
                return pictureName;
            }
            set
            {
                if (pictureName != value)
                {
                    SetProperty(ref pictureName, value);

                    Preferences.Set("PictureName", value);
                }
            }
        }
    }
}
