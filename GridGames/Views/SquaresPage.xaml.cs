﻿using GridGames.ResX;
using GridGames.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;
using static GridGames.ViewModels.SquaresViewModel;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SquaresPage : ContentPage
    {
        // Path to most recently fully loaded picture.
        private string previousLoadedPicture = "";
        private SKBitmap originalCustomPictureBitmap = null;

        private Timer timerSetCustomPicture;
        private bool inShowCustomPictureIfReady = false;

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

                var vm = this.BindingContext as SquaresViewModel;
                vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);
            };

#if ANDROID
            InputBlockingGrid.IsVisible = false;

            SquaresCollectionView.SelectionChanged += SquaresCollectionView_SelectionChanged;
#endif
        }

#if ANDROID
        private async void SquaresCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = sender as CollectionView;
            if (collectionView != null)
            {
                if (collectionView.SelectedItem != null)
                {
                    // Don't leave any square selected after this attempt to move.
                    collectionView.SelectedItem = null;

                    var vm = this.BindingContext as SquaresViewModel;
                    bool gameIsWon = vm.AttemptMoveBySelection(collectionView.SelectedItem);
                    if (gameIsWon)
                    {
                        await OfferToRestartGame();
                    }

                }
            }
        }
#endif

        private void ShowCustomPicture()
        {
            var vm = this.BindingContext as SquaresViewModel;

            if (vm.ShowPicture && !String.IsNullOrWhiteSpace(vm.PicturePathSquares))
            {
                vm.GameIsLoading = true;

                // If the CollectionView isn't sized yet, wait a while and try again.
                if ((SquaresCollectionView.Width <= 0) || (SquaresCollectionView.Height <= 0))
                {
                    if (timerSetCustomPicture == null)
                    {
                        Debug.WriteLine("ShowCustomPicture: Start timer while CollectionView dimensions not set.");

                        timerSetCustomPicture = new Timer(
                            new TimerCallback((s) => ShowCustomPictureIfReady()),
                                       null,
                                       TimeSpan.FromMilliseconds(2000),
                                       TimeSpan.FromMilliseconds(2000));
                    }
                }
                else
                {
                    // The CollectionView is reader for the pictures.
                    if (timerSetCustomPicture != null)
                    {
                        timerSetCustomPicture.Dispose();
                        timerSetCustomPicture = null;
                    }

                    ShowCustomPictureInSquaresInBackground();
                }
            }
        }

        private void ShowCustomPictureIfReady()
        {
            if (inShowCustomPictureIfReady)
            {
                return;
            }

            inShowCustomPictureIfReady = true;

            Debug.WriteLine("ShowCustomPictureIfReady: CollectionView dimensions: " +
                SquaresCollectionView.Width + ", " + SquaresCollectionView.Height);

            if ((SquaresCollectionView.Width > 0) && (SquaresCollectionView.Height > 0))
            {
                if (timerSetCustomPicture != null)
                {
                    timerSetCustomPicture.Dispose();
                    timerSetCustomPicture = null;
                }

                ShowCustomPictureInSquaresInBackground();
            }

            inShowCustomPictureIfReady = false;
        }

        private int destGridPortionWidth = 0;
        private int destGridPortionHeight = 0;

        private void ShowCustomPictureInSquaresInBackground()
        {
            var vm = this.BindingContext as SquaresViewModel;

            vm.GameIsLoading = true;

            vm.RaiseNotificationEvent(PleaseWaitLabel.Text);

            destGridPortionWidth = (int)(SquaresCollectionView.Width / 4);
            destGridPortionHeight = (int)(SquaresCollectionView.Height / 4);

            Debug.WriteLine("ShowCustomPictureInSquaresInBackground: Thread.CurrentThread.IsBackground " 
                + Thread.CurrentThread.IsBackground);

            var th = new Thread(ShowCustomPictureInSquares);

            th.IsBackground = true;

            th.Start(this);
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
                if (vm.PicturePathSquares != previousLoadedPicture)
                {
                    originalCustomPictureBitmap = null;
                }

                // Restore the order of the squares in the grid.
                //vm.RestoreEmptyGrid();

                // Check whether the image file exists before trying to load it into the ImageEditor.
                if (vm.IsImageFilePathValid(vm.PicturePathSquares))
                {
                    // Set the images shown on the squares.
                    ShowCustomPicture();

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

                    originalCustomPictureBitmap = null;

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
                    ShowCustomPicture();
                }
                else
                {
                    vm.ResetGrid();
                }
            }
        }

        private static void ShowCustomPictureInSquares(Object obj)
        {
            Debug.WriteLine("ShowCustomPictureInSquares: Thread.CurrentThread.IsBackground "
                + Thread.CurrentThread.IsBackground);

            var page = obj as SquaresPage;

            var vm = page.BindingContext as SquaresViewModel;

            string picturePathSquares = vm.PicturePathSquares;
            if (String.IsNullOrWhiteSpace(picturePathSquares))
            {
                return;
            }

            Debug.WriteLine("ShowCustomPictureInSquares: Loading pictures into squares now.");

            Debug.WriteLine("ShowCustomPictureInSquares: Thread.CurrentThread.IsBackground " + 
                Thread.CurrentThread.IsBackground);

            try
            {
                if (page.originalCustomPictureBitmap == null)
                {
                    using (Stream fileStream = File.OpenRead(picturePathSquares))
                    {
                        using (SKManagedStream originalStream = new SKManagedStream(fileStream))
                        {
                            page.originalCustomPictureBitmap = SKBitmap.Decode(originalStream);

                            originalStream.Dispose();
                        }

                        fileStream.Close();
                    }
                }

                // Leave the 16th square empty.
                for (int i = 0; i < 15; ++i)
                {
                    vm.SquareListCollection[i].PictureImageSource = page.GetImageSourceForSquare(
                                                                        vm.SquareListCollection[i].TargetIndex);
                }

                vm.SquareListCollection[15].PictureImageSource = ImageSource.FromFile("emptysquare.jpg");
            }
            catch (Exception ex) 
            {
                Debug.WriteLine("Load custom picture: " + ex.Message);
            }

            // Now jumble the squares.
            vm.ResetGrid();

            Debug.WriteLine("ShowCustomPictureInSquares: Done loading pictures into squares.");

            vm.GameIsLoading = false;
        }

        private ImageSource GetImageSourceForSquare(int index)
        {
            var sourceImagePortionWidth = (int)(originalCustomPictureBitmap.Width / 4);
            var sourceImagePortionHeight = (int)(originalCustomPictureBitmap.Height / 4);

            Debug.WriteLine("Picture portioning: Source " +
                sourceImagePortionWidth + ", " + sourceImagePortionHeight + ", Dest " +
                destGridPortionWidth + ", " + destGridPortionHeight);

            SKRect destRect = new SKRect(0, 0, destGridPortionWidth, destGridPortionHeight);

            int col = index % 4;
            int row = index / 4;

            SKRect sourceRect = new SKRect(
                col * sourceImagePortionWidth, 
                row * sourceImagePortionHeight, 
                (col + 1) * sourceImagePortionWidth, 
                (row + 1) * sourceImagePortionHeight);

            SKBitmap destBitmap = new SKBitmap(destGridPortionWidth, destGridPortionHeight);

            using (SKCanvas canvas = new SKCanvas(destBitmap))
            {
                canvas.DrawBitmap(originalCustomPictureBitmap, sourceRect, destRect);
            }

            return (SKBitmapImageSource)destBitmap;
        }
    }
}
