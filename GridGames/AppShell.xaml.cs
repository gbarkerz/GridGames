using GridGames.ViewModels;
using GridGames.Views;
using System.Diagnostics;

namespace GridGames;

public partial class AppShell : Shell
{
    public static WCAGPage AppWCAGPage;

    public AppShell()
    {
        InitializeComponent();

        // Instatiating the WCAGPage takes quite a while, so only do that once.
        // It still takes a while for the page to first appear, but after that
        // it's relatively quick.
        var timeBefore = DateTime.Now;
        AppWCAGPage = new WCAGPage();
        Debug.WriteLine("Time to create WCAGPage: " + (DateTime.Now - timeBefore).TotalMilliseconds);
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
