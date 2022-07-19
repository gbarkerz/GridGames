﻿using GridGames.ViewModels;
using GridGames.Views;

namespace GridGames;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
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

    //private async void OnAppSettingsMenuItemClicked(object sender, EventArgs e)
    //{
    //    Shell.Current.FlyoutIsPresented = false;

    //    bool showAppSettingWindow = false;

    //    var currentPage = this.CurrentPage;
    //    if (currentPage is MatchingPage)
    //    {
    //        var vm = (CurrentPage as MatchingPage).BindingContext as MatchingViewModel;
    //        showAppSettingWindow = !vm.FirstRunMatching;
    //    }
    //    else if (currentPage is WheresPage)
    //    {
    //        var vm = (CurrentPage as WheresPage).BindingContext as WheresViewModel;
    //        showAppSettingWindow = !vm.FirstRunWheres;
    //    }

    //    if (showAppSettingWindow)
    //    {
    //        var appSettingsPage = new AppSettingsPage();
    //        await Navigation.PushModalAsync(appSettingsPage);
    //    }
    //}

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
