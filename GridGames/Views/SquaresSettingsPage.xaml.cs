using GridGames.ViewModels;
using System.Diagnostics;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SquaresSettingsPage : ContentPage
    {
        public SquaresSettingsPage()
        {
            InitializeComponent();

#if IOS
            // VoiceOver doesn't navigate to controls contained in a Grid
            // that has an accessible name.
            SemanticProperties.SetDescription(ShowNumbersGrid, null);
            SemanticProperties.SetDescription(ShowPictureGrid, null);
#endif

            // Adding localized strings to a Picker in XAML seems complicated, so do it in code.
            //SquaresNumberSizePicker.Items.Add(AppResources.ResourceManager.GetString("Small"));
            //SquaresNumberSizePicker.Items.Add(AppResources.ResourceManager.GetString("Medium"));
            //SquaresNumberSizePicker.Items.Add(AppResources.ResourceManager.GetString("Large"));
            //SquaresNumberSizePicker.Items.Add(AppResources.ResourceManager.GetString("Largest"));

            var baseScale = 100;

            for (int i = 0; i < 11; i++)
            {
                SquaresGridSizeScale.Items.Add(baseScale.ToString() + "%");

                baseScale += 25;
            }

            this.BindingContext = new SquareSettingsViewModel();

            var vm = this.BindingContext as SquareSettingsViewModel;
            vm.ShowNumbers = Preferences.Get("ShowNumbers", true);
            vm.NumberSizeIndex = Preferences.Get("NumberSizeIndex", 1);
            vm.ShowPicture = Preferences.Get("ShowPicture", false);
            vm.PicturePathSquares = Preferences.Get("PicturePathSquares", "");
            vm.PictureName = Preferences.Get("PictureName", "");

            // Default to keeping the full grid in view
            vm.GridSizeScale = (int)Preferences.Get("SquaresGridSizeScale", 100);
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            SaveCurrentSettings();

            await Navigation.PopModalAsync();
        }


        private void SaveCurrentSettings()
        {
            var vm = this.BindingContext as SquareSettingsViewModel;

            var scale = (SquaresGridSizeScale.SelectedIndex * 25) + 100;

            Preferences.Set("SquaresGridSizeScale", scale);
        }

        private void PictureClearButton_Clicked(object sender, EventArgs e)
        {
            ResetPictureData();
        }

        private void ResetPictureData()
        {
            var vm = this.BindingContext as SquareSettingsViewModel;

            // Clear all cached and persisted data related to the use of custom pictures.
            vm.PicturePathSquares = "";
            vm.ShowPicture = false;
            vm.PictureName = "";
        }

        private async void PictureBrowseButton_Clicked(object sender, EventArgs e)
        {
            var options = new PickOptions
            {
                PickerTitle = "Please select a background picture."
            };

            try
            {
                // Barker: For now only load jpg images.
                options.FileTypes = FilePickerFileType.Images;

                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    var settingsViewModel = this.BindingContext as SquareSettingsViewModel;

                    // Copy the selected picture into a tmp folder where we can access it later.
                    var targetFolder = Path.Combine(Path.GetTempPath(), "SquaresGameCurrentPictures");
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    // Empty this dedicated tmp folder now.
                    DirectoryInfo di = new DirectoryInfo(targetFolder);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    var filenameSource = Path.GetFileName(result.FullPath);

                    var filenameDest = Path.Combine(targetFolder, filenameSource);

                    // Barker: Windows required destination filename, but other platforms expect destination directory?
                    File.Copy(result.FullPath, filenameDest);

                    settingsViewModel.PicturePathSquares = filenameDest;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MobileGridGames: Browse exception: " + ex.Message);
            }
        }
    }
}