using GridGames.ResX;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GridGames.ViewModels
{
    // View model for the Squares Settings page in the app.
    public class MatchingSettingsViewModel : BaseViewModel
    {
        public MatchingSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("PairsSettings");

            this.PictureListCollection = new ObservableCollection<PictureData>();
        }

        private ObservableCollection<PictureData> pictureList;
        public ObservableCollection<PictureData> PictureListCollection
        {
            get { return pictureList; }
            set { this.pictureList = value; }
        }

        private string picturePathMatching;
        public string PicturePathMatching
        {
            get
            {
                return picturePathMatching;
            }
            set
            {
                if (picturePathMatching != value)
                {
                    SetProperty(ref picturePathMatching, value);

                    Preferences.Set("PicturePathMatching", value);
                }
            }
        }

        private string pictureOriginalPathMatching;
        public string PictureOriginalPathMatching
        {
            get
            {
                return pictureOriginalPathMatching;
            }
            set
            {
                if (pictureOriginalPathMatching != value)
                {
                    SetProperty(ref pictureOriginalPathMatching, value);

                    Preferences.Set("PictureOriginalPathMatching", value);
                }
            }
        }

        private bool showCustomPictures;
        public bool ShowCustomPictures
        {
            get
            {
                return showCustomPictures;
            }
            set
            {
                if (showCustomPictures != value)
                {
                    SetProperty(ref showCustomPictures, value);

                    Preferences.Set("ShowCustomPictures", value);
                }
            }
        }

        private Aspect pictureAspect;
        public Aspect PictureAspect
        {
            get
            {
                return pictureAspect;
            }
            set
            {
                if (pictureAspect != value)
                {
                    SetProperty(ref pictureAspect, value);

                    Preferences.Set("PictureAspect", (int)value);
                }
            }
        }

        private int gridSizeScale = 100;
        public int GridSizeScale
        {
            get
            {
                return gridSizeScale;
            }
            set
            {
                if (gridSizeScale != value)
                {
                    SetProperty(ref gridSizeScale, value);

                    Preferences.Set("GridSizeScale", (int)value);
                }
            }
        }
    }

    public class PictureData : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string FileName { get; set; }

        private string fullPath;
        public string FullPath
        {
            get { return fullPath; }
            set
            {
                SetProperty(ref fullPath, value);
            }
        }

        private string accessibleName;
        public string AccessibleName
        {
            get { return accessibleName; }
            set
            {
                SetProperty(ref accessibleName, value);
            }
        }

        private string accessibleDescription;
        public string AccessibleDescription
        {
            get { return accessibleDescription; }
            set
            {
                SetProperty(ref accessibleDescription, value);
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
