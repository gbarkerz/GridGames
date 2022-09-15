using GridGames.ResX;
using GridGames.Services;
using GridGames.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SquaresPage : ContentPage
    {
        // Path to most recently fully loaded picture.
        private string previousLoadedPicture = "";

        public SquaresPage()
        {
            InitializeComponent();
        }

        // This gets called when switching from the Matching Game to the Squares Game,
        // and also when closing the Squares Settings page.
        protected override async void OnAppearing()
        {
            Debug.Write("Squares Game: OnAppearing called.");

            base.OnAppearing();

            Preferences.Set("InitialGame", "Squares");

            // Account for the app settings changing since the page was last shown.
            var vm = this.BindingContext as SquaresViewModel;
            vm.FirstRunSquares = Preferences.Get("FirstRunSquares", true);
            if (vm.FirstRunSquares)
            {
                // Barker: Update steps for custom announcements.
                //var service = DependencyService.Get<IMobileGridGamesPlatformAction>();
                //service.ScreenReaderAnnouncement(
                //    SquaresWelcomeTitleLabel.Text + ", " + SquaresWelcomeTitleInstructions.Text);
            }

            vm.ShowNumbers = Preferences.Get("ShowNumbers", true);
            vm.NumberHeight = Preferences.Get("NumberSizeIndex", 1);
            vm.ShowPicture = Preferences.Get("ShowPicture", false);
            vm.PicturePathSquares = Preferences.Get("PicturePathSquares", "");
            vm.PictureName = Preferences.Get("PictureName", "");

            // Barker: Remove the Hide Grid feature.
            //vm.HideGrid = Preferences.Get("HideGrid", false);

            // Has the state of the picture being shown changed since we were last changed?
            if (vm.ShowPicture && (vm.PicturePathSquares != previousLoadedPicture))
            {
                // Prevent input on the grid while the image is being loaded into the squares.
                vm.GameIsLoading = true;

                // Future: Without a delay here, the loading UI rarely shows up on iOS.
                // Investigate this further and remove this delay.
                await Task.Delay(200);

                // Restore the order of the squares in the grid.
                vm.RestoreEmptyGrid();

                // The loading of the images into the squares is made synchronously through the first 15 squares.

                // Barker: No image editor for now.
                //nextSquareIndexForImageSourceSetting = 0;

                // Check whether the image file exists before trying to load it into the ImageEditor.
                if (vm.IsImageFilePathValid(vm.PicturePathSquares))
                {
                    vm.RaiseNotificationEvent(PleaseWaitLabel.Text);

                    // Future: Verify that if the various event handlers are still being called from the
                    // previous attempt to load a picture, those event handlers will no longer be called
                    // once the loading of another picture begins.

                    // Barker: No image editor for now.
                    //GridGameImageEditor.Source = ImageSource.FromFile(vm.PicturePathSquares);
                    //Debug.WriteLine("Grid Games: ImageEditor source now " + GridGameImageEditor.Source.ToString());
                }
                else
                {
                    Debug.WriteLine("Grid Games: Valid image file not found. " + vm.PicturePathSquares);

                    // We'll not attempt to load this picture again.
                    Preferences.Set("PicturePathSquares", "");
                    vm.PicturePathSquares = "";

                    vm.GameIsLoading = false;

                    if (Shell.Current != null)
                    {
                        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
                    }
                }
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (vm.FirstRunSquares || vm.GameIsLoading)
            {
                return;
            }

            var itemGrid = (Grid)sender;
            var itemAccessibleName = AutomationProperties.GetName(itemGrid);

            Debug.WriteLine("Grid Games: Tapped on Square " + itemAccessibleName);

            AttemptToMoveSquareByName(itemAccessibleName);
        }

        private async void AttemptToMoveSquareByName(string itemName)
        {
            var vm = this.BindingContext as SquaresViewModel;

            int itemIndex = GetItemCollectionIndexFromItemAccessibleName(itemName);
            if (itemIndex != -1)
            {
                bool gameIsWon = vm.AttemptToMoveSquare(itemIndex);
                if (gameIsWon)
                {
                    await OfferToRestartGame();
                }
            }
        }

        private int GetItemCollectionIndexFromItemAccessibleName(string ItemAccessibleName)
        {
            var vm = this.BindingContext as SquaresViewModel;

            int itemIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (vm.SquareListCollection[i].AccessibleName == ItemAccessibleName)
                {
                    itemIndex = i;
                    break;
                }
            }

            return itemIndex;
        }

        private async Task OfferToRestartGame()
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (!vm.FirstRunSquares)
            {
                var message = "";

                if (String.IsNullOrWhiteSpace(vm.PictureName))
                {
                    message = String.Format(
                        AppResources.ResourceManager.GetString("SquaresWonInGoes"), 8 + vm.MoveCount);
                }
                else
                {
                    message = String.Format(
                        AppResources.ResourceManager.GetString("CompletedSquaresPictureInMoves"),
                            vm.PictureName,
                            8 + vm.MoveCount);
                }

                var answer = await DisplayAlert(
                    AppResources.ResourceManager.GetString("Congratulations"),
                    message,
                    AppResources.ResourceManager.GetString("Yes"),
                    AppResources.ResourceManager.GetString("No"));
                if (answer)
                {
                    vm.ResetGrid();
                }
            }
        }

        // SelectionChanged handling only exists today in the app to support Android Switch Access. 
        // At some point SelectionChanged may also be a part of keyboard support, but currently the
        // rest of the app is not keyboard accessible, and focus feedback is unusable on the items.

        private async void SquaresGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Squares Grid Game: Selection changed. Selection count is " + e.CurrentSelection.Count);

            // Do nothing here if pictures have not been loaded yet onto the squares.
            var vm = this.BindingContext as SquaresViewModel;
            if (vm.FirstRunSquares || vm.GameIsLoading)
            {
                return;
            }

            // No action required here if there is no selected item.
            if (e.CurrentSelection.Count > 0)
            {
                bool gameIsWon = vm.AttemptMoveBySelection(e.CurrentSelection[0]);

                // Clear the selection now to support the same square moving again.
                SquaresCollectionView.SelectedItem = null;

                if (gameIsWon)
                {
                    await OfferToRestartGame();
                }
            }
        }

        private void SquaresWelcomeOKButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;
            vm.FirstRunSquares = false;
        }

        private async void SquaresGameSettingsButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (!vm.FirstRunSquares)
            {
                var settingsPage = new SquaresSettingsPage();
                await Navigation.PushModalAsync(settingsPage);
            }
        }

        public async void ReactToKeyInputOnSelectedCard()
        {
            var item = SquaresCollectionView.SelectedItem as SquaresViewModel.Square;
            if (item != null)
            {
                await ReactToInputOnCard(item);
            }
        }

        private async Task ReactToInputOnCard(SquaresViewModel.Square item)
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (vm.FirstRunSquares || vm.GameIsLoading)
            {
                return;
            }

            int itemIndex = GetItemCollectionIndexFromItemAccessibleName(item.AccessibleName);
            if (itemIndex != -1)
            {
                bool gameIsWon = vm.AttemptToMoveSquare(itemIndex);
                if (gameIsWon)
                {
                    await OfferToRestartGame();
                }
            }
        }


        // Barker: No image editor for now. 
        /*
                // Important: The remainder of this file relates to setting of pictures on the squares in the
                // grid. However, the threading model for the various event handlers below is not understood,
                // so while the code seems to work it seems almost certain that the code will change once the
                // threading model is understood.

                // The use of nextSquareIndexForImageSourceSetting as the index of the square whose
                // PictureImageSource property is being set, assumes that the squares have not yet
                // been shuffled in the view model's collection of Squares.
                private int nextSquareIndexForImageSourceSetting = 0;

                // ImageLoad is called once following the picture being set on the control.
                private void GridGameImageEditor_ImageLoaded(object sender, ImageLoadedEventArgs args)
                {
                    Debug.WriteLine("MobileGridGames: In ImageLoaded, calling PerformCrop.");

                    // Don't load a picture if this picture is already fully loaded.
                    var vm = this.BindingContext as SquaresViewModel;
                    if (vm.PicturePathSquares == previousLoadedPicture)
                    {
                        return;
                    }

                    // Prevent interaction with any of the flyout items by preventing access to the flyout.
                    // This can't be done in OnAppearing() because the Shell.Current might still be null
                    // then. Future: While Shell.Current seems to be set here, is this robust? Change this
                    // approach so I can have some reason to think it's robust.
                    Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

                    if (nextSquareIndexForImageSourceSetting != 0)
                    {
                        Debug.WriteLine("MobileGridGames: Error in ImageLoaded, nextSquareIndexForImageSourceSetting should be zero, " +
                            nextSquareIndexForImageSourceSetting.ToString());

                        vm.GameIsLoading = false;
                        nextSquareIndexForImageSourceSetting = 0;

                        return;
                    }

                    // Perform a crop for first square.
                    PerformCrop();

                    Debug.WriteLine("MobileGridGames: Leave ImageLoaded, done call to PerformCrop.");
                }

                private void PerformCrop()
                {
                    // The x,y values for cropping are a percentage of the full image size.
                    int x = 25 * (nextSquareIndexForImageSourceSetting % 4);
                    int y = 25 * (nextSquareIndexForImageSourceSetting / 4);

                    // Future: On release builds, often the fisrt square contains the full image,
                    // with no cropping at all. This unexpected result does not seem to happen if
                    // we don't set the origin to 0,0. So until this issue is understood, set the
                    // origin of the first crop to 1,1.
                    if (nextSquareIndexForImageSourceSetting == 0)
                    {
                        x = 1;
                        y = 1;
                    }

                    Debug.WriteLine("MobileGridGames: In PerformCrop, crop at " + x + ", " + y + ".");

                    // Set up the bounds for the next crop operation.
                    GridGameImageEditor.ToggleCropping(new Rectangle(x, y, 25, 25));

                    Debug.WriteLine("MobileGridGames: Called ToggleCropping.");

                    // Crop() seems to need to be run on the UI thread.
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Debug.WriteLine("MobileGridGames: PerformCrop, about to call Crop.");

                        GridGameImageEditor.Crop();

                        Debug.WriteLine("MobileGridGames: PerformCrop, called Crop.");
                    });

                    Debug.WriteLine("MobileGridGames: Leave PerformCrop.");
                }

                private int mostRecentSquareWithImageEdited = -1;

                // ImageEdited is called following a Crop operation and when the image is reset.
                private void GridGameImageEditor_ImageEdited(object sender, ImageEditedEventArgs e)
                {
                    Debug.WriteLine("MobileGridGames: In ImageEdited.");

                    // On iOS, GridGameImageEditor_ImageEdited seems to be called multiple times
                    // in succession, without a specific edit in between. If that happens here,
                    // only react to the first call to GridGameImageEditor_ImageEdited.
                    if (mostRecentSquareWithImageEdited == nextSquareIndexForImageSourceSetting)
                    {
                        return;
                    }

                    // If we're here following a resetting of the image, take no follow-up action.
                    if (e.IsImageEdited)
                    {
                        mostRecentSquareWithImageEdited = nextSquareIndexForImageSourceSetting;

                        // We must be here following a crop operation.
                        GridGameImageEditor.Save();
                    }

                    Debug.WriteLine("MobileGridGames: Leave ImageEdited.");
                }

                // ImageSaving is called following each crop of the picture.
                private void GridGameImageEditor_ImageSaving(object sender, ImageSavingEventArgs args)
                {
                    Debug.WriteLine("MobileGridGames: In ImageSaving." + Shell.Current);

                    // Important: Prevent the cropped image from being saved to a file.
                    args.Cancel = true;

                    var vm = this.BindingContext as SquaresViewModel;

                    // Get the image data for the previous crop operation.
                    var source = GetImageSourceFromPictureStream(args.Stream);

                    // We assume here that the use of nextSquareIndexForImageSourceSetting is synchronous
                    // as all the cropping operations are performed.

                    if (nextSquareIndexForImageSourceSetting > 14)
                    {
                        Debug.WriteLine("MobileGridGames: Error in ImageSaving, nextSquareIndexForImageSourceSetting too high, " +
                            nextSquareIndexForImageSourceSetting);

                        vm.GameIsLoading = false;
                        nextSquareIndexForImageSourceSetting = 0;

                        return;
                    }

                    // Set the cropped image on the next square.
                    var square = vm.SquareListCollection[nextSquareIndexForImageSourceSetting];
                    square.PictureImageSource = source;

                    // Now reset the image to its original form, in order to perform the next crop.
                    // This seems to need to be run on the UI thread.
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Debug.WriteLine("MobileGridGames: About to call Reset.");

                        GridGameImageEditor.Reset();

                        Debug.WriteLine("MobileGridGames: Back from call to Reset.");
                    });

                    Debug.WriteLine("MobileGridGames: Leave ImageSaving.");
                }

                private ImageSource GetImageSourceFromPictureStream(Stream stream)
                {
                    stream.Position = 0;

                    // The input stream will get closed, so create a new stream from it here.
                    var buffer = new byte[stream.Length];

                    MemoryStream ms = new MemoryStream();

                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    var imageSource = ImageSource.FromStream(() => new MemoryStream(buffer));

                    return imageSource;
                }

                // EndResetis called as the original image is reset to perform the crop operation for the next square.
                private void GridGameImageEditor_EndReset(object sender, EndResetEventArgs args)
                {
                    Debug.WriteLine("MobileGridGames: In EndReset.");

                    var vm = this.BindingContext as SquaresViewModel;

                    // Provide a "3 2 1" countdown for players using screen readers.
                    if (nextSquareIndexForImageSourceSetting % 5 == 0)
                    {
                        var countdown = (15 - nextSquareIndexForImageSourceSetting) / 5;
                        vm.RaiseNotificationEvent(countdown.ToString());
                    }

                    // We've completed the image settings for a square. Continue with the next square if there is one.
                    ++nextSquareIndexForImageSourceSetting;

                    Debug.WriteLine("MobileGridGames: nextSquareIndexForImageSourceSetting now " + nextSquareIndexForImageSourceSetting);

                    // If we're not done loading pictures into squares, load a picture into the next square.
                    if (nextSquareIndexForImageSourceSetting < 15)
                    {
                        PerformCrop();
                    }
                    else
                    {
                        vm.GameIsLoading = false;
                        nextSquareIndexForImageSourceSetting = 0;

                        // We've loaded all the pictures, so shuffle them and enable the game.
                        vm.ResetGrid();

                        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;

                        // Now that a picture has been fully loaded, cache the path to the loaded picture.
                        // We'll not load another picture until the picture being loaded is different from
                        // this successfully loaded picture.
                        previousLoadedPicture = vm.PicturePathSquares;
                    }

                    Debug.WriteLine("MobileGridGames: Leave EndReset");
                }
        */
    }
}
