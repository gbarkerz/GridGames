using GridGames.ResX;
using GridGames.ViewModels;
using InvokePlatformCode.Services.PartialMethods;
using System.Diagnostics;

namespace GridGames.Views
{
    // Barker Todo: Fix context menu when invoked through mouse or touch.

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SweeperPage : ContentPage
    {
        public static DateTime timeOfMostRecentSelectionChanged = DateTime.Now;

        public SweeperPage()
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

                var vm = this.BindingContext as SweeperViewModel;
                vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);
            };

            SweeperCollectionView.SelectionChanged += SweeperCollectionView_SelectionChanged;

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

            var collectionView = sender as CollectionView;
            if (collectionView != null)
            {
                if (collectionView.SelectedItem != null)
                {
                    Debug.WriteLine("Sweeper: Have selected item.");

                    var vm = this.BindingContext as SweeperViewModel;

                    var item = collectionView.SelectedItem as SweeperViewModel.Square;
                    if (item != null)
                    {
                        Debug.WriteLine("Sweeper: Selected item item.ShowsQueryFrog " + item.ShowsQueryFrog);

                        SetQueryFrogCheckBox.IsChecked = item.ShowsQueryFrog;

                        // If this selection change is very likely due to a use of an Arrow key to move
                        // between squares, do nothing here. If instead the selection change is more
                        // likely due to programmatic selection via a screen reader, attempt to move 
                        // the square.
                        var timeSinceMostRecentArrowKeyPress = DateTime.Now - MauiProgram.timeOfMostRecentArrowKeyPress;
                        if (timeSinceMostRecentArrowKeyPress.TotalMilliseconds < 100)
                        {
                            return;
                        }

                        await ReactToInputOnCard(item);
                    }
                }
            }
        }

        private bool IsFirstTurnUp()
        {
            var vm = this.BindingContext as SweeperViewModel;

            bool isFirstTurnUp = true;

            for (int i = 0; i < 16; ++i)
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

            for (int i = 0; i < 16; ++i)
            {
                if (vm.SweeperListCollection[i].TurnedUp)
                {
                    ++turnedUpCount;
                }
            }

            if (turnedUpCount == 14)
            {
                vm.GameWon = true;

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

            bool gameIsOver = vm.ActOnInputOnSquare(item.targetIndex);
            if (gameIsOver)
            {
                for (int i = 0; i < 16; ++i)
                {
                    vm.SweeperListCollection[i].TurnedUp = true;
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
        }

        private void SetQueryFrogCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var vm = this.BindingContext as SweeperViewModel;

            if (vm.GameOver)
            {
                return;
            }

            var item = SweeperCollectionView.SelectedItem as SweeperViewModel.Square;
            if (item != null)
            {
                SetShowsQueryFrogInSquare(item.TargetIndex, e.Value);
            }
        }

        public void SetShowsQueryFrogInSquare(int itemIndex, bool showQueryFrog)
        {
            var vm = this.BindingContext as SweeperViewModel;

            if (itemIndex != -1)
            {
                if (SetQueryFrogCheckBox.IsChecked != showQueryFrog)
                {
                    SetQueryFrogCheckBox.IsChecked = showQueryFrog;
                }

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
                    RestartGame();
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
                    RestartGame();
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

        public void RestartGame()
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                vm.GameWon = false;
                vm.GameLost = false;

                vm.ResetGrid();
            }
        }
    }
}
