using GridGames.ResX;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GridGames.ViewModels
{
    public enum SudokuNoMoveResponseChoices
    {
        NoResponse,
        PlaySound,
        Announcement,
        PlaySoundAndAnnouncement
    };

    // View model for the Sudoku Settings page.
    public class SudokuSettingsViewModel : BaseViewModel
    {
        public SudokuSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("SudokuSettings");

            BlankSquareCount = (int)Preferences.Get("BlankSquareCount", 10);
            SudokuNoMoveResponse = Preferences.Get("SudokuNoMoveResponse", (int)SudokuNoMoveResponseChoices.Announcement);
            EmptySquareIndicatorIsX = Preferences.Get("EmptySquareIndicatorIsX", false);
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

        private int sudokuNoMoveResponse;
        public int SudokuNoMoveResponse
        {
            get
            {
                return sudokuNoMoveResponse;
            }
            set
            {
                if (sudokuNoMoveResponse != value)
                {
                    SetProperty(ref sudokuNoMoveResponse, value);

                    Preferences.Set("SudokuNoMoveResponse", (int)value);
                }
            }
        }

        private bool emptySquareIndicatorIsX;
        public bool EmptySquareIndicatorIsX
        {
            get
            {
                return emptySquareIndicatorIsX;
            }
            set
            {
                if (emptySquareIndicatorIsX != value)
                {
                    SetProperty(ref emptySquareIndicatorIsX, value);

                    Preferences.Set("EmptySquareIndicatorIsX", (bool)value);
                }
            }
        }
    }
}
