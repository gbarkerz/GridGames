using GridGames.ViewModels;
using GridGames.Views;

namespace GridGames;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

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
