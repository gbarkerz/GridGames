using GridGames.ResX;
using GridGames.ViewModels;
using System.Diagnostics;

namespace GridGames.Views;

public partial class SudokuPage : ContentPage
{
	public SudokuPage()
	{
		InitializeComponent();
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
            // Todo: Take some action here.
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

    public void RestartGame()
    {
        var vm = this.BindingContext as SudokuViewModel;
        if (!vm.FirstRunSudoku)
        {
            // Todo: Restart the Sudoku game here.
        }
    }

    private void WelcomeMessageCloseButton_Clicked(object sender, EventArgs e)
    {
        var vm = this.BindingContext as SudokuViewModel;
        vm.FirstRunSudoku = false;

        var readyMessage = AppResources.ResourceManager.GetString("SudokuReadyToPlay");
        vm.RaiseNotificationEvent(readyMessage);
    }
}
