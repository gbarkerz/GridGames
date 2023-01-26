using GridGames.ResX;
using GridGames.ViewModels;
using InvokePlatformCode.Services.PartialMethods;
using System.Diagnostics;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SweeperPage : ContentPage
    {
        public static DateTime timeOfMostRecentSelectionChanged = DateTime.Now;

        private int previousSideLength;
        private int previousFrogCount;
        private bool firstRunThisInstance = true;

        public SweeperPage()
        {
            InitializeComponent();

#if IOS
            SemanticProperties.SetDescription(WelcomeBorder, null);
#endif

            WelcomeBorder.Loaded += WelcomeBorder_Loaded;

            var vm = this.BindingContext as SweeperViewModel;

            SweeperCollectionView.ItemsLayout = new GridItemsLayout(
                vm.SweeperSettingsVM.SideLength, ItemsLayoutOrientation.Vertical);

            Application.Current.RequestedThemeChanged += (s, a) =>
            {
                var currentTheme = a.RequestedTheme;
                if (currentTheme == AppTheme.Unspecified)
                {
                    currentTheme = Application.Current.PlatformAppTheme;
                }

                vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);
            };

            SweeperCollectionView.SelectionChanged += SweeperCollectionView_SelectionChanged;

            SweeperCollectionView.SelectedItem = null;

#if IOS
            // At this time, VoiceOver won't navigate to the items in a CollectionView
            // if the CollectionView has a SemanticProperties.Description. So for now,
            // remove the Description on iOS.
            SemanticProperties.SetDescription(SweeperCollectionView, null);
#endif
        }

        private void WelcomeBorder_Loaded(object sender, EventArgs e)
        {
            if ((sender as Border).IsVisible)
            {
                /*
                var vm = this.BindingContext as MatchingViewModel;

                vm.RaiseDelayedNotificationEvent(
                    SweeperWelcomeTitleLabel.Text + ", " +
                    SweeperWelcomeTitleInstructions.Text,
                    4000);
                */

                SweeperCollectionView.IsVisible = false;

                WelcomeMessageCloseButton.Focus();
            }
        }

        private async void SweeperCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Sweeper: SweeperCollectionView_SelectionChanged");

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
                    Debug.WriteLine("Sweeper: Have selected item.");

                    var item = collectionView.SelectedItem as SweeperViewModel.Square;
                    if (item != null)
                    {
                        Debug.WriteLine("Sweeper: Selected item item.ShowsQueryFrog " + item.ShowsQueryFrog);

                        await ReactToInputOnCard(item);
                    }
                }
            }
        }

        private bool IsFirstTurnUp()
        {
            var vm = this.BindingContext as SweeperViewModel;

            bool isFirstTurnUp = true;

            for (int i = 0; i < (vm.SweeperSettingsVM.SideLength * vm.SweeperSettingsVM.SideLength); ++i)
            {
                if (vm.SweeperListCollection[i].TurnedUp)
                {
                    isFirstTurnUp = false;

                    break;
                }
            }

            return isFirstTurnUp;
        }

        private async Task IsGameWon()
        {
            var vm = this.BindingContext as SweeperViewModel;

            int turnedUpCount = 0;

            for (int i = 0; i < (vm.SweeperSettingsVM.SideLength * vm.SweeperSettingsVM.SideLength); ++i)
            {
                if (vm.SweeperListCollection[i].TurnedUp)
                {
                    ++turnedUpCount;
                }
            }

            if (turnedUpCount == (vm.SweeperSettingsVM.SideLength * vm.SweeperSettingsVM.SideLength) - vm.SweeperSettingsVM.FrogCount)
            {
                vm.GameWon = true;

                for (int i = 0; i < (vm.SweeperSettingsVM.SideLength * vm.SweeperSettingsVM.SideLength); ++i)
                {
                    // We know where all the frogs are now.
                    vm.SweeperListCollection[i].ShowsQueryFrog = false;
                }

                await OfferToRestartWonGame();
            }
        }

        public async void ReactToKeyInputOnSelectedCard()
        {
            var item = SweeperCollectionView.SelectedItem as SweeperViewModel.Square;
            if (item != null)
            {
                await ReactToInputOnCard(item);
            }
        }

        private async Task ReactToInputOnCard(SweeperViewModel.Square item)
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (vm.FirstRunSweeper)
            {
                return;
            }

            if (vm.GameOver)
            {
                return;
            }

            if (IsFirstTurnUp())
            {
                vm.InitialiseGrid(item.targetIndex);
            }

            bool gameIsLost = vm.ActOnInputOnSquare(item.targetIndex);
            if (gameIsLost)
            {
                for (int i = 0; i < (vm.SweeperSettingsVM.SideLength * vm.SweeperSettingsVM.SideLength); ++i)
                {
                    vm.SweeperListCollection[i].TurnedUp = true;
                    vm.SweeperListCollection[i].ShowsQueryFrog = false;
                }

                vm.GameLost = true;

                await OfferToRestartLostGame();
            }
            else
            {
                await IsGameWon();
            }
        }

        // This gets called when switching to the Sweeper Game from other games.
        protected override void OnAppearing()
        {
            Debug.WriteLine("Sweeper Game: OnAppearing called.");

            base.OnAppearing();

            Debug.WriteLine("Sweeper Game: Done base OnAppearing.");

            Preferences.Set("InitialGame", "Sweeper");

            var vm = this.BindingContext as SweeperViewModel;

            vm.FirstRunSweeper = Preferences.Get("FirstRunSweeper", true);

            if (vm.FirstRunSweeper)
            {
                vm.RaiseNotificationEvent(
                    SweeperWelcomeTitleLabel.Text + ", " + SweeperWelcomeTitleInstructions.Text);
            }

            var currentTheme = Application.Current.UserAppTheme;
            if (currentTheme == AppTheme.Unspecified)
            {
                currentTheme = Application.Current.PlatformAppTheme;
            }

            vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);

            var sideLength = (int)Preferences.Get("SideLength", 4);
            var frogCount = (int)Preferences.Get("FrogCount", 2);

            if (firstRunThisInstance ||
                (sideLength != previousSideLength) ||
                (frogCount != previousFrogCount))
            {
                RestartGame(firstRunThisInstance);

                firstRunThisInstance = false;

                previousSideLength = sideLength;
                previousFrogCount = frogCount;
            }
        }

        public void ShowContextMenu()
        {
            var vm = this.BindingContext as SweeperViewModel;

            if (vm.GameOver)
            {
                return;
            }

#if WINDOWS
            // Force a square's context menu to appear.
            var square = SweeperCollectionView.SelectedItem as SweeperViewModel.Square;
            if (square != null)
            {
                int borderCount = 0;

                bool shownContextMenu = false;

                // First find the main container Border associated with the selected square.
                var gridDescendants = SweeperCollectionView.GetVisualTreeDescendants();
                for (int i = 0; i < gridDescendants.Count; ++i)
                {
                    var gridDescendant = gridDescendants[i];
                    if (gridDescendant is Border)
                    {
                        if (borderCount == square.targetIndex)
                        {
                            // Ok, we've found the Border for the square of interest.
                            var gridItemDescendants = gridDescendant.GetVisualTreeDescendants();

                            // Now find the border in the square with which the context menu is associated.
                            for (int j = 0; j < gridItemDescendants.Count; ++j)
                            {
                                if (gridItemDescendants[j] is Border)
                                {
                                    var borderWithContextMenu = gridItemDescendants[j] as Border;

                                    var contextFlyout = FlyoutBase.GetContextFlyout(borderWithContextMenu);

                                    // Now show the context menu next to the square of interest.
                                    var platformAction = new GridGamesPlatformAction();
                                    platformAction.ShowFlyout(contextFlyout, borderWithContextMenu);

                                    shownContextMenu = true;

                                    break;
                                }
                            }
                        }

                        if (shownContextMenu)
                        {
                            break;
                        }

                        ++borderCount;
                    }
                }
            }
#endif
        }

        public void SetShowsQueryFrogInSquare(int itemIndex, bool showQueryFrog)
        {
            Debug.WriteLine("SetShowsQueryFrogInSquare: itemIndex, showQueryFrog " + 
                itemIndex + ", " + showQueryFrog);

            var vm = this.BindingContext as SweeperViewModel;

            if (itemIndex >= 0)
            {
                vm.SweeperListCollection[itemIndex].ShowsQueryFrog = showQueryFrog;
            }
        }

        private async Task OfferToRestartWonGame()
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                var answer = await DisplayAlert(
                    AppResources.ResourceManager.GetString("Congratulations"),
                    AppResources.ResourceManager.GetString("CompletedSweeper"),
                    AppResources.ResourceManager.GetString("Yes"),
                    AppResources.ResourceManager.GetString("No"));
                if (answer)
                {
                    RestartGame(false);
                }
            }
        }

        private async Task OfferToRestartLostGame()
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                var answer = await DisplayAlert(
                    AppResources.ResourceManager.GetString("Oops"),
                    AppResources.ResourceManager.GetString("FailedSweeper"),
                    AppResources.ResourceManager.GetString("Yes"),
                    AppResources.ResourceManager.GetString("No"));
                if (answer)
                {
                    RestartGame(false);
                }
            }
        }

        private void WelcomeMessageCloseButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SweeperViewModel;
            vm.FirstRunSweeper = false;

            SweeperCollectionView.IsVisible = true;
        }

        public async void ShowHelp()
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                await Navigation.PushModalAsync(new HelpPage(this));

                SweeperCollectionView.Focus();
            }
        }

        public void RestartGame(bool suppressAnnouncement)
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                vm.GameWon = false;
                vm.GameLost = false;

                vm.ResetGrid();

                SweeperCollectionView.SelectedItem = null;

                if (!suppressAnnouncement)
                {
                    vm.RaiseNotificationEvent("Leaf Sweeper game restarted.");
                }
            }
        }

        private void MenuFlyoutItem_Clicked(object sender, EventArgs e)
        {
            var menuItem = (sender as MenuFlyoutItem).Parent;

            var square = menuItem.BindingContext as SweeperViewModel.Square;

            SetShowsQueryFrogInSquare(square.TargetIndex, !square.ShowsQueryFrog);
        }

        private void ToggleQueryFrogButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var square = button.BindingContext as SweeperViewModel.Square;

            SetShowsQueryFrogInSquare(square.TargetIndex, !square.ShowsQueryFrog);
        }

        private async void SweeperGameSettingsButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                var settingsPage = new SweeperGameSettingsPage(vm.SweeperSettingsVM);
                await Navigation.PushModalAsync(settingsPage);
            }
        }
    }
}
