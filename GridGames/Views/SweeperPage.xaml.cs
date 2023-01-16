using GridGames.ResX;
using GridGames.ViewModels;
using InvokePlatformCode.Services.PartialMethods;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;
using Border = Microsoft.Maui.Controls.Border;

namespace GridGames.Views
{
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

            //GBTEST WelcomeBorder.Loaded += WelcomeBorder_Loaded;

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

            // IMPORTANT! TalkBack usage with a tapped handler is unreliable on Android, so don't use it.
            //https://github.com/xamarin/Xamarin.Forms/issues/9991
            //[Bug] Tap gesture recognizer doesn't fire in android with screen reader enabled #9991

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
                    SquaresWelcomeTitleLabel.Text + ", " +
                    SquaresWelcomeTitleInstructions.Text,
                    4000);
                */

                SweeperCollectionView.IsVisible = false;

                //GBTEST WelcomeMessageCloseButton.Focus();
            }
        }

        private async void SweeperCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    var vm = this.BindingContext as SweeperViewModel;

                    var item = collectionView.SelectedItem as SweeperViewModel.Square;
                    if (item != null)
                    {
                        ActOnInput(item.AccessibleName);
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

            var vm = this.BindingContext as SweeperViewModel;
            if (vm.FirstRunSweeper) //GBTEST  || vm.GameIsLoading)
            {
                return;
            }

            var itemBorder = (Border)sender;
            var itemAccessibleName = SemanticProperties.GetDescription(itemBorder);

            Debug.WriteLine("Grid Games: Tapped on Square " + itemAccessibleName);

            ActOnInput(itemAccessibleName);
        }

        private async void ActOnInput(string itemName)
        {
            var vm = this.BindingContext as SweeperViewModel;

            int itemIndex = GetItemCollectionIndexFromItemAccessibleName(itemName);
            if (itemIndex != -1)
            {
                if (IsFirstTurnUp())
                {
                    vm.InitialiseGrid(itemIndex);
                }

                bool gameIsOver = vm.ActOnInputOnSquare(itemIndex);
                if (gameIsOver)
                {
                    await DisplayAlert(
                                    "Leaf Sweeper",
                                    "Sorry, game over!",
                                    "OK");
                }
                else
                {
                    await IsGameWon();
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
                for (int i = 0; i < 16; ++i)
                {
                    if (vm.SweeperListCollection[i].HasFrog)
                    {
                        vm.SweeperListCollection[i].AccessibleName = "Frog";
                    }
                }

                await DisplayAlert(
                                "Leaf Sweeper",
                                "Congratulations, you won!",
                                "OK");
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
            if (vm.FirstRunSweeper) //GBTEST  || vm.GameIsLoading)
            {
                return;
            }

            int itemIndex = GetItemCollectionIndexFromItemAccessibleName(item.AccessibleName);
            if (itemIndex != -1)
            {
                if (IsFirstTurnUp())
                {
                    vm.InitialiseGrid(itemIndex);
                }

                bool gameIsOver = vm.ActOnInputOnSquare(itemIndex);
                if (gameIsOver)
                {
                    await DisplayAlert(
                                    "Leaf Sweeper",
                                    "Sorry, game over!",
                                    "OK");
                }
                else
                {
                    await IsGameWon();
                }
            }
        }

        // This gets called when switching to the Squares Game from other games,
        // and also when closing the Squares Settings page.
        protected override void OnAppearing()
        {
            Debug.WriteLine("Sweeper Game: OnAppearing called.");

            base.OnAppearing();

            Debug.WriteLine("Sweeper Game: Done base OnAppearing.");

            Preferences.Set("InitialGame", "Sweeper");

            // Account for the app settings changing since the page was last shown.
            var vm = this.BindingContext as SweeperViewModel;

            vm.FirstRunSweeper = false; // Preferences.Get("FirstRunSweeper", true);

            var isFirstRun = vm.FirstRunSweeper;

            if (vm.FirstRunSweeper)
            {
                //GBTEST 
                //vm.RaiseNotificationEvent(
                //    SquaresWelcomeTitleLabel.Text + ", " + SquaresWelcomeTitleInstructions.Text);
            }

            var currentTheme = Application.Current.UserAppTheme;
            if (currentTheme == AppTheme.Unspecified)
            {
                currentTheme = Application.Current.PlatformAppTheme;
            }

            vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);

            //GBTEST vm.GameIsLoading = false;
        }

        private int GetItemCollectionIndexFromItemAccessibleName(string ItemAccessibleName)
        {
            var vm = this.BindingContext as SweeperViewModel;

            int itemIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (vm.SweeperListCollection[i].AccessibleName == ItemAccessibleName)
                {
                    itemIndex = i;
                    break;
                }
            }

            return itemIndex;
        }

        public void ShowContextMenu()
        {
#if WINDOWS
            var square = SweeperCollectionView.SelectedItem as SweeperViewModel.Square;
            if (square != null)
            {
                int borderCount = 0;

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

                                    break;
                                }
                            }
                        }

                        ++borderCount;
                    }
                }
            }
#endif
        }

        public void PlantFlag()
        {
            var item = SweeperCollectionView.SelectedItem as SweeperViewModel.Square;
            if (item != null)
            {
                PlantFlagInItem(item.TargetIndex);
            }
        }

        public void PlantFlagInItem(int itemIndex)
        {
            var vm = this.BindingContext as SweeperViewModel;

            if (itemIndex != -1)
            {
                if (vm.SweeperListCollection[itemIndex].ShowsFlag)
                {
                    vm.SweeperListCollection[itemIndex].ShowsFlag = false;
                }
                else
                {
                    vm.SweeperListCollection[itemIndex].ShowsFlag = true;

                    vm.SweeperListCollection[itemIndex].AccessibleName = "Query frog";
                }
            }
        }


        private async Task OfferToRestartGame()
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                var message = "";

                message = String.Format(
                    AppResources.ResourceManager.GetString("SquaresWonInGoes"), 8 + vm.MoveCount);

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
            var vm = this.BindingContext as SweeperViewModel;
            vm.FirstRunSweeper = false;

            SweeperCollectionView.IsVisible = true;
        }

        private async void SquaresGameSettingsButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                var settingsPage = new SquaresSettingsPage();
                await Navigation.PushModalAsync(settingsPage);
            }
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
                vm.ResetGrid();
            }
        }

        private void MenuFlyoutItem_Clicked(object sender, EventArgs e)
        {
            var menuItem = (sender as MenuFlyoutItem).Parent;

            var square = menuItem.BindingContext as SweeperViewModel.Square;

            PlantFlagInItem(square.TargetIndex);
        }
    }
}
