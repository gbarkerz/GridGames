using GridGames.ViewModels;

namespace GridGames.Views;

public partial class PageTitleArea : ContentView
{
	public PageTitleArea()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageTitleArea));

    public string Title
    {
        get => (string)GetValue(PageTitleArea.TitleProperty);
        set => SetValue(PageTitleArea.TitleProperty, value);
    }

    private async void SettingsButton_Clicked(object sender, EventArgs e)
    {
        if (this.BindingContext is SudokuViewModel)
        {
            var vm = this.BindingContext as SudokuViewModel;
            if (!vm.FirstRunSudoku)
            {
                var settingsPage = new SudokuSettingsPage(vm.SudokuSettingsVM);
                await Navigation.PushModalAsync(settingsPage);
            }
        }
        else if (this.BindingContext is MatchingViewModel)
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                var settingsPage = new MatchingGameSettingsPage();
                await Navigation.PushModalAsync(settingsPage);
            }
        }
        if (this.BindingContext is SweeperViewModel)
        {
            var vm = this.BindingContext as SweeperViewModel;
            if (!vm.FirstRunSweeper)
            {
                var settingsPage = new SweeperGameSettingsPage(vm.SweeperSettingsVM);
                await Navigation.PushModalAsync(settingsPage);
            }
        }
        if (this.BindingContext is SquaresViewModel)
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (!vm.FirstRunSquares)
            {
                var settingsPage = new SquaresSettingsPage();
                await Navigation.PushModalAsync(settingsPage);
            }
        }
    }
}