using GridGames.ViewModels;
using GridGames.Views;

namespace GridGames;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // By default, we show the Where's WCAG? game.
        var initialGame = Preferences.Get("InitialGame", "Sudoku");

        // Assume we know the order of the items in the app flyout.
        if (this.Items.Count > 0)
        {
            if (initialGame == "Sudoku")
            {
                this.CurrentItem = this.Items[0];
            }
            else if (initialGame == "Sweeper")
            {
                this.CurrentItem = this.Items[1];
            }
            else if (initialGame == "Pairs")
            {
                this.CurrentItem = this.Items[2];
            }
            else if (initialGame == "Squares")
            {
                this.CurrentItem = this.Items[3];
            }
        }

        this.Loaded += AppShell_Loaded;
    }

    private void AppShell_Loaded(object sender, EventArgs e)
    {
#if !WINDOWS
        // Currently keyboard support is only available on Windows.
        HelpMenuItem.Text = HelpMenuItem.Text.Replace(" (F1)", "");
        RestartGameMenuItem.Text = RestartGameMenuItem.Text.Replace(" (F5)", "");
#else
        // Set minimum dimensions on the window on Windows.
        Window.MinimumWidth = 600;
        Window.MinimumHeight = 600;
#endif
    }

    private async void OnHelpMenuItemClicked(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = false;

        var currentPage = this.CurrentPage;
        if (currentPage is MatchingPage)
        {
            var vm = (CurrentPage as MatchingPage).BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                await Navigation.PushModalAsync(new HelpPage(currentPage));
            }
        }
        else if (currentPage is SquaresPage)
        {
            var vm = (CurrentPage as SquaresPage).BindingContext as SquaresViewModel;
            if (!vm.FirstRunSquares)
            {
                await Navigation.PushModalAsync(new HelpPage(currentPage));
            }
        }
        else if (currentPage is SweeperPage)
        {
            var vm = (CurrentPage as SweeperPage).BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                await Navigation.PushModalAsync(new HelpPage(currentPage));
            }
        }
        else if (currentPage is SudokuPage)
        {
            var vm = (CurrentPage as SudokuPage).BindingContext as SudokuViewModel;
            if (!vm.FirstRunSudoku)
            {
                await Navigation.PushModalAsync(new HelpPage(currentPage));
            }
        }
    }

    private void OnRestartMenuItemClicked(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = false;

        var currentPage = this.CurrentPage;
        if (currentPage is MatchingPage)
        {
            var vm = (CurrentPage as MatchingPage).BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                var pairsPage = (CurrentPage as MatchingPage);

                pairsPage.RestartGame();
            }
        }
        else if (currentPage is SquaresPage)
        {
            var vm = (CurrentPage as SquaresPage).BindingContext as SquaresViewModel;
            if (!vm.FirstRunSquares)
            {
                var squaresPage = (CurrentPage as SquaresPage);

                squaresPage.RestartGame();
            }
        }
        else if (currentPage is SweeperPage)
        {
            var vm = (CurrentPage as SweeperPage).BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                var sweeperPage = (CurrentPage as SweeperPage);

                sweeperPage.RestartGame(false);
            }
        }
        else if (currentPage is SudokuPage)
        {
            var vm = (CurrentPage as SudokuPage).BindingContext as SudokuViewModel;
            if (!vm.FirstRunSudoku)
            {
                var sudokuPage = (CurrentPage as SudokuPage);

                sudokuPage.RestartGame();
            }
        }
    }
}
