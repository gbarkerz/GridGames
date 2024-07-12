using GridGames.ResX;
using GridGames.ViewModels;
using InvokePlatformCode.Services.PartialMethods;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MatchingPage : ContentPage
    {
        private bool previousShowCustomPictures;
        private string previousPicturePathMatching;
        private int previousGridSizeScale = 0;
        private bool firstRunThisInstance = true;
        private bool hasSettingsWindowAppeared = false;

        public static DateTime timeOfMostRecentSelectionChanged = DateTime.Now;

        public MatchingPage()
        {
            InitializeComponent();

#if WINDOWS
            GameTitleLabel.HorizontalOptions = LayoutOptions.Center;
#endif

#if IOS
            SemanticProperties.SetDescription(WelcomeBorder, null);
#endif

            WelcomeBorder.Loaded += WelcomeBorder_Loaded;

            PairsCollectionView.SizeChanged += PairsCollectionView_SizeChanged;

            Application.Current.RequestedThemeChanged += (s, a) =>
            {
                var currentTheme = a.RequestedTheme;
                if (currentTheme == AppTheme.Unspecified)
                {
                    currentTheme = Application.Current.PlatformAppTheme;
                }

                var vm = this.BindingContext as MatchingViewModel;
                vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);
            };

            (this.BindingContext as MatchingViewModel).SetMatchingPage(this);

            PairsCollectionView.SelectionChanged += PairsCollectionView_SelectionChanged;

            PairsCollectionView.Loaded += PairsCollectionView_Loaded;
#if IOS
            // At this time, VoiceOver won't navigate to the items in a CollectionView
            // if the CollectionView has a SemanticProperties.Description. So for now,
            // remove the Description on iOS.
            SemanticProperties.SetDescription(PairsCollectionView, null);
#endif
        }

        private void PairsCollectionView_Loaded(object sender, EventArgs e)
        {
#if WINDOWS
            var platformAction = new GridGamesPlatformAction();
            platformAction.SetGridCollectionViewAccessibleData(PairsCollectionView, false, null);
#endif
        }

        private async void PairsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    var item = PairsCollectionView.SelectedItem as Card;
                    if (item != null)
                    {
                        await ReactToInputOnCard(item.Index);
                    }
                }
            }
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var timeSinceMostRecentSelectionChanged = DateTime.Now - timeOfMostRecentSelectionChanged;
            if (timeSinceMostRecentSelectionChanged.TotalMilliseconds < 100)
            {
                return;
            }

            int itemIndex = (int)(e as Microsoft.Maui.Controls.TappedEventArgs).Parameter;
            await ReactToInputOnCard(itemIndex);
        }

        public async void ReactToKeyInputOnSelectedCard()
        {
            var item = PairsCollectionView.SelectedItem as Card;
            if (item != null)
            {
                await ReactToInputOnCard(item.Index);
            }
        }

        private async Task ReactToInputOnCard(int itemIndex)
        {
            Debug.WriteLine("Grid Games: Input on Square " + itemIndex);

            var vm = this.BindingContext as MatchingViewModel;
            if (vm.FirstRunMatching)
            {
                return;
            }

            int itemCollectionIndex = GetItemCollectionIndexFromItemIndex(itemIndex);
            if (itemCollectionIndex == -1)
            {
                return;
            }

            bool gameIsWon = vm.AttemptToTurnOverSquare(itemCollectionIndex);
            if (gameIsWon)
            {
                await OfferToRestartGame();
            }
        }

        private void WelcomeBorder_Loaded(object sender, EventArgs e)
        {
            if ((sender as Border).IsVisible)
            {
                /*
                var vm = this.BindingContext as MatchingViewModel;

                vm.RaiseDelayedNotificationEvent(
                    MatchingWelcomeTitleLabel.Text + ", " +
                    MatchingWelcomeTitleInstructions.Text,
                    4000);
                */

                PairsCollectionView.IsVisible = false;

                WelcomeMessageCloseButton.Focus();
            }
        }

        private void PairsCollectionView_SizeChanged(object sender, EventArgs e)
        {
            SetGridSize();
        }

        private void SetGridSize()
        {
            if (PairsCollectionView.Height > 0)
            {
                var collectionViewWidth = (int)PairsCollectionView.Width;
                var scrollViewWidth = (int)PairsGridScrollView.Width;

                Debug.WriteLine("PairsCollectionView_Loaded: collectionViewWidth " +
                    collectionViewWidth + ", scrollViewWidth " + scrollViewWidth);

                if ((collectionViewWidth > 0) && (scrollViewWidth > 0))
                {
                    var vm = this.BindingContext as MatchingViewModel;

                    PairsCollectionView.WidthRequest = (PairsGridScrollView.Width * vm.GridSizeScale) / 100;
                    PairsCollectionView.HeightRequest = (PairsGridScrollView.Height * vm.GridSizeScale) / 100;

                    vm.GridRowHeight = (PairsCollectionView.HeightRequest / 4) - 12;
                }
            }
        }

        private async void MatchingGameSettingsButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                hasSettingsWindowAppeared = true;

                var settingsPage = new MatchingGameSettingsPage();
                await Navigation.PushModalAsync(settingsPage);
            }
        }

        // This gets called when switching from the Squares Game to the Matching Game,
        // and also when closing the Matching Settings page.
        protected override async void OnAppearing()
        {
            Debug.WriteLine("Matching Game: OnAppearing called.");

            base.OnAppearing();

            Preferences.Set("InitialGame", "Pairs");

            // We must account for the app settings changing since the page was last shown.

            var vm = this.BindingContext as MatchingViewModel;

            vm.FirstRunMatching = Preferences.Get("FirstRunMatching", true);

            var currentTheme = Application.Current.UserAppTheme;
            if (currentTheme == AppTheme.Unspecified)
            {
                currentTheme = Application.Current.PlatformAppTheme;
            }

            vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);

            // Default to Fill and Don't Clip.
            vm.PictureAspect = (Aspect)Preferences.Get("PictureAspect", 2);

            // Default to keeping the full grid in view.
            vm.GridSizeScale = (int)Preferences.Get("PairsGridSizeScale", 100);

            if ((previousGridSizeScale != 0) && (previousGridSizeScale != vm.GridSizeScale))
            {
                SetGridSize();
            }

            previousGridSizeScale = vm.GridSizeScale;

            // Has something changed related to custom picture use since the last time
            // we were in OnAppearing()?
            var showCustomPictures = Preferences.Get("ShowCustomPictures", false);
            var picturePathMatching = Preferences.Get("PicturePathMatching", "");

            if (firstRunThisInstance || 
                (showCustomPictures != previousShowCustomPictures) ||
                (picturePathMatching != previousPicturePathMatching))
            {
                firstRunThisInstance = false;

                var customPictureLoadFailed = await SetUpCards();

                // If an attempt to load custom pictures failed, don't cache details
                // which suggest that we are showing custom pictures.
                if (!customPictureLoadFailed)
                {
                    previousShowCustomPictures = showCustomPictures;
                    previousPicturePathMatching = picturePathMatching;
                }
                else
                {
                    previousShowCustomPictures = false;
                    previousPicturePathMatching = "";
                }
            }
        }

        public async Task<bool> SetUpCards()
        {
            bool customPictureLoadFailed = false;

            var vm = this.BindingContext as MatchingViewModel;

            vm.ResetGameStatus();

            var showCustomPictures = Preferences.Get("ShowCustomPictures", false);
            var picturePathMatching = Preferences.Get("PicturePathMatching", "");

            // Should we be showing the default pictures?
            if (!showCustomPictures || String.IsNullOrWhiteSpace(picturePathMatching))
            {
                vm.SetupDefaultMatchingCardList();
            }
            else
            {
                // Attempt to load up the custom pictures and associated accessible data.
                var customPictures = new Collection<Card>();

                bool resetToUseDefaultPictures = false;

                // We should have 8 pairs of cards.
                for (int i = 0; i < 8; i++)
                {
                    // For each of the 2 cards in each pair...
                    for (int j = 0; j < 2; j++)
                    {
                        var card = new Card();

                        // The index is from 1-16.
                        card.Index = (i * 2) + j + 1;

                        string settingName = "Card" + (i + 1) + "Path";
                        var cardPath = Preferences.Get(settingName, "");
                        if (!File.Exists(cardPath))
                        {
                            Debug.WriteLine("Pairs: Card path missing.");

                            resetToUseDefaultPictures = true;

                            break;
                        }

                        try
                        {
                            card.PictureImageSource = ImageSource.FromFile(cardPath);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Pairs: Failed to load image. " + ex.Message);

                            resetToUseDefaultPictures = true;

                            break;
                        }

                        settingName = "Card" + (i + 1) + "Name";
                        card.OriginalAccessibleName = Preferences.Get(settingName, "");
                        if (String.IsNullOrWhiteSpace(card.OriginalAccessibleName))
                        {
                            Debug.WriteLine("Pairs: Accessible name missing.");

                            resetToUseDefaultPictures = true;

                            // If we're not here on startup when the app UI hasn't been fully created 
                            // yet, pop up a message to let the customer know what the problem is.
                            if (hasSettingsWindowAppeared)
                            {
                                var info = new FileInfo(cardPath);

                                await DisplayAlert(
                                    "Problem loading custom pictures",
                                    "The accessible name for the file \"" + info.Name + 
                                        "\" wasn't found in the PairsGamePictureDetails.txt file.",
                                    "Ok");
                            }

                            break;
                        }

                        settingName = "Card" + (i + 1) + "Description";
                        card.OriginalAccessibleDescription = Preferences.Get(settingName, "");

                        vm.SetFaceDownAccessibleDetails(card);

                        customPictures.Add(card);
                    }

                    if (resetToUseDefaultPictures)
                    {
                        break;
                    }
                }

                // Now use the custom pictures is we have all the required data.
                if (!resetToUseDefaultPictures)
                {
                    vm.SetupCustomMatchingCardList(customPictures);
                }
                else
                {
                    vm.SetupDefaultMatchingCardList();
                }

#if WINDOWS
                timer = new Timer(
                    new TimerCallback((s) => SetRowColumnData()),
                               null,
                               TimeSpan.FromMilliseconds(500),
                               TimeSpan.FromMilliseconds(Timeout.Infinite));
#endif

                customPictureLoadFailed = resetToUseDefaultPictures;
            }

            return customPictureLoadFailed;
        }

        public async void ShowHelp()
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                await Navigation.PushModalAsync(new HelpPage(this));
            }
        }

#if WINDOWS
        // If keyboard focus is at the start or end of a row in the grid, don't move to 
        // an adjacent row in response to a press of a left or right arrow key press.
        public void HandleLeftRightArrow(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var square = PairsCollectionView.SelectedItem as Card;
            if (square != null)
            {
                int itemCollectionIndex = GetItemCollectionIndexFromItemIndex(square.Index);

                bool isStartOfRow = (itemCollectionIndex % 4) == 0;
                bool isEndOfRow = (itemCollectionIndex % 4) == 3;

                if ((isStartOfRow && (e.Key == Windows.System.VirtualKey.Left)) ||
                    (isEndOfRow && (e.Key == Windows.System.VirtualKey.Right)))
                {
                    e.Handled = true;
                }
            }
        }
#endif

        private Timer timer;

        public void RestartGame()
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                vm.ResetGrid(true);

#if WINDOWS
                timer = new Timer(
                    new TimerCallback((s) => SetRowColumnData()),
                               null,
                               TimeSpan.FromMilliseconds(500),
                               TimeSpan.FromMilliseconds(Timeout.Infinite));
#endif
            }
        }

        private void SetRowColumnData()
        {
#if WINDOWS
            timer.Dispose();

            var platformAction = new GridGamesPlatformAction();
            platformAction.SetGridCollectionViewAccessibleData(PairsCollectionView, false, null);
#endif
        }

        private async Task OfferToRestartGame()
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                var message = String.Format(
                    AppResources.ResourceManager.GetString("MatchingWonInMoves"), 8 + vm.TryAgainCount);

                var answer = await DisplayAlert(
                    AppResources.ResourceManager.GetString("Congratulations"),
                    message,
                    AppResources.ResourceManager.GetString("Yes"),
                    AppResources.ResourceManager.GetString("No"));
                if (answer)
                {
                    vm.ResetGrid(true);

#if WINDOWS
                    timer = new Timer(
                        new TimerCallback((s) => SetRowColumnData()),
                                   null,
                                   TimeSpan.FromMilliseconds(500),
                                   TimeSpan.FromMilliseconds(Timeout.Infinite));
#endif
                }
            }
        }

        public int GetItemCollectionIndexFromItemIndex(int itemIndex)
        {
            var vm = this.BindingContext as MatchingViewModel;

            int itemCollectionIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (vm.SquareListCollection[i].Index == itemIndex)
                {
                    itemCollectionIndex = i;
                    break;
                }
            }

            return itemCollectionIndex;
        }

        private void WelcomeMessageCloseButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as MatchingViewModel;
            vm.FirstRunMatching = false;

            PairsCollectionView.IsVisible = true;

            vm.RaiseNotificationEvent("The Pairs game is ready to play!");
        }
    }
}
