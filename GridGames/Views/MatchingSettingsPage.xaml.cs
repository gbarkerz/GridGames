using GridGames.ResX;
using GridGames.Services;
using GridGames.ViewModels;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MatchingGameSettingsPage : ContentPage
    {
        // For now, there are exactly 8 different pairs of cards in the game.
        private int countCardPairs = 8;

        public MatchingGameSettingsPage()
        {
            InitializeComponent();

            // Adding localized strings to a Picker in XAML seems complicated, so do it in code.
            MatchingPictureAspectPicker.Items.Add(AppResources.ResourceManager.GetString("ShowFullPicture"));
            MatchingPictureAspectPicker.Items.Add(AppResources.ResourceManager.GetString("FillCardAndClip"));
            MatchingPictureAspectPicker.Items.Add(AppResources.ResourceManager.GetString("FillCardWithoutClipping"));

            this.BindingContext = new MatchingSettingsViewModel();

            var vm = this.BindingContext as MatchingSettingsViewModel;

            vm.ShowCustomPictures = Preferences.Get("ShowCustomPictures", false);
            vm.PicturePathMatching = Preferences.Get("PicturePathMatching", "");
            vm.PictureOriginalPathMatching = Preferences.Get("PictureOriginalPathMatching", "");

            // Default to Fill and Don't Clip.
            vm.PictureAspect = (Aspect)Preferences.Get("PictureAspect", 2);

            LoadCustomPictureData();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Calling SetSemanticFocus() raises an exception.
            //PairsSettingsTitle.SetSemanticFocus();
            var vm = this.BindingContext as BaseViewModel;
            vm.RaiseNotificationEvent(PairsSettingsTitle.Text);
        }

        private void LoadCustomPictureData()
        {
            var vm = this.BindingContext as MatchingSettingsViewModel;

            // Check we have a filepath for custom pictures.
            if (String.IsNullOrEmpty(vm.PicturePathMatching))
            {
                return;
            }

            vm.PictureListCollection.Clear();

            bool resetCustomPictureData = false;

            // Load up the persisted details for each picture.
            for (int i = 0; i < countCardPairs; i++)
            {
                var item = new PictureData();

                item.Index = i + 1;

                string settingName = "Card" + (i + 1) + "Path";
                var fullpath = Preferences.Get(settingName, "");
                if (String.IsNullOrWhiteSpace(fullpath))
                {
                    // The filepath to a picture is missing.
                    resetCustomPictureData = true;

                    break;
                }

                item.FullPath = fullpath;
                item.FileName = Path.GetFileName(fullpath);

                settingName = "Card" + (i + 1) + "Name";
                item.AccessibleName = Preferences.Get(settingName, "");
                if (String.IsNullOrWhiteSpace(item.AccessibleName))
                {
                    // The accessible name of a picture is missing.
                    resetCustomPictureData = true;

                    break;
                }

                settingName = "Card" + (i + 1) + "Description";
                item.AccessibleDescription = Preferences.Get(settingName, "");

                vm.PictureListCollection.Add(item);
            }

            if (resetCustomPictureData)
            {
                ResetCustomPictureData();
            }
        }

        private void ResetCustomPictureData()
        {
            var vm = this.BindingContext as MatchingSettingsViewModel;

            // Clear all cached and persisted data related to the use of custom pictures.
            vm.PicturePathMatching = "";
            vm.PictureOriginalPathMatching = "";
            vm.ShowCustomPictures = false;

            vm.PictureListCollection.Clear();

            for (int i = 0; i < countCardPairs; i++)
            {
                string settingName = "Card" + (i + 1) + "Path";
                Preferences.Set(settingName, "");

                settingName = "Card" + (i + 1) + "Name";
                Preferences.Set(settingName, "");

                settingName = "Card" + (i + 1) + "Description";
                Preferences.Set(settingName, "");
            }
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            SaveCurrentSettings();

            await Navigation.PopModalAsync();
        }

        private async void ViewSampleButton_Clicked(object sender, EventArgs e)
        {
            await Launcher.OpenAsync(
                "https://github.com/gbarkerz/GridGames/tree/master/GridGames/PairsSamples/LittleMoretonHall");
        }

        private void PictureClearButton_Clicked(object sender, EventArgs e)
        {
            ResetCustomPictureData();
        }

        private async void PictureBrowseButton_Clicked(object sender, EventArgs e)
        {
            var options = new PickOptions
            {
                PickerTitle = "Please select a picture from a folder containing only 8 pictures."
            };

            try
            {
                // Pick a picture file from a folder containing the 8 custom pictures.
                // Note that this doesn't work if selecting a picture from a OneDrive folder.

                string pathToPictures = "";

                //if (Device.RuntimePlatform == Device.iOS)
                //{
                //    var service = DependencyService.Get<IGridGamesPlatformAction>();
                //    pathToPictures = await service.GetPairsPictureFolder();
                //}
                //else if (Device.RuntimePlatform == Device.Android)
                //else
                {
                    var result = await FilePicker.PickAsync(options);
                    if (result != null)
                    {
                        pathToPictures = result.FullPath;
                    }
                }

                if (!String.IsNullOrWhiteSpace(pathToPictures))
                {
                    // The selected folder must contain exactly the required number of pictures in it.
                    var picturePathIsValid = await IsPicturePathValid(pathToPictures);
                    if (picturePathIsValid)
                    {
                        var pictureDetailsFileName = "PairsGamePictureDetails.txt";

                        var displayLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                        if (displayLanguage == "nl")
                        {
                            pictureDetailsFileName = "PairsGamePictureDetails-nl.txt";
                        }

                        // Check the file containing the accessible details for the pictures exists in the folder.
                        string importFile = Path.GetDirectoryName(pathToPictures) +
                                                "/" + pictureDetailsFileName;

                        if (!File.Exists(importFile))
                        {
                            await DisplayAlert(
                                AppResources.ResourceManager.GetString("PairsSettings"),
                                AppResources.ResourceManager.GetString("MissingPairsGamePictureDetails"),
                                AppResources.ResourceManager.GetString("OK"));

                            return;
                        }

                        StreamReader streamReader = null;
                        if ((streamReader = new StreamReader(importFile)) != null)
                        {
                            // We have 8 picture files and an accessible details file,
                            // so attempt to load up everything.
                            var vm = this.BindingContext as MatchingSettingsViewModel;
                            vm.PictureListCollection.Clear();

                            vm.PicturePathMatching = Path.GetDirectoryName(pathToPictures);

                            // PictureOriginalPathMatching is used only for display purposes in the Settings window.
                            var pathOriginal = Path.GetDirectoryName(pathToPictures);
                            DirectoryInfo diOriginal = new DirectoryInfo(pathOriginal);
                            vm.PictureOriginalPathMatching = diOriginal.Name;

                            var folder = Path.GetDirectoryName(pathToPictures);
                            DirectoryInfo di = new DirectoryInfo(folder);

                            string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };

                            var files = di.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                                            .Where(f => extensions.Contains(f.Extension.ToLower()))
                                            .ToArray();

                            // First create the new set of 8 PictureData objects.
                            for (int i = 0; i < files.Length; i++)
                            {
                                var filepath = files[i];
                                var item = new PictureData();
                                item.Index = i + 1;
                                item.FullPath = filepath.FullName;
                                item.FileName = Path.GetFileName(filepath.FullName);
                                vm.PictureListCollection.Add(item);
                            }

                            // Now populate the PictureData with the accessible names and descriptions.
                            string content = null;
                            while ((content = streamReader.ReadLine()) != null)
                            {
                                if (!SetNameDescription(content))
                                {
                                    await DisplayAlert(
                                        AppResources.ResourceManager.GetString("PairsSettings"),
                                        AppResources.ResourceManager.GetString("UnexpectedDataInFile"),
                                        AppResources.ResourceManager.GetString("OK"));

                                    // The expected data was missing from the file, so reset.
                                    ResetCustomPictureData();

                                    break;
                                }
                            }

                            streamReader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GridGames: Browse exception: " + ex.Message);

                ResetCustomPictureData();
            }
        }

        // For now, a folder is considered valid if it contains exactly 8 files
        // with extensions suggesting that the game can handle them.
        public async Task<bool> IsPicturePathValid(string picturePath)
        {
            bool picturePathValid = true;

            try
            {
                var folder = Path.GetDirectoryName(picturePath);
                DirectoryInfo di = new DirectoryInfo(folder);

                string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };

                var files = di.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                                .Where(f => extensions.Contains(f.Extension.ToLower()))
                                .ToArray();

                if (files.Length != 8)
                {
                    picturePathValid = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Pairs: IsPicturePathValid " + ex.Message);

                picturePathValid = false;
            }

            if (!picturePathValid)
            {
                await DisplayAlert(
                    AppResources.ResourceManager.GetString("PairsSettings"),
                    AppResources.ResourceManager.GetString("ChooseEightPictures"),
                    AppResources.ResourceManager.GetString("OK"));
            }

            return picturePathValid;
        }

        // Returns false if the attempt to import data should be terminated and
        // an error message should be shown to the player.
        private bool SetNameDescription(string content)
        {
            var fileNameDelimiter = content.IndexOf('\t');

            // Account for the string containing no tabs at all.
            if (fileNameDelimiter == -1)
            {
                // There was no accessible name found on this line, so do nothing here.
                // Do not terminate the attempt to find all the picture details due to this, 
                // rather continue to process the datafile looking for picture details.
                // This approach means that unexpected blank lines in the datafile won't
                // break the attempt to load the data.
                return true;
            }

            string fileName = content.Substring(0, fileNameDelimiter);

            var vm = this.BindingContext as MatchingSettingsViewModel;

            // Find the PictureData object in our current collection which has the
            // same name as that found in the accessible details file.
            for (int i = 0; i < countCardPairs; i++)
            {
                if (fileName == vm.PictureListCollection[i].FileName)
                {
                    // We have a match, so set the accessible name and description.
                    string details = content.Substring(fileNameDelimiter + 1);

                    var nameDelimiter = details.IndexOf('\t');

                    string name = "";
                    string description = "";

                    // Account for the string containing no tab following the Name.
                    if (nameDelimiter != -1)
                    {
                        name = details.Substring(0, nameDelimiter);
                        description = details.Substring(nameDelimiter + 1);
                    }
                    else
                    {
                        // Leave the description empty.
                        name = details;
                    }

                    if (String.IsNullOrWhiteSpace(name))
                    {
                        // A picture must an associated accessible name.
                        return false;
                    }

                    vm.PictureListCollection[i].AccessibleName = name;
                    vm.PictureListCollection[i].AccessibleDescription = description;

                    break;
                }
            }

            return true;
        }

        private void SaveCurrentSettings()
        {
            var vm = this.BindingContext as MatchingSettingsViewModel;

            // The Settings window is being closed, so persist whatever picture data we currently have.
            if (!String.IsNullOrWhiteSpace(vm.PicturePathMatching))
            {
                for (int i = 0; i < countCardPairs; i++)
                {
                    if (vm.PictureListCollection.Count > i)
                    {
                        string settingName = "Card" + (i + 1) + "Path";
                        Preferences.Set(settingName, vm.PictureListCollection[i].FullPath);

                        settingName = "Card" + (i + 1) + "Name";
                        Preferences.Set(settingName, vm.PictureListCollection[i].AccessibleName);

                        settingName = "Card" + (i + 1) + "Description";
                        Preferences.Set(settingName, vm.PictureListCollection[i].AccessibleDescription);
                    }
                }
            }
        }
    }
}