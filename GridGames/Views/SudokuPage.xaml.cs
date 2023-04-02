using CommunityToolkit.Maui.Views;
using GridGames.ResX;
using GridGames.ViewModels;
using InvokePlatformCode.Services.PartialMethods;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.StartScreen;
using Application = Microsoft.Maui.Controls.Application;

namespace GridGames.Views;

public partial class SudokuPage : ContentPage
{
    public static DateTime timeOfMostRecentSelectionChanged = DateTime.Now;

    public SudokuPage()
	{
		InitializeComponent();

#if WINDOWS
        GameTitleLabel.HorizontalOptions = LayoutOptions.Center;
#endif

#if IOS
        SemanticProperties.SetDescription(WelcomeBorder, null);
#endif

        SudokuCollectionView.Loaded += SudokuCollectionView_Loaded;

        SudokuCollectionView.SelectionChanged += SudokuCollectionView_SelectionChanged;

#if IOS
        // At this time, VoiceOver won't navigate to the items in a CollectionView
        // if the CollectionView has a SemanticProperties.Description. So for now,
        // remove the Description on iOS.
        SemanticProperties.SetDescription(SudokuCollectionView, null);
#endif
    }

    private void SudokuCollectionView_Loaded(object sender, EventArgs e)
    {
#if WINDOWS
        var platformAction = new GridGamesPlatformAction();
        platformAction.SetGridCollectionViewAccessibleData(SudokuCollectionView, true);
#endif
    }

    protected override void OnAppearing()
    {
        Debug.Write("Sudoku: OnAppearing called.");

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
            TurnOverSquare(item);
        }
    }

    private void SudokuCollectionView_SelectionChanged(object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
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
            var item = SudokuCollectionView.SelectedItem as SudokuViewModel.Square;
            if (item != null)
            {
                TurnOverSquare(item);
            }
        }
    }

    private async void TurnOverSquare(SudokuViewModel.Square item)
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

                var msg = "Now " + item.AccessibleName;
                vm.RaiseNotificationEvent(msg);
            }
#if !WINDOWS
            else
            {
                var popup = new SudokuInputPopup();

                var result = await this.ShowPopupAsync(popup) as string;
                if (result != "")
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

        var platformAction = new GridGamesPlatformAction();
        platformAction.SetGridCollectionViewAccessibleData(SudokuCollectionView, true);
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
                    SudokuCollectionView.SelectedItem = vm.SudokuListCollection[i];

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
                    SudokuCollectionView.SelectedItem = vm.SudokuListCollection[i];

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
                    SudokuCollectionView.SelectedItem = vm.SudokuListCollection[i];

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
                    SudokuCollectionView.SelectedItem = vm.SudokuListCollection[i];

                    doneMove = true;

                    break;
                }
            }
        }
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

#endif

    private void SKCanvasView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        var view = sender as SKCanvasView;

        SKPaint paint = new SKPaint();

        // Account for the current app theme being either light or dark.
        paint.Color = (Application.Current.RequestedTheme == AppTheme.Dark ?
                        SKColor.Parse("FFFF00") : SKColor.Parse("2B0B98"));

        // Draw two horizonal bars and two vertical bars.

        float width = view.CanvasSize.Width;
        float height = view.CanvasSize.Height;

        var itemHeight = (float)(9 * ((int)(height) / 9)) - 9;

        // First draw the two horizontal lines.

        var yLine = (float)(3.0 * itemHeight) / 9f;

        SKRect rect = new SKRect(
            0,
            yLine - 2,
            width,
            yLine + 6);

        e.Surface.Canvas.DrawRect(rect, paint);

        yLine = (float)(6.0 * itemHeight) / 9f;

        rect = new SKRect(
            0,
            yLine - 2,
            width,
            yLine + 6);

        e.Surface.Canvas.DrawRect(rect, paint);

        // Next draw the two vertical lines.

        var left = (width / 3) - 3;

        height = itemHeight;

        rect = new SKRect(
            left,
            0,
            left + 8,
            height);

        e.Surface.Canvas.DrawRect(rect, paint);

        rect.Left = (2 * (width / 3)) - 3;
        rect.Right = rect.Left + 8;

        e.Surface.Canvas.DrawRect(rect, paint);
    }
}
