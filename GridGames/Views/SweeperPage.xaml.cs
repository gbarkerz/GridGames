﻿using CommunityToolkit.Maui.Views;
using GridGames.ResX;
using GridGames.ViewModels;
using System.Diagnostics;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SweeperPage : ContentPage
    {
        // Create a bindable ItemRow property to set the height of the CollectionView items
        // in the main Sweeper grid.
        public static readonly BindableProperty ItemRowHeightProperty =
            BindableProperty.Create(nameof(ItemRowHeight), typeof(int), typeof(SudokuPage));

        public int ItemRowHeight
        {
            get => (int)GetValue(SweeperPage.ItemRowHeightProperty);
            set => SetValue(SweeperPage.ItemRowHeightProperty, value);
        }

        public static readonly BindableProperty ItemRowWidthProperty =
            BindableProperty.Create(nameof(ItemRowWidth), typeof(int), typeof(SudokuPage));

        public int ItemRowWidth
        {
            get => (int)GetValue(SweeperPage.ItemRowWidthProperty);
            set => SetValue(SweeperPage.ItemRowWidthProperty, value);
        }

        // Create a bindable ItemFontSize property to set the size of the CollectionView item font
        // in the main Sudoku grid.
        public static readonly BindableProperty ItemFontSizeProperty =
            BindableProperty.Create(nameof(ItemFontSize), typeof(int), typeof(SudokuPage));

        public int ItemFontSize
        {
            get => (int)GetValue(SudokuPage.ItemFontSizeProperty);
            set => SetValue(SudokuPage.ItemFontSizeProperty, value);
        }

        public static DateTime timeOfMostRecentSelectionChanged = DateTime.Now;

        private int previousSideLength;
        private int previousFrogCount;
        private bool firstRunThisInstance = true;

        // In order to set the grid item size as required (ie the default size when the CollectionView
        // is larger than the containing ScrollView, or larger-than-default size when we have to increase
        // the size of the CollectionView to fill the ScrollView), we need to cache the original size of
        // the grid view items.
        private bool storeOriginalGridItemSize = true;
        private Size originalGridItemSize;

        public SweeperPage()
        {
            InitializeComponent();

#if WINDOWS
            GameTitleLabel.HorizontalOptions = LayoutOptions.Center;
#endif

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

            SweeperCollectionView.Loaded += SweeperCollectionView_Loaded;

#if IOS
            // At this time, VoiceOver won't navigate to the items in a CollectionView
            // if the CollectionView has a SemanticProperties.Description. So for now,
            // remove the Description on iOS.
            SemanticProperties.SetDescription(SweeperCollectionView, null);
#endif
        }

        private void SweeperCollectionView_Loaded(object sender, EventArgs e)
        {
            var gameTitleLabel = PageTitleArea.FindByName("GameTitleLabel") as Label;
            if (gameTitleLabel != null)
            {
                this.ItemFontSize = (int)(gameTitleLabel.FontSize * 2);
            }

#if WINDOWS
            var platformAction = new GridGamesPlatformAction();
            platformAction.SetGridCollectionViewAccessibleData(SweeperCollectionView, false, null);
#endif
        }

        // We need to store the original size of the grid items in the event that we increase 
        // the size of the items to fill the area of the containing ScrollView later.
        private void GridItemBorder_SizeChanged(object sender, EventArgs e)
        {
            if (!storeOriginalGridItemSize)
            {
                return;
            }

            var border = sender as Border;

            if ((border.Width > 0) && (border.Height > 0))
            {
                originalGridItemSize.Width = border.Width;
                originalGridItemSize.Height = border.Height;

                Debug.WriteLine("GridItemBorder_SizeChanged: Stored original item width, height " +
                    originalGridItemSize.Width + ", " + originalGridItemSize.Height);

                // We only need to do this once, as all the grid items are the same size
                // and they don't change size unless we choose to change their size later
                // when we get notified of a change in size of the containing CollectionView.

                // *** IMPORTANT *** This assumes the first grid item size change notification
                // arrives before the first CollectionView size change notification.
                storeOriginalGridItemSize = false;
            }
        }

        // Originally the grid to was set to have minimum dimensions set from the size of the 
        // containing ScrollView. That seemed to work except for some reason the TalkBack perf
        // was usable when doing that. So manually set whatever widths and heights of the 
        // CollectionView and its items are necessary here.
        private void SweeperCollectionView_SizeChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("SweeperCollectionView_SizeChanged: SweeperCollectionView Width,Height " +
                SweeperCollectionView.Width + ", " + SweeperGridScrollView.Height);

            // If the CollectionView is smaller that the containing ScrollView, increase the CollectionView
            // dimensions to match the ScrollView. And in that case, also increase the size of the contained
            // items to fill the CollectionView.
            if ((SweeperCollectionView.Width > 0) && (SweeperCollectionView.Height > 0))
            {
                var vm = this.BindingContext as SweeperViewModel;

                if (SweeperCollectionView.Width <= SweeperGridScrollView.Width)
                {
                    SweeperCollectionView.WidthRequest = SweeperGridScrollView.Width;

                    this.ItemRowWidth = (int)(SweeperGridScrollView.Width / vm.SideLength) - 1;
                }

                if (SweeperCollectionView.Height <= SweeperGridScrollView.Height)
                {
                    SweeperCollectionView.HeightRequest = SweeperGridScrollView.Height;

                    this.ItemRowHeight = (int)(SweeperGridScrollView.Height / vm.SideLength) - 1;
                }

                Debug.WriteLine("Leaf Sweeper: SweeperCollectionView_SizeChanged() Item size now: width, height " +
                    this.ItemRowWidth + ", " + this.ItemRowHeight);
            }
        }

        private void WelcomeBorder_Loaded(object sender, EventArgs e)
        {
            if ((sender as Border).IsVisible)
            {
                /*
                var vm = this.BindingContext as SweeperViewModel;

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

        private bool currentlyProcessingInputOnCard = false;

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

            if (currentlyProcessingInputOnCard)
            {
                return;
            }

            currentlyProcessingInputOnCard = true;

#if !WINDOWS
            var popup = new SweeperMarkFrogPopup();

            // Unselect all items in order for the next tap to select an item and
            // always trigger a reaction even if it's the same as the most recent 
            // tapped item.
            SweeperCollectionView.SelectedItem = null;

            var result = await this.ShowPopupAsync(popup) as string;
            if (String.IsNullOrEmpty(result)) 
            {
                currentlyProcessingInputOnCard = false;

                return;
            }

            if (result == "MarkFrog") 
            {
                SetShowsQueryFrogInSquare(item.TargetIndex, !item.ShowsQueryFrog);

                currentlyProcessingInputOnCard = false;

                return;
            }
#endif

            if (IsFirstTurnUp())
            {
                vm.InitialiseGrid(item.TargetIndex);
            }

            // If the card was previously in a Query Frog state, it is no longer so.
            item.ShowsQueryFrog = false;

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

                if (!vm.GameWon)
                {
                    vm.RaiseNotificationEvent("Swept stone " +
                        (item.targetIndex + 1).ToString() +
                        ", " +
                        item.AccessibleName);
                }
            }

            currentlyProcessingInputOnCard = false;
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

            bool updatePreviousValues = false;

            if (firstRunThisInstance)
            {
                updatePreviousValues = true;

                RestartGame(firstRunThisInstance);

                firstRunThisInstance = false;
            }

            if (((previousSideLength > 0) && (sideLength != previousSideLength)) ||
                ((previousFrogCount > 0) && (frogCount != previousFrogCount)))
            {
                Debug.WriteLine("Leaf Sweeper: OnAppearing() Restore original grid item size, width, height " +
                    this.originalGridItemSize.Width + ", " + originalGridItemSize.Height);

                this.ItemRowWidth = (int)originalGridItemSize.Width;
                this.ItemRowHeight = (int)originalGridItemSize.Height;

                // Now set the CollectionView based on the size of the contained grid items.
                // Both the item size and CollectionView size will be increased later if
                // necessary to fill the containing ScrollView.
                SweeperCollectionView.WidthRequest = this.ItemRowWidth * sideLength;
                SweeperCollectionView.HeightRequest = this.ItemRowHeight * sideLength;

                updatePreviousValues = true;

                vm.SweeperListCollection.Clear();

                SweeperCollectionView.ItemsLayout = new GridItemsLayout(
                    sideLength, ItemsLayoutOrientation.Vertical);

                RestartGame(firstRunThisInstance);
            }

            if (updatePreviousValues)
            {
                previousSideLength = sideLength;
                previousFrogCount = frogCount;

                vm.SideLength = sideLength;

                vm.SweeperSettingsVM.SideLength = sideLength;
                vm.SweeperSettingsVM.FrogCount = frogCount;
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

            SweeperCollectionView.SelectedItem = null;
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

#if WINDOWS
        // If keyboard focus is at the start or end of a row in the grid, don't move to 
        // an adjacent row in response to a press of a left or right arrow key press.
        public void HandleLeftRightArrow(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var square = SweeperCollectionView.SelectedItem as SweeperViewModel.Square;
            if (square != null)
            {
                var vm = this.BindingContext as SweeperViewModel;

                bool isStartOfRow = (square.TargetIndex % vm.SweeperSettingsVM.SideLength) == 0;
                bool isEndOfRow = ((square.TargetIndex % vm.SweeperSettingsVM.SideLength) == 
                                        vm.SweeperSettingsVM.SideLength - 1);

                if ((isStartOfRow && (e.Key == Windows.System.VirtualKey.Left)) ||
                    (isEndOfRow && (e.Key == Windows.System.VirtualKey.Right)))
                {
                    e.Handled = true;
                }
            }
        }
#endif
        public void RestartGame(bool suppressAnnouncement)
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                vm.ResetGrid();

                // Important: Do not set these to false until after the grid has been reset. 
                // Otherwise if a tap of a square is made after they're set to false, yet
                // before the grid is reset, the tap will be processed as though the game is 
                // in progress. Instead we must ignore all touch input until the game has 
                // been restarted.
                vm.GameWon = false;
                vm.GameLost = false;


#if WINDOWS
                timer = new Timer(
                    new TimerCallback((s) => SetRowColumnData()),
                               null,
                               TimeSpan.FromMilliseconds(500),
                               TimeSpan.FromMilliseconds(Timeout.Infinite));
#endif

                SweeperCollectionView.SelectedItem = null;

                if (!suppressAnnouncement)
                {
                    vm.RaiseNotificationEvent("Leaf Sweeper game restarted.");
                }
            }
        }

        private void SetRowColumnData()
        {
#if WINDOWS
            timer.Dispose();

            var platformAction = new GridGamesPlatformAction();
            platformAction.SetGridCollectionViewAccessibleData(SweeperCollectionView, false, null);
#endif
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
