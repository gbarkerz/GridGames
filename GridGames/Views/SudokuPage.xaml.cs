using GridGames.ResX;
using GridGames.ViewModels;
using InvokePlatformCode.Services.PartialMethods;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;
using Application = Microsoft.Maui.Controls.Application;

namespace GridGames.Views;

public partial class SudokuPage : ContentPage
{
    public static DateTime timeOfMostRecentSelectionChanged = DateTime.Now;

    public SudokuPage()
	{
		InitializeComponent();

        SudokuCollectionView.Loaded += SudokuCollectionView_Loaded;

        SudokuCollectionView.SelectionChanged += SudokuCollectionView_SelectionChanged;
    }

    private void SudokuCollectionView_Loaded(object sender, EventArgs e)
    {
#if WINDOWS
        var platformAction = new GridGamesPlatformAction();
        platformAction.SetGridCollectionViewAccessibleData(SudokuCollectionView);
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

    private void SudokuCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
    }

    public async void ShowHelp()
    {
        var vm = this.BindingContext as SudokuViewModel;
        if (!vm.FirstRunSudoku)
        {
            // Todo: Show Sudoku help content here.
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
        platformAction.SetGridCollectionViewAccessibleData(SudokuCollectionView);
#endif
    }

    private void WelcomeMessageCloseButton_Clicked(object sender, EventArgs e)
    {
        var vm = this.BindingContext as SudokuViewModel;
        vm.FirstRunSudoku = false;

        var readyMessage = AppResources.ResourceManager.GetString("SudokuReadyToPlay");
        vm.RaiseNotificationEvent(readyMessage);
    }

    private void SKCanvasView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        var view = sender as SKCanvasView;

        SKPaint paint = new SKPaint();
        paint.Color = SKColor.Parse("000000");

        float width = view.CanvasSize.Width;
        float height = view.CanvasSize.Height;

        int thickness = 5;

        float itemHeight = (9 * ((int)height / 9)) / 9;

        var top = ((itemHeight - 1) * 3) - 3;

        SKRect rect = new SKRect(
            0,
            top, 
            width,
            top + thickness);

        e.Surface.Canvas.DrawRect(rect, paint);

        rect.Top = ((itemHeight - 1) * 6) - 3;
        rect.Bottom = rect.Top + thickness;

        e.Surface.Canvas.DrawRect(rect, paint);

        var left = (width / 3) - (thickness / 2) + 1;

        rect = new SKRect(
            left,
            0,
            left + thickness,
            height - 18);

        e.Surface.Canvas.DrawRect(rect, paint);

        rect.Left = (2 * (width / 3)) - (thickness / 2) + 1;
        rect.Right = rect.Left + thickness;

        e.Surface.Canvas.DrawRect(rect, paint);
    }
}
