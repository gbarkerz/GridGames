using GridGames.ViewModels;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SudokuSettingsPage : ContentPage
    {
        public SudokuSettingsPage(SudokuSettingsViewModel sudokuSettingsVM)
        {
            InitializeComponent();
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
   }
}
