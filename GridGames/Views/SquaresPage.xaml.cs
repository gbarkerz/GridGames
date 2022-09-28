using GridGames.ResX;
using GridGames.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;

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

            Application.Current.RequestedThemeChanged += (s, a) =>
            {
                var currentTheme = a.RequestedTheme;
                if (currentTheme == AppTheme.Unspecified)
                {
                    currentTheme = Application.Current.PlatformAppTheme;
                }

                var vm = this.BindingContext as WheresViewModel;
                vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);
            };

#if WINDOWS
            SquaresCollectionView.SizeChanged += SquaresCollectionView_SizeChanged;
#endif

#if ANDROID
            InputBlockingGrid.IsVisible = false;
#endif
        }

        // This gets called when switching to the Squares Game from other games,
        // and also when closing the Squares Settings page.
        protected override void OnAppearing()
        {
            Debug.Write("Squares Game: OnAppearing called.");

            base.OnAppearing();

            Preferences.Set("InitialGame", "Squares");

            // Account for the app settings changing since the page was last shown.
            var vm = this.BindingContext as SquaresViewModel;

            vm.FirstRunSquares = Preferences.Get("FirstRunSquares", true);
            if (vm.FirstRunSquares)
            {
                vm.RaiseNotificationEvent(
                    SquaresWelcomeTitleLabel.Text + ", " + SquaresWelcomeTitleInstructions.Text);
            }

            var currentTheme = Application.Current.UserAppTheme;
            if (currentTheme == AppTheme.Unspecified)
            {
                currentTheme = Application.Current.PlatformAppTheme;
            }

            vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);

            vm.ShowNumbers = Preferences.Get("ShowNumbers", true);
            vm.NumberHeight = Preferences.Get("NumberSizeIndex", 1);
            vm.ShowPicture = Preferences.Get("ShowPicture", false);
            vm.PicturePathSquares = Preferences.Get("PicturePathSquares", "");
            vm.PictureName = Preferences.Get("PictureName", "");

            // Has the state of the picture being shown changed since we were last changed?
            if (vm.ShowPicture && (vm.PicturePathSquares != null) &&
                (vm.PicturePathSquares != previousLoadedPicture))
            {
                // Restore the order of the squares in the grid.
                vm.RestoreEmptyGrid();

                // Check whether the image file exists before trying to load it into the ImageEditor.
                if (vm.IsImageFilePathValid(vm.PicturePathSquares))
                {
                    // Set the images shown on the squares.
                    if ((SquaresCollectionView.Width > 0) && (SquaresCollectionView.Height > 0))
                    {
                        ShowCustomPictureInSquares();
                    }

                    // Now that a picture has been fully loaded, cache the path to the loaded picture.
                    // We'll not load another picture until the picture being loaded is different from
                    // this successfully loaded picture.
                    previousLoadedPicture = vm.PicturePathSquares;
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

        private async void FallthroughGrid_Tapped(object sender, EventArgs e)
        {
            await DisplayAlert(
                AppResources.ResourceManager.GetString("GridGames"),
                AppResources.ResourceManager.GetString("FallthroughTapMessage"),
                AppResources.ResourceManager.GetString("OK"));
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (vm.FirstRunSquares || vm.GameIsLoading)
            {
                return;
            }

            var itemGrid = (Grid)sender;
            var itemAccessibleName = SemanticProperties.GetDescription(itemGrid);

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
                    RestartGame();
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

        public async void ShowHelp()
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (!vm.FirstRunSquares)
            {
                await Navigation.PushModalAsync(new HelpPage(this));

                SquaresCollectionView.Focus();
            }
        }

        public void RestartGame()
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (!vm.FirstRunSquares)
            {
                // Restore the order of the squares in the grid.
                vm.RestoreEmptyGrid();

                if (vm.ShowPicture && !String.IsNullOrWhiteSpace(vm.PicturePathSquares))
                {
                    ShowCustomPictureInSquares();
                }
                else
                {
                    vm.ResetGrid();
                }
            }
        }

        // The remainder of this file relates to setting the images shown on the squares in the game.

        private Timer timerTransformImages;

        private void SquaresCollectionView_SizeChanged(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;

            if (vm.ShowPicture && !String.IsNullOrWhiteSpace(vm.PicturePathSquares))
            {
                if ((SquaresCollectionView.Width > 0) && (SquaresCollectionView.Height > 0))
                {
                    ShowCustomPictureInSquares();
                }
            }
        }

        public void ShowCustomPictureInSquares()
        {
            var vm = this.BindingContext as SquaresViewModel;

            string picturePathSquares = vm.PicturePathSquares;

            // Prevent input on the grid while the image is being loaded into the squares.
            vm.GameIsLoading = true;

            //vm.RaiseNotificationEvent(PleaseWaitLabel.Text);

            SquaresCollectionView.Margin = new Thickness(0);

            Stream fileStream = File.OpenRead(picturePathSquares);

            var originalStream = new SKManagedStream(fileStream);

            var originalBitmap = SKBitmap.Decode(originalStream);

            // Leave the 16th square empty.
            for (int i = 0; i < 15; ++i)
            {
                vm.SquareListCollection[i].PictureImageSource = GetImageSourceForSquare(originalBitmap, i);
            }

            SquaresCollectionView.Margin = new Thickness(1);

            vm.GameIsLoading = false;

            // Now jumble the squares.
            vm.ResetGrid();
        }

        private ImageSource GetImageSourceForSquare(SKBitmap originalBitmap, int index)
        {
            var sourceImagePortionWidth = (int)(originalBitmap.Width / 4) - 10;
            var sourceImagePortionHeight = (int)(originalBitmap.Height / 4) - 10;

            var destGridPortionWidth = (int)(SquaresCollectionView.Width / 4) - 10;
            var destGridPortionHeight = (int)(SquaresCollectionView.Height / 4) - 10;

            SKRect destRect = new SKRect(0, 0, destGridPortionWidth, destGridPortionHeight);

            // Barker: Check this.
            int col = index % 4;
            int row = index / 4;

            SKRect sourceRect = new SKRect(
                col * sourceImagePortionWidth, 
                row * sourceImagePortionHeight, 
                (col + 1) * sourceImagePortionWidth, 
                (row + 1) * sourceImagePortionHeight);

            // Copy 1/16 of the original into that bitmap
            SKBitmap bitmap = new SKBitmap(sourceImagePortionWidth, sourceImagePortionHeight);

            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.DrawBitmap(originalBitmap, sourceRect, destRect);
            }

            var croppedImageSource = (SKBitmapImageSource)bitmap;

            return croppedImageSource;
        }
    }
}
