using GridGames.ResX;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GridGames.ViewModels
{
    // View model for the Sudoku Settings page.
    public class SudokuSettingsViewModel : BaseViewModel
    {
        public SudokuSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("SudokuSettings");

            BlankSquareCount = (int)Preferences.Get("BlankSquareCount", 10);
        }

        private int blankSquareCount;
        public int BlankSquareCount
        {
            get
            {
                return blankSquareCount;
            }
            set
            {
                if (blankSquareCount != value)
                {
                    SetProperty(ref blankSquareCount, value);

                    Preferences.Set("BlankSquareCount", (int)value);
                }
            }
        }
    }
}
