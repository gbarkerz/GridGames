using GridGames.ResX;
using GridGames.ViewModels;
using InvokePlatformCode.Services.PartialMethods;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SquaresPage : ContentPage
    {
        public static DateTime timeOfMostRecentSelectionChanged = DateTime.Now;

        // Barker: IMPORTANT.
        // While most of the binding of CollectionView item properties works great,
        // in tests, binding of IsVisible was not 100% robust. That is, most times
        // when two items were swapped in the collection while the Squares game is 
        // played, the binding worked as expected, sometimes the binding on one or
        // other items did not take effect. Presumably this is because my code was
        // not in compliance with binding requirements (eg related to threading),
        // but so far I've not been able to pinpoint the cause of the issue. So for 
        // now, add this temporary "Fixup" code to manually set the visibility on 
        // all the items. This unblocks me while working on the game experience,
        // and I'll revisit this once the full .NET 7.0 is released.

        private void FixupSquaresWithDelay(int delay)
        {
            // Add a small delay after items have been updated, before setting
            // the various item elements' visibility.
            var timer = new Timer(
                new TimerCallback((s) => FixupSquares()),
                           null,
                           TimeSpan.FromMilliseconds(delay),
                           TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        private void FixupSquares()
        {
            try
            {
                // Always run this on the UI thread.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var vm = this.BindingContext as SquaresViewModel;

                    // Set the visibility of the items' images and labels as appropriate.
                    var descendants = SquaresCollectionView.GetVisualTreeDescendants();
                    for (int i = 0; i < descendants.Count; ++i)
                    {
                        var descendant = descendants[i];
                        if (descendant is Label)
                        {
                            var label = descendant as Label;

                            // The empty square never has its Label visible.
                            label.IsVisible = vm.ShowNumbers && (label.Text != "");
                        }
                        else if (descendant is Image)
                        {
                            var image = descendant as Image;
                            image.IsVisible = vm.ShowPicture;
                        }
                    }
                });
            }
            catch (Exception ex) 
            {
                Debug.WriteLine("Error trying to fix up squares: " + ex.Message);
            }
        }

        // Path to most recently fully loaded picture.
        private string previousLoadedPicture = "";
        private SKBitmap originalCustomPictureBitmap = null;

        private Timer timerSetCustomPicture;
        private bool inShowCustomPictureIfReady = false;

        private int destGridPortionWidth = 0;
        private int destGridPortionHeight = 0;

        public SquaresPage()
        {
            InitializeComponent();

#if IOS
            SemanticProperties.SetDescription(WelcomeBorder, null);
#endif

            WelcomeBorder.Loaded += WelcomeBorder_Loaded;

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

            // IMPORTANT! TalkBack usage with a tapped handler is unreliable on Android, so don't use it.
            //https://github.com/xamarin/Xamarin.Forms/issues/9991
            //[Bug] Tap gesture recognizer doesn't fire in android with screen reader enabled #9991

            SquaresCollectionView.SelectionChanged += SquaresCollectionView_SelectionChanged;

            SquaresCollectionView.DescendantAdded += SquaresCollectionView_DescendantAdded;

#if IOS
            // At this time, VoiceOver won't navigate to the items in a CollectionView
            // if the CollectionView has a SemanticProperties.Description. So for now,
            // remove the Description on iOS.
            SemanticProperties.SetDescription(SquaresCollectionView, null);
#endif
        }

        private void WelcomeBorder_Loaded(object sender, EventArgs e)
        {
            if ((sender as Border).IsVisible)
            {
                /*
                var vm = this.BindingContext as MatchingViewModel;

                vm.RaiseDelayedNotificationEvent(
                    SquaresWelcomeTitleLabel.Text + ", " +
                    SquaresWelcomeTitleInstructions.Text,
                    4000);
                */

                SquaresCollectionView.IsVisible = false;

                WelcomeMessageCloseButton.Focus();
            }
        }

        private async void SquaresCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            timeOfMostRecentSelectionChanged = DateTime.Now;

            // If this selection change is very likely due to a use of an Arrow key to move
            // between squares, do nothing here. If instead the selection change is more
            // likely due to programmatic selection via a screen reader, attempt to move 
            // the square.
            var timeSinceMostRecentArrowKeyPress = DateTime.Now - MauiProgram.timeOfMostRecentArrowKeyPress;
            if (timeSinceMostRecentArrowKeyPress.TotalMilliseconds < 100)
            {
                return;
            }

            var collectionView = sender as CollectionView;
            if (collectionView != null)
            {
                if (collectionView.SelectedItem != null)
                {
                    bool gameIsWon = AttemptMoveBySelection(collectionView);

                    FixupSquaresWithDelay(200);

                    if (gameIsWon)
                    {
                        await OfferToRestartGame();
                    }
                }
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var timeSinceMostRecentSelectionChanged = DateTime.Now - timeOfMostRecentSelectionChanged;
            if (timeSinceMostRecentSelectionChanged.TotalMilliseconds < 100)
            {
                return;
            }

            var vm = this.BindingContext as SquaresViewModel;
            if (vm.FirstRunSquares || vm.GameIsLoading)
            {
                return;
            }

            var itemBorder = (Border)sender;
            var itemAccessibleName = SemanticProperties.GetDescription(itemBorder);

            Debug.WriteLine("Grid Games: Tapped on Square " + itemAccessibleName);

            AttemptToMoveSquareByName(itemAccessibleName);
        }

        private async void AttemptToMoveSquareByName(string itemName)
        {
            var vm = this.BindingContext as SquaresViewModel;

            int itemIndex = GetItemCollectionIndexFromItemAccessibleName(itemName);
            if (itemIndex != -1)
            {
                bool gameIsWon = AttemptToMoveSquare(itemIndex);
                if (gameIsWon)
                {
                    await OfferToRestartGame();
                }
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
                bool gameIsWon = AttemptToMoveSquare(itemIndex);
                if (gameIsWon)
                {
                    await OfferToRestartGame();
                }
            }
        }

        private bool AttemptToMoveSquare(int itemIndex)
        {
            var vm = this.BindingContext as SquaresViewModel;

            bool squareSwapped;
            int emptySquareIndex;

            bool gameIsWon = vm.AttemptToMoveSquare(itemIndex, 
                                                    out squareSwapped,
                                                    out emptySquareIndex);

            SetHelpTextData(squareSwapped, itemIndex, emptySquareIndex);

            return gameIsWon;
        }

        private bool AttemptMoveBySelection(CollectionView collectionView)
        {
            var vm = this.BindingContext as SquaresViewModel;

            bool squareSwapped;
            int itemIndex;
            int emptySquareIndex;

            bool gameIsWon = vm.AttemptMoveBySelection(collectionView.SelectedItem,
                                                       out squareSwapped,
                                                       out itemIndex,
                                                       out emptySquareIndex);

            SetHelpTextData(squareSwapped, itemIndex, emptySquareIndex);

            return gameIsWon;
        }

        private void SetHelpTextData(bool squareSwapped, int itemIndex, int emptySquareIndex)
        {
#if WINDOWS
            if (!squareSwapped)
            {
                return;
            }

            var vm = this.BindingContext as SquaresViewModel;

            var countItemsTotal = vm.SquareListCollection.Count;
            var countItemsInRow = (int)Math.Sqrt(countItemsTotal);

            var platformAction = new GridGamesPlatformAction();

            int row = (itemIndex / countItemsInRow) + 1;
            int column = (itemIndex % countItemsInRow) + 1;

            platformAction.SetGridItemCollectionViewAccessibleData(SquaresCollectionView, 
                                                                   itemIndex,
                                                                   row,
                                                                   column);

            row = (emptySquareIndex / countItemsInRow) + 1;
            column = (emptySquareIndex % countItemsInRow) + 1;

            platformAction.SetGridItemCollectionViewAccessibleData(SquaresCollectionView,
                                                                   emptySquareIndex,
                                                                   row,
                                                                   column);
#endif
        }

        private void SquaresCollectionView_DescendantAdded(object sender, ElementEventArgs e)
        {
            // Manually setting the visibility of various elements after updating the 
            // CollectionView is a temporary measure and will be removed once the full
            // .NET 7.0 is available.
            FixupSquares();
        }

        private void ShowCustomPicture()
        {
            Debug.WriteLine("ShowCustomPicture Start.");

            var vm = this.BindingContext as SquaresViewModel;

            if (vm.ShowPicture && !String.IsNullOrWhiteSpace(vm.PicturePathSquares))
            {
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

                    ShowCustomPictureInSquares();
                }
            }

            Debug.WriteLine("ShowCustomPicture Done.");
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

                ShowCustomPictureInSquares();
            }

            inShowCustomPictureIfReady = false;
        }

        // This gets called when switching to the Squares Game from other games,
        // and also when closing the Squares Settings page.
        protected override void OnAppearing()
        {
            Debug.WriteLine("Squares Game: OnAppearing called.");

            base.OnAppearing();

            Debug.WriteLine("Squares Game: Done base OnAppearing.");

            Preferences.Set("InitialGame", "Squares");

            // Account for the app settings changing since the page was last shown.
            var vm = this.BindingContext as SquaresViewModel;

            vm.FirstRunSquares = Preferences.Get("FirstRunSquares", true);

            var isFirstRun = vm.FirstRunSquares;

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

            bool loadedCustomPicture = false;

            Debug.WriteLine("Squares Game: vm.PicturePathSquares: " + vm.PicturePathSquares);

            // Has the state of the picture being shown changed since we were last changed?
            if (vm.ShowPicture && (vm.PicturePathSquares != null) &&
                (vm.PicturePathSquares != previousLoadedPicture))
            {
                if (vm.PicturePathSquares != previousLoadedPicture)
                {
                    originalCustomPictureBitmap = null;
                }

                // Restore the order of the squares in the grid.
                vm.RestoreEmptyGrid();

                // Check whether the image file exists before trying to load it into the ImageEditor.
                if (vm.IsImageFilePathValid(vm.PicturePathSquares))
                {
                    loadedCustomPicture = true;

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
            else
            {
                if (isFirstRun)
                {
                    vm.ResetGrid();
                }

#if WINDOWS
                timer = new Timer(
                    new TimerCallback((s) => SetRowColumnData()),
                               null,
                               TimeSpan.FromMilliseconds(2000),
                               TimeSpan.FromMilliseconds(Timeout.Infinite));
#endif

                vm.GameIsLoading = false;
            }

            if (!loadedCustomPicture)
            {
                FixupSquaresWithDelay(500);
            }
        }

        private Timer timer;

        private void SetRowColumnData()
        {
#if WINDOWS
            timer.Dispose();

            var platformAction = new GridGamesPlatformAction();
            platformAction.SetGridCollectionViewAccessibleData(SquaresCollectionView, false);
#endif
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

        private void WelcomeMessageCloseButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;
            vm.FirstRunSquares = false;

            SquaresCollectionView.IsVisible = true;
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

        private async void ShowCustomPictureInSquares()
        {
            var vm = this.BindingContext as SquaresViewModel;

            vm.GameIsLoading = true;

            vm.RaiseNotificationEvent(PleaseWaitLabel.Text);

            // Load the image on a background thread to give a chance for the
            // "Please wait" message to show up on the UI thread.

            // Barker: Make all this more robust
            await Task.Run(async () =>
            {
                Thread.Sleep(1000);

                destGridPortionWidth = (int)(SquaresCollectionView.Width / 4);
                destGridPortionHeight = (int)(SquaresCollectionView.Height / 4);

                string picturePathSquares = vm.PicturePathSquares;
                if (!String.IsNullOrWhiteSpace(picturePathSquares))
                {
                    Debug.WriteLine("ShowCustomPictureInSquares: Loading pictures into squares now.");

                    try
                    {
                        if (originalCustomPictureBitmap == null)
                        {
                            using (Stream fileStream = File.OpenRead(picturePathSquares))
                            {
                                using (SKManagedStream originalStream = new SKManagedStream(fileStream))
                                {
                                    originalCustomPictureBitmap = SKBitmap.Decode(originalStream);

                                    originalStream.Dispose();
                                }

                                fileStream.Close();
                            }
                        }

                        // Leave the 16th square empty.
                        for (int i = 0; i < 15; ++i)
                        {
                            vm.SquareListCollection[i].PictureImageSource = GetImageSourceForSquare(
                                                                                vm.SquareListCollection[i].TargetIndex);
                        }

                        vm.SquareListCollection[15].PictureImageSource = ImageSource.FromFile("emptysquare.jpg");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Load custom picture: " + ex.Message);
                    }
                }

                // Now jumble the squares.
                vm.ResetGrid();

                Debug.WriteLine("ShowCustomPictureInSquares: Done loading pictures into squares.");

                vm.GameIsLoading = false;

                vm.RaiseDelayedNotificationEvent(
                    AppResources.ResourceManager.GetString("GameReady"),
                    1000);

#if WINDOWS
                var platformAction = new GridGamesPlatformAction();
                platformAction.SetGridCollectionViewAccessibleData(SquaresCollectionView, false);
#endif
            });
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
