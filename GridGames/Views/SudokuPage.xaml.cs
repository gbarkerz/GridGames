using CommunityToolkit.Maui.Views;
using GridGames.ResX;
using GridGames.ViewModels;
using System.Diagnostics;
using Application = Microsoft.Maui.Controls.Application;

#if WINDOWS
using Windows.System;
#endif

namespace GridGames.Views;

public partial class SudokuPage : ContentPage
{
    // Create a bindable ItemRow property to set the height of the CollectionView items
    // in the main Sudoku grid.
    public static readonly BindableProperty ItemRowHeightProperty =
        BindableProperty.Create(nameof(ItemRowHeight), typeof(int), typeof(SudokuPage));

    public int ItemRowHeight
    {
        get => (int)GetValue(SudokuPage.ItemRowHeightProperty);
        set => SetValue(SudokuPage.ItemRowHeightProperty, value);
    }

    public static DateTime timeOfMostRecentProgrammaticSelection = DateTime.Now;

    public SudokuPage()
    {
        InitializeComponent();

#if WINDOWS
        // This static is used when the page is called by Windows platform Gamepad-related code.
        sudokuPage = this;

        GameTitleLabel.HorizontalOptions = LayoutOptions.Center;
#endif

#if IOS
        SemanticProperties.SetDescription(WelcomeBorder, null);
#endif

        SudokuCollectionView.Loaded += SudokuCollectionView_Loaded;

        SudokuCollectionView.SelectionChanged += SudokuCollectionView_SelectionChanged;

        SudokuCollectionView.Focused += SudokuCollectionView_Focused;
#if IOS
        // At this time, VoiceOver won't navigate to the items in a CollectionView
        // if the CollectionView has a SemanticProperties.Description. So for now,
        // remove the Description on iOS.
        SemanticProperties.SetDescription(SudokuCollectionView, null);
#endif
    }

    private void SudokuCollectionView_Focused(object sender, FocusEventArgs e)
    {
#if WINDOWS
        // If the grid's getting focus and no square is currently selected, select the 
        // first square now. This might happen when tabbing into the grid after the 
        // game's first started or when it's been restarted. 
        if (SudokuCollectionView.SelectedItem == null)
        {
            SetItemSelection(0);
        }
#endif
    }

    private void SudokuCollectionView_Loaded(object sender, EventArgs e)
    {
#if WINDOWS
        var squareLocationAnnouncementFormat = (string)Preferences.Get(
                                                "SquareLocationAnnouncementFormat",
                                                AppResources.ResourceManager.GetString("SquareLocationAnnouncementDefault"));

        var platformAction = new GridGamesPlatformAction();
        platformAction.SetGridCollectionViewAccessibleData(SudokuCollectionView, true, squareLocationAnnouncementFormat);

        platformAction.PrepareGamepadUsage();
#endif
    }

    protected override void OnAppearing()
    {
        Debug.WriteLine("Sudoku: OnAppearing called.");

        base.OnAppearing();

        Preferences.Set("InitialGame", "Sudoku");

        var vm = this.BindingContext as SudokuViewModel;

        vm.FirstRunSudoku = Preferences.Get("FirstRunSudoku", true);

        var currentTheme = Application.Current.UserAppTheme;
        if (currentTheme == AppTheme.Unspecified)
        {
            currentTheme = Application.Current.PlatformAppTheme;
        }

        vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);

        vm.SudokuSettingsVM.BlankSquareCount = (int)Preferences.Get("BlankSquareCount", 10);

        if (vm.EmptySquareIndicatorIsX != vm.SudokuSettingsVM.EmptySquareIndicatorIsX)
        {
            vm.EmptySquareIndicatorIsX = vm.SudokuSettingsVM.EmptySquareIndicatorIsX;

            // Update the accessible names of all the squares based on the current
            // game setting relating to whether an 'x' is shown in empty squares.
            for (int i = 0; i < 81; ++i)
            {
                if (!vm.SudokuListCollection[i].NumberShown)
                {
                    vm.SudokuListCollection[i].RefreshAccessibleName();
                }
            }
        }

        if (vm.SquareLocationAnnouncementFormat != vm.SudokuSettingsVM.SquareLocationAnnouncementFormat)
        {
            vm.SquareLocationAnnouncementFormat = vm.SudokuSettingsVM.SquareLocationAnnouncementFormat;

            var squareLocationAnnouncementFormat = (string)Preferences.Get(
                                                    "SquareLocationAnnouncementFormat",
                                                    AppResources.ResourceManager.GetString("SquareLocationAnnouncementDefault"));
#if WINDOWS
            var platformAction = new GridGamesPlatformAction();
            platformAction.SetGridCollectionViewAccessibleData(SudokuCollectionView, true, squareLocationAnnouncementFormat);
#endif
        }
    }

    private async void SudokuSettingsButton_Clicked(object sender, EventArgs e)
    {
        var vm = this.BindingContext as SudokuViewModel;
        if (!vm.FirstRunSudoku)
        {
            var settingsPage = new SudokuSettingsPage(vm.SudokuSettingsVM);
            await Navigation.PushModalAsync(settingsPage);
        }
    }

    public async void ReactToKeyInputOnSelectedCard()
    {
        var item = SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
        if (item != null)
        {
            ClearSquare(item);
        }
    }

    private void SudokuCollectionView_SelectionChanged(object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        var timeSinceMostRecentProgrammaticSelection = DateTime.Now - timeOfMostRecentProgrammaticSelection;

        // If we're here because the game itself has programmatically set the current selection
        // in the grid, do not attempt to clear the selected square.

        Debug.WriteLine("SudokuCollectionView_SelectionChanged: Time since most recent programmatic selection " +
            timeSinceMostRecentProgrammaticSelection.TotalMilliseconds);

        if (timeSinceMostRecentProgrammaticSelection.TotalMilliseconds < 200)
        {
            return;
        }

        // If this selection change is very likely due to a use of an Arrow key to move
        // between squares, do nothing here.
        var timeSinceMostRecentArrowKeyPress = DateTime.Now - MauiProgram.timeOfMostRecentArrowKeyPress;

        Debug.WriteLine("SudokuCollectionView_SelectionChanged: Time since most recent press " +
            timeSinceMostRecentArrowKeyPress.TotalMilliseconds);

        if (timeSinceMostRecentArrowKeyPress.TotalMilliseconds < 200)
        {
            return;
        }

        var collectionView = sender as CollectionView;
        if (collectionView != null)
        {
            var item = SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
            if (item != null)
            {
                ClearSquare(item);
            }
        }
    }

    private async void ClearSquare(SudokuViewModel.Square item)
    {
        if (item != null)
        {
            if (item.FixedNumber)
            {
                await DisplayAlert(
                    "Sudoku",
                    "This square is fixed, and cannot be cleared.",
                    "OK");

                return;
            }

            if (item.NumberShown)
            {
                var vm = this.BindingContext as SudokuViewModel;
                ++vm.CurrentBlankSquareCount;

                item.NumberShown = false;
                item.Number = item.OriginalNumber;

                var msg = "Now " + item.AccessibleName;
                vm.RaiseNotificationEvent(msg);
            }
#if !WINDOWS
            else
            {
                var popup = new SudokuInputPopup();

                var result = await this.ShowPopupAsync(popup) as string;
                if ((result != null) && (result != ""))
                {
                    var vm = this.BindingContext as SudokuViewModel;
                    if (!item.NumberShown) 
                    {
                        --vm.CurrentBlankSquareCount;

                        item.NumberShown = true;
                    }

                    item.Number = result.ToString();

                    var msg = "Now " + item.AccessibleName;
                    vm.RaiseNotificationEvent(msg);

                    bool gameWon;

                    if (vm.IsGridFilled(out gameWon)) {
                        var answer = await DisplayAlert(
                            "Sudoku",
                            (gameWon ? "Congratulations, you won the game!" : "Sorry, the grid is not correct.") +
                                " Would you like another game?",
                            AppResources.ResourceManager.GetString("Yes"),
                            AppResources.ResourceManager.GetString("No"));
                        if (answer) {
                            RestartGame();
                        }
                    }
                }
            }

            // Unselect all items in order for the next tap to select an item and
            // always trigger a reaction even if it's the same as the most recent 
            // tapped item.
            SudokuCollectionView.SelectedItem = null;
#endif
        }
    }

#if WINDOWS
    // If keyboard focus is at the start or end of a row in the grid, don't move to 
    // an adjacent row in response to a press of a left or right arrow key press.
    public void HandleLeftRightArrow(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        var square = SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
        if (square != null)
        {
            bool isStartOfRow = (square.Index % 9) == 0;
            bool isEndOfRow = (square.Index % 9) == 8;

            if ((isStartOfRow && (e.Key == Windows.System.VirtualKey.Left)) ||
                (isEndOfRow && (e.Key == Windows.System.VirtualKey.Right)))
            {
                e.Handled = true;
            }
        }
    }

    public async void HandleNumberInput(Windows.System.VirtualKey key)
    {
        var item = SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
        if (item == null)
        {
            return;
        }

        if (item.FixedNumber)
        {
            await DisplayAlert(
                "Sudoku",
                "This square is fixed, and cannot be changed.",
                "OK");

            return;
        }

        int number;

        switch (key)
        {
            case Windows.System.VirtualKey.Number1:
            case Windows.System.VirtualKey.NumberPad1:

                number = 1;
                break;

            case Windows.System.VirtualKey.Number2:
            case Windows.System.VirtualKey.NumberPad2:

                number = 2;
                break;

            case Windows.System.VirtualKey.Number3:
            case Windows.System.VirtualKey.NumberPad3:

                number = 3;
                break;

            case Windows.System.VirtualKey.Number4:
            case Windows.System.VirtualKey.NumberPad4:

                number = 4;
                break;

            case Windows.System.VirtualKey.Number5:
            case Windows.System.VirtualKey.NumberPad5:

                number = 5;
                break;

            case Windows.System.VirtualKey.Number6:
            case Windows.System.VirtualKey.NumberPad6:

                number = 6;
                break;

            case Windows.System.VirtualKey.Number7:
            case Windows.System.VirtualKey.NumberPad7:

                number = 7;
                break;

            case Windows.System.VirtualKey.Number8:
            case Windows.System.VirtualKey.NumberPad8:

                number = 8;
                break;

            default:

                number = 9;
                break;
        }

        var vm = this.BindingContext as SudokuViewModel;
        if (!item.NumberShown)
        {
            --vm.CurrentBlankSquareCount;

            item.NumberShown = true;
        }

        item.Number = number.ToString();

        bool gameWon;

        if (vm.IsGridFilled(out gameWon))
        {
            var answer = await DisplayAlert(
                "Sudoku",
                (gameWon ? "Congratulations, you won the game!" : "Sorry, the grid is not correct.") +
                    " Would you like another game?",
                AppResources.ResourceManager.GetString("Yes"),
                AppResources.ResourceManager.GetString("No"));
            if (answer)
            {
                RestartGame();
            }
        }
        else
        {
            var msg = "Now " + item.AccessibleName;
            vm.RaiseNotificationEvent(msg);
        }
    }

#endif 

    public async void ShowHelp()
    {
        var vm = this.BindingContext as SudokuViewModel;
        if (!vm.FirstRunSudoku)
        {
            await Navigation.PushModalAsync(new HelpPage(this));
        }
    }

    private Timer timer;

    public void RestartGame()
    {
        var vm = this.BindingContext as SudokuViewModel;
        if (!vm.FirstRunSudoku)
        {
#if WINDOWS
            SetItemSelection(-1);
#endif

            Debug.WriteLine("RestartGame: Reset view model list.");

            vm.ResetGrid();

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

        var squareLocationAnnouncementFormat = (string)Preferences.Get(
                                                "SquareLocationAnnouncementFormat",
                                                AppResources.ResourceManager.GetString("SquareLocationAnnouncementDefault"));
        var platformAction = new GridGamesPlatformAction();
        platformAction.SetGridCollectionViewAccessibleData(SudokuCollectionView, true, squareLocationAnnouncementFormat);
#endif
    }

    private void WelcomeMessageCloseButton_Clicked(object sender, EventArgs e)
    {
        var vm = this.BindingContext as SudokuViewModel;
        vm.FirstRunSudoku = false;

        var readyMessage = AppResources.ResourceManager.GetString("SudokuReadyToPlay");
        vm.RaiseNotificationEvent(readyMessage);
    }

#if WINDOWS
    public void RespondToArrowPress(VirtualKey key, bool jumpToDifferentStateSquare)
    {
        Debug.WriteLine("RespondToArrowPress: JumpToDifferentStateSquare " + jumpToDifferentStateSquare);

        var square = SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
        if (square == null)
        {
            return;
        }

        bool needsResponse = false;

        if (jumpToDifferentStateSquare)
        {
            bool doneMove;

            JumpToDifferentStateSquare(key, square, out doneMove);
            if (!doneMove)
            {
                needsResponse = true;
            }
        }
        else
        {
            var index = square.Index;

            // Are we trying to moving beyond the start or end of a row or column?
            if (((index % 9 == 0) && (key == VirtualKey.Left)) ||
                ((index % 9 == 8) && (key == VirtualKey.Right)) ||
                ((index < 9) && (key == VirtualKey.Up)) ||
                ((index > 71) && (key == VirtualKey.Down)))
            {
                needsResponse = true;
            }
        }

        if (needsResponse)
        {
            var vm = this.BindingContext as SudokuViewModel;

            string announcement = "";

            if ((vm.SudokuSettingsVM.SudokuNoMoveResponse == (int)SudokuNoMoveResponseChoices.Announcement) ||
                (vm.SudokuSettingsVM.SudokuNoMoveResponse == (int)SudokuNoMoveResponseChoices.PlaySoundAndAnnouncement))
            {
                var resMgr = AppResources.ResourceManager;

                if (!jumpToDifferentStateSquare)
                {
                    announcement = resMgr.GetString("EdgeOfBoard");
                }
                else
                {
                    announcement = String.Format(
                        resMgr.GetString("JumpToDifferentStateNotPossible"),
                        resMgr.GetString(
                            square.NumberShown ? "Empty" : "Numbered"));
                }
            }

            switch (vm.SudokuSettingsVM.SudokuNoMoveResponse)
            {
                case (int)SudokuNoMoveResponseChoices.PlaySound:

                    mediaElement.Stop();
                    mediaElement.Play();

                    break;

                case (int)SudokuNoMoveResponseChoices.Announcement:

                    vm.RaiseNotificationEvent(announcement);

                    break;

                case (int)SudokuNoMoveResponseChoices.PlaySoundAndAnnouncement:

                    mediaElement.Stop();
                    mediaElement.Play();

                    vm.RaiseNotificationEvent(announcement);

                    break;

                case (int)SudokuNoMoveResponseChoices.NoResponse:
                default:

                    break;
            }
        }
    }

    private void JumpToDifferentStateSquare(VirtualKey key, SudokuViewModel.Square square, out bool doneMove)
    {
        doneMove = false;

        var vm = this.BindingContext as SudokuViewModel;

        var index = square.Index;

        int rowValue = index / 9;

        if (key == VirtualKey.Left)
        {
            int indexStartOfRow = 9 * rowValue;

            for (int i = index - 1; i >= indexStartOfRow; --i)
            {
                if (square.NumberShown != vm.SudokuListCollection[i].NumberShown)
                {
                    SetItemSelection(i);

                    doneMove = true;

                    break;
                }
            }
        }
        else if (key == VirtualKey.Right)
        {
            int indexEndOfRow = (9 * rowValue) + 8;

            for (int i = index + 1; i <= indexEndOfRow; ++i)
            {
                if (square.NumberShown != vm.SudokuListCollection[i].NumberShown)
                {
                    SetItemSelection(i);

                    doneMove = true;

                    break;
                }
            }
        }
        else if (key == VirtualKey.Up)
        {
            for (int i = index - 9; i >= 0; i -= 9)
            {
                if (square.NumberShown != vm.SudokuListCollection[i].NumberShown)
                {
                    SetItemSelection(i);

                    doneMove = true;

                    break;
                }
            }
        }
        else if (key == VirtualKey.Down)
        {
            for (int i = index + 9; i < 81; i += 9)
            {
                if (square.NumberShown != vm.SudokuListCollection[i].NumberShown)
                {
                    SetItemSelection(i);

                    doneMove = true;

                    break;
                }
            }
        }
    }

    private void SetItemSelection(int selectedItemIndex)
    {
        timeOfMostRecentProgrammaticSelection = DateTime.Now;

        SetItemSelectionHighlight(SudokuCollectionView, false);

        if (selectedItemIndex == -1)
        {
            SudokuCollectionView.SelectedItem = null;
        }
        else
        {
            var vm = this.BindingContext as SudokuViewModel;

            SudokuCollectionView.SelectedItem = vm.SudokuListCollection[selectedItemIndex];

            SetItemSelectionHighlight(SudokuCollectionView, true);
        }
    }

    public void SetItemSelectionHighlight(CollectionView collectionView, bool isSelected)
    {
#if WINDOWS
        try
        {
            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Force a square's context menu to appear.
                var square = collectionView.SelectedItem as SudokuViewModel.Square;
                if (square != null)
                {
                    int borderCount = 0;

                    // First find the main container Border associated with the selected square.
                    var gridDescendants = collectionView.GetVisualTreeDescendants();
                    for (int i = 0; i < gridDescendants.Count; ++i)
                    {
                        var gridDescendant = gridDescendants[i];
                        if (gridDescendant is Microsoft.Maui.Controls.Border)
                        {
                            if (borderCount == square.Index)
                            {
                                // Ok, we've found the Border for the square of interest.
                                var gridItemDescendants = gridDescendant.GetVisualTreeDescendants();

                                var element = gridDescendant as VisualElement;

                                VisualStateManager.GoToState(
                                    element, isSelected ? "Selected" : "Normal");
                            }

                            ++borderCount;
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SetItemSelectionHighlight: " + ex.Message);
        }
#endif
    }

    public void AnnounceRCGDetails(VirtualKey key)
    {
        Debug.WriteLine("AnnounceRCGDetails: key " + key);

        var square = SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
        if (square == null)
        {
            return;
        }

        var vm = this.BindingContext as SudokuViewModel;

        var index = square.Index;

        int rowValue = index / 9;
        int columnIndex = index % 9;

        int[] missingNumberArray = new int[9];

        var resMgr = AppResources.ResourceManager;

        int countMissingNumbers = 0;

        string announcement = "";

        if (key == VirtualKey.R)
        {
            int indexStartOfRow = 9 * rowValue;

            for (int i = indexStartOfRow; i < indexStartOfRow + 9; ++i)
            {
                if (!vm.SudokuListCollection[i].NumberShown)
                {
                    missingNumberArray[countMissingNumbers] =
                        Int32.Parse(vm.SudokuListCollection[i].Number);

                    ++countMissingNumbers;
                }
            }

            announcement = resMgr.GetString(countMissingNumbers == 0 ? "Row" : "RowNeeds");
        }
        else if (key == VirtualKey.C)
        {
            int indexStartOfColumn = columnIndex;

            for (int i = indexStartOfColumn; i < 81; i += 9)
            {
                if (!vm.SudokuListCollection[i].NumberShown)
                {
                    missingNumberArray[countMissingNumbers] =
                        Int32.Parse(vm.SudokuListCollection[i].Number);

                    ++countMissingNumbers;
                }
            }

            announcement = resMgr.GetString(countMissingNumbers == 0 ? "Column" : "ColumnNeeds");
        }
        else if (key == VirtualKey.G)
        {
            int indexFirstRowInGroup = 3 * (rowValue / 3);
            int indexColumnRowInGroup = 3 * (columnIndex / 3);

            int indexStartOfGroupSection = (indexFirstRowInGroup * 9) + indexColumnRowInGroup;

            for (int j = 0; j < 3; ++j)
            {
                for (int i = indexStartOfGroupSection; i < indexStartOfGroupSection + 3; ++i)
                {
                    if (!vm.SudokuListCollection[i].NumberShown)
                    {
                        missingNumberArray[countMissingNumbers] =
                            Int32.Parse(vm.SudokuListCollection[i].Number);

                        ++countMissingNumbers;
                    }
                }

                indexStartOfGroupSection += 9;
            }

            announcement = resMgr.GetString(countMissingNumbers == 0 ? "Group" : "GroupNeeds");
        }

        if (countMissingNumbers == 0)
        {
            announcement += resMgr.GetString("NoneMissing");
        }
        else
        {
            Array.Sort(missingNumberArray);

            for (int i = 0; i < 9; ++i)
            {
                if (missingNumberArray[i] != 0)
                {
                    announcement += missingNumberArray[i].ToString();

                    if (i < 8)
                    {
                        announcement += ", ";
                    }
                }
            }
        }

        vm.RaiseNotificationEvent(announcement);
    }


    public void HandleNavigationKey(VirtualKey key)
    {
        Debug.WriteLine("HandleNavigationKey: key " + key);

        var square = SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
        if (square == null)
        {
            return;
        }

        var vm = this.BindingContext as SudokuViewModel;

        var index = square.Index;

        int rowValue = index / 9;
        int columnValue = index % 9;

        int indexTargetSquare = -1;

        if (key == VirtualKey.Home)
        {
            indexTargetSquare = 9 * rowValue;
        }
        else if (key == VirtualKey.End)
        {
            indexTargetSquare = (9 * rowValue) + 8;
        }
        else if (key == VirtualKey.PageUp)
        {
            indexTargetSquare = columnValue;
        }
        else if (key == VirtualKey.PageDown)
        {
            indexTargetSquare = 72 + columnValue;
        }

        if (indexTargetSquare != -1)
        {
            SetItemSelection(indexTargetSquare);
        }
    }

    public void AnnounceNumberPresence(VirtualKey key)
    {
        Debug.WriteLine("AnnounceNumberPresence: key " + key);

        var square = SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
        if (square == null)
        {
            return;
        }

        int number = 0;

        switch (key)
        {
            case VirtualKey.Number1:
            case VirtualKey.NumberPad1:
                number = 1;
                break;

            case VirtualKey.Number2:
            case VirtualKey.NumberPad2:
                number = 2;
                break;

            case VirtualKey.Number3:
            case VirtualKey.NumberPad3:
                number = 3;
                break;

            case VirtualKey.Number4:
            case VirtualKey.NumberPad4:
                number = 4;
                break;

            case VirtualKey.Number5:
            case VirtualKey.NumberPad5:
                number = 5;
                break;

            case VirtualKey.Number6:
            case VirtualKey.NumberPad6:
                number = 6;
                break;

            case VirtualKey.Number7:
            case VirtualKey.NumberPad7:
                number = 7;
                break;

            case VirtualKey.Number8:
            case VirtualKey.NumberPad8:
                number = 8;
                break;

            case VirtualKey.Number9:
            case VirtualKey.NumberPad9:
                number = 9;
                break;

            default:
                break;
        }

        if (number == 0)
        {
            return;
        }

        var vm = this.BindingContext as SudokuViewModel;

        var index = square.Index;

        int rowValue = index / 9;
        int columnIndex = index % 9;

        var resMgr = AppResources.ResourceManager;

        // First check the current row.
        int indexStartOfRow = 9 * rowValue;

        bool rowShowsNumber = false;

        for (int i = indexStartOfRow; i < indexStartOfRow + 9; ++i)
        {
            if (vm.SudokuListCollection[i].NumberShown &&
                (Int32.Parse(vm.SudokuListCollection[i].Number) == number))
            {
                rowShowsNumber = true;

                break;
            }
        }

        // Next check the current column.
        int indexStartOfColumn = columnIndex;

        bool columnShowsNumber = false;

        for (int i = indexStartOfColumn; i < 81; i += 9)
        {
            if (vm.SudokuListCollection[i].NumberShown &&
                (Int32.Parse(vm.SudokuListCollection[i].Number) == number))
            {
                columnShowsNumber = true;

                break;
            }
        }

        // Last check the current group.

        int indexFirstRowInGroup = 3 * (rowValue / 3);
        int indexColumnRowInGroup = 3 * (columnIndex / 3);

        int indexStartOfGroupSection = (indexFirstRowInGroup * 9) + indexColumnRowInGroup;

        bool groupShowsNumber = false;

        for (int j = 0; j < 3; ++j)
        {
            for (int i = indexStartOfGroupSection; i < indexStartOfGroupSection + 3; ++i)
            {
                if (vm.SudokuListCollection[i].NumberShown &&
                    (Int32.Parse(vm.SudokuListCollection[i].Number) == number))
                {
                    groupShowsNumber = true;

                    break;
                }
            }

            if (groupShowsNumber)
            {
                break;
            }

            indexStartOfGroupSection += 9;
        }

        string announcement;

        // Barker Todo: Tidy up all the announcement string building below.
        if (rowShowsNumber || columnShowsNumber || groupShowsNumber)
        {
            announcement = String.Format(
                resMgr.GetString("NumberShownPrefix"),
                number);

            if (rowShowsNumber)
            {
                announcement += "row";

                if (columnShowsNumber)
                {
                    if (groupShowsNumber)
                    {
                        announcement += ", column, and group";
                    }
                    else
                    {
                        announcement += " and column";
                    }
                }
                else
                {
                    if (groupShowsNumber)
                    {
                        announcement += " and group";
                    }
                }
            }
            else
            {
                if (columnShowsNumber)
                {
                    if (groupShowsNumber)
                    {
                        announcement += "column and group";
                    }
                    else
                    {
                        announcement += "column";
                    }
                }
                else
                {
                    if (groupShowsNumber)
                    {
                        announcement += "group";
                    }
                }
            }

            announcement += ".";
        }
        else
        {
            announcement = String.Format(
                resMgr.GetString("NumberNotShownInRowColumnGroup"),
                number);
        }

        vm.RaiseNotificationEvent(announcement);
    }
#endif

    private void SpeechTargetButton_Clicked(object sender, EventArgs e)
    {
        var speechTargetButton = sender as Microsoft.Maui.Controls.Button;

        // The accessible name of the button will be "Select square " followed by 
        // the 1-based index of the square in the grid.
        var accessibleName = SemanticProperties.GetDescription(speechTargetButton);

        var speechTargetButtonPrefix = AppResources.ResourceManager.GetString("SpeechTargetButtonPrefix");

        var itemIndexString = accessibleName.Replace(speechTargetButtonPrefix + " ", "");

        int itemIndex = int.Parse(itemIndexString) - 1;

        // If the square isn't already selected, select it now.
        var vm = this.BindingContext as SudokuViewModel;

        if (SudokuCollectionView.SelectedItem != vm.SudokuListCollection[itemIndex])
        {
            Debug.WriteLine("SpeechTargetButton_Clicked: Select square " + itemIndex);

#if WINDOWS
            SetItemSelection(itemIndex);
#else
            SudokuCollectionView.SelectedItem = vm.SudokuListCollection[itemIndex];
#endif
        }
    }

    private void SudokuCollectionView_SizeChanged(object sender, EventArgs e)
    {
        var collectionView = (sender as CollectionView);
        var collectionViewHeight = collectionView.Height;

        var scrollViewHeight = SudokuGridScrollView.Height;

        // We only need to set the item height such that the grid fills the main area on the page
        // if the grid cannot be scrolled. If the grid can scroll then allow the default item sizing
        // and ScrollView behavior.
        if ((collectionViewHeight > 0) && (scrollViewHeight > 0) &&
            (collectionViewHeight == scrollViewHeight))
        {
            // Space available for items is: (Note CollectionView doesn't support padding.)
            // collection view height - (2 x 2 x wide gap between rows) - (6 x 2 x narrow gap between rows)
            var availableSpace = (int)collectionViewHeight - (2 * 2 * 4) - (6 * 2 * 1);

            this.ItemRowHeight = availableSpace / 9;
        }
    }

    // The remainder of this file contains code to react to Gamepad input when running on Windows.

    // Important: This code is here only to raise the discussion around how a Gamepad might be used 
    // to play a game of Sudoku. The feature is not complete. For example, it is not possible to move
    // keyboard focus into the grid, or to restart a game once the game is completed. 

    // Also, the app polls for the state of the gamepad buttons, which is not at all what I'd like to
    // be doing. I've not yet found a way to add gamepad button event handles to a MAUI app as it can
    // be done in a UWP XAML app. I found no way of getting a MAUI app's KeyDown handler to be called 
    // following a gamepad button press. Interesting this Microsoft resource also mentions use of 
    // polling to get the gamepad state...
    // https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/reference/input/gameinput/interfaces/igameinput/methods/igameinput_getcurrentreading

#if WINDOWS

    public static SudokuPage sudokuPage;
    private static SudokuInputPopup currentPopup;

    // Handle a Gamepad button being pressed.
    static public void HandleGamepadButtonInput(VirtualKey key)
    {
        Debug.WriteLine("HandleGamepadButtonInput: Key " + key.ToString());

        // If the Sudoku input popup is up, have the Gamepad input trigger action in the popup.
        if (currentPopup != null)
        {
            HandleGamepadButtonInputAtPopup(key);

            return;
        }

        // Have the Gamepad input trigger action in the main Sudoku grid.
        if ((sudokuPage.SudokuCollectionView != null) &&
            (sudokuPage.SudokuCollectionView.SelectedItem != null))
        {
            var square = sudokuPage.SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
            if (square != null)
            {
                if (key == VirtualKey.Space)
                {
                    try
                    {
                        // Always run this on the UI thread.
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            currentPopup = new SudokuInputPopup();

                            var result = await sudokuPage.ShowPopupAsync(currentPopup) as string;
                            if (result != "")
                            {
                                var vm = sudokuPage.BindingContext as SudokuViewModel;

                                if (!square.NumberShown)
                                {
                                    --vm.CurrentBlankSquareCount;

                                    square.NumberShown = true;
                                }

                                square.Number = result.ToString();

                                var msg = "Now " + square.AccessibleName;
                                vm.RaiseNotificationEvent(msg);

                                bool gameWon;

                                if (vm.IsGridFilled(out gameWon))
                                {
                                    var answer = await sudokuPage.DisplayAlert(
                                        "Sudoku",
                                        (gameWon ? "Congratulations, you won the game!" : "Sorry, the grid is not correct.") +
                                            " Would you like another game?",
                                        AppResources.ResourceManager.GetString("Yes"),
                                        AppResources.ResourceManager.GetString("No"));
                                    if (answer)
                                    {
                                        sudokuPage.RestartGame();
                                    }
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("HandleGamepadButtonInput: Exception " + ex.Message);
                    }
                }
                else
                {
                    int targetIndex = -1;

                    if (key == VirtualKey.Left)
                    {
                        bool isStartOfRow = (square.Index % 9) == 0;
                        if (!isStartOfRow)
                        {
                            targetIndex = square.Index - 1;
                        }
                    }
                    else if (key == VirtualKey.Right)
                    {
                        bool isEndOfRow = (square.Index % 9) == 8;
                        if (!isEndOfRow)
                        {
                            targetIndex = square.Index + 1;
                        }
                    }
                    else if (key == VirtualKey.Up)
                    {
                        if (square.Index > 8)
                        {
                            targetIndex = square.Index - 9;
                        }
                    }
                    else if (key == VirtualKey.Down)
                    {
                        if (square.Index < 72)
                        {
                            targetIndex = square.Index + 9;
                        }
                    }

                    if (targetIndex != -1)
                    {
                        var vm = sudokuPage.BindingContext as SudokuViewModel;

                        try
                        {
                            // Always run this on the UI thread.
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                timeOfMostRecentProgrammaticSelection = DateTime.Now;

                                sudokuPage.SetItemSelection(targetIndex);
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("HandleGamepadButtonInput: Exception " + ex.Message);
                        }
                    }
                }
            }
        }
    }

    static public void HandleGamepadButtonInputAtPopup(VirtualKey key)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var vm = sudokuPage.BindingContext as SquaresViewModel;

            var descendants = currentPopup.GetVisualTreeDescendants();

            bool focusNextButton = false;

            for (int i = 0; i < descendants.Count; ++i)
            {
                var descendant = descendants[i];
                if (descendant is Microsoft.Maui.Controls.Button)
                {
                    var button = descendant as Microsoft.Maui.Controls.Button;

                    if (focusNextButton)
                    {
                        button.Focus();

                        break;
                    }
                    else if (button.IsFocused)
                    {
                        if (key == VirtualKey.Space)
                        {
                            currentPopup.Close(button.Text.StartsWith("Close") ? "" : button.Text);

                            currentPopup = null;
                        }
                        else
                        {
                            focusNextButton = true;
                        }
                    }
                }
            }
        });
    }

#endif

}
