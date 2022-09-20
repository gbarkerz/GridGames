﻿using GridGames.ViewModels;
using GridGames.Views;

namespace GridGames;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // By default, we show the Where's WCAG? game.
        var initialGame = Preferences.Get("InitialGame", "Wheres");

        // Assume we know the order of the items in the app flyout.
        if (this.Items.Count > 0)
        {
            if (initialGame == "Pairs")
            {
                this.CurrentItem = this.Items[1];
            }
            else if (initialGame == "Squares")
            {
                this.CurrentItem = this.Items[2];
            }
        }

        this.Loaded += AppShell_Loaded;
    }

    public static WCAGPage AppWCAGPage;

    private void AppShell_Loaded(object sender, EventArgs e)
    {
        AppWCAGPage = new WCAGPage();
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
        else if (currentPage is WheresPage)
        {
            var vm = (CurrentPage as WheresPage).BindingContext as WheresViewModel;
            if (!vm.FirstRunWheres)
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
                vm.ResetGrid(true);
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
        else if (currentPage is WheresPage)
        {
            var vm = (CurrentPage as WheresPage).BindingContext as WheresViewModel;
            if (!vm.FirstRunWheres)
            {
                vm.ResetGrid(true);
            }
        }
    }
}
