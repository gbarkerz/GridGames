using GridGames.ResX;
using GridGames.ViewModels;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SudokuSettingsPage : ContentPage
    {
        public SudokuSettingsPage(SudokuSettingsViewModel sudokuSettingsVM)
        {
            InitializeComponent();

            for (int i = 1; i <= 16; i++)
            {
                BlankSquareCountPicker.Items.Add((i * 5).ToString());
            }

            this.BindingContext = sudokuSettingsVM;
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
   }
}
