using GridGames.ResX;

namespace GridGames.ViewModels
{
    // View model for the Sudoku Settings page.
    public class SudokuSettingsViewModel : BaseViewModel
    {
        public SudokuSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("SudokuSettings");
        }
    }
}
