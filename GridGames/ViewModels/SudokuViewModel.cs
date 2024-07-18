using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Security;
using System.Runtime.CompilerServices;
using GridGames.ResX;

using Sudoku;

namespace GridGames.ViewModels
{
    // View model for the Sudoku page.
    public class SudokuViewModel : BaseViewModel
    {
        // Barker Todo: Remove the use of these statics.
        static public bool SudokuEmptySquareIndicatorIsX;
        static public string SudokuSquareLocationAnnouncementFormat;

        private const int gridDimensions = 9;

        private ObservableCollection<Square> sudokuList;
        public ObservableCollection<Square> SudokuListCollection
        {
            get { return sudokuList; }
            set { this.sudokuList = value; }
        }

        private SudokuSettingsViewModel sudokuSettingsVM;
        public SudokuSettingsViewModel SudokuSettingsVM
        {
            get { return sudokuSettingsVM; }
            set
            {
                SetProperty(ref sudokuSettingsVM, value);
            }
        }

        private bool firstRunSudoku = true;
        public bool FirstRunSudoku
        {
            get
            {
                return firstRunSudoku;
            }
            set
            {
                if (firstRunSudoku != value)
                {
                    SetProperty(ref firstRunSudoku, value);

                    Preferences.Set("FirstRunSudoku", firstRunSudoku);
                }
            }
        }

        private int currentBlankSquareCount;
        public int CurrentBlankSquareCount
        {
            get
            {
                return currentBlankSquareCount;
            }
            set
            {
                if (currentBlankSquareCount != value)
                {
                    SetProperty(ref currentBlankSquareCount, value);
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

                    SudokuEmptySquareIndicatorIsX = value;
                }
            }
        }


        private string squareLocationAnnouncementFormat;
        public string SquareLocationAnnouncementFormat
        {
            get
            {
                return squareLocationAnnouncementFormat;
            }
            set
            {
                if (squareLocationAnnouncementFormat != value)
                {
                    SetProperty(ref squareLocationAnnouncementFormat, value);

                    SudokuSquareLocationAnnouncementFormat = value;
                }
            }
        }

        private SudokuBoard GameBoard = new SudokuBoard();

        public SudokuViewModel()
        {
            Title = AppResources.ResourceManager.GetString("Sudoku");

            sudokuSettingsVM = new SudokuSettingsViewModel();

            sudokuList = new ObservableCollection<Square>();

            RestartGame();
        }

        private void RestartGame()
        {
            GameBoard.Clear();
            GameBoard.Solver.SolveThePuzzle(UseRandomGenerator: true);

            this.CreateGrid();
        }

        private void CreateGrid()
        {
            sudokuList.Clear();

            int countSquares = gridDimensions * gridDimensions;

            CurrentBlankSquareCount = sudokuSettingsVM.BlankSquareCount;

            var numberVisibleArray = new bool[countSquares];

            var random = new Random();

            int countNumberShownFound = 0;

            while (countNumberShownFound < CurrentBlankSquareCount)
            {
                var randomIndex = random.Next(countSquares);

                if (!numberVisibleArray[randomIndex])
                {
                    numberVisibleArray[randomIndex] = true;
                    ++countNumberShownFound;
                }
            }

            for (int index = 0; index < countSquares; ++index)
            {
                int cellIndex = index;

                string cellValue = GameBoard.GetCell(cellIndex).Value == -1 ? "" : GameBoard.GetCell(cellIndex).Value.ToString();

                var numberShown = !numberVisibleArray[index];

                sudokuList.Add(
                    new Square
                    {
                        Index = index,
                        Number = cellValue,
                        OriginalNumber = cellValue,
                        NumberShown = numberShown,
                        FixedNumber = numberShown
                    });
            };
        }

        public bool IsGridFilled(out bool gameWon)
        {
            bool gridIsFilled = true;
            bool gameWonSoFar = true;

            for (int index = 0; index < (gridDimensions * gridDimensions); ++index)
            {
                if (!sudokuList[index].NumberShown)
                {
                    gridIsFilled = false;

                    break;
                }

                if (gameWonSoFar && (sudokuList[index].Number != GameBoard.GetCell(index).Value.ToString()))
                {
                    gameWonSoFar = false;
                }
            }

            gameWon = gameWonSoFar;

            return gridIsFilled;
        }

        public void ResetGrid()
        {
            RestartGame();

            RaiseNotificationEvent("Sudoku game restarted.");
        }

        public class Square : INotifyPropertyChanged
        {
            // Support the ability to customise the UIA Name property of the square.

            // Important: The evolving requirements of the game have led to a poor implementation around
            // setting the square's accessible name. Particularly involving the use of the static bool
            // SudokuEmptySquareIndicatorIsX. At some point this code will be cleaned up to remove the 
            // use of SudokuEmptySquareIndicatorIsX and the RefreshAccessibleName() method below.

            public string AccessibleName
            {
                get
                {
                    var name = (NumberShown ? 
                                    Number + ", " + (FixedNumber ? "Fixed" : "Guess") :
                                    (SudokuViewModel.SudokuEmptySquareIndicatorIsX ? "x" : "No number shown"));

                    return name;
                }
            }

            public void RefreshAccessibleName()
            {
                OnPropertyChanged("AccessibleName");
            }

            public string AccessibleDescription
            {
                get
                {
                    int rowIndex = (index / 9);
                    int columnIndex = (index % 9);

                    int groupIndex = (3 * (int)(rowIndex / 3)) + (columnIndex / 3);

                    string rowColumnData = "";

#if IOS
                    rowColumnData = ", Row " + (rowIndex + 1) + " Column " + (columnIndex + 1);
#endif

                    string fullDescription;

#if WINDOWS
                    fullDescription = SudokuViewModel.SudokuSquareLocationAnnouncementFormat;

                    fullDescription = fullDescription.Replace("$g", (groupIndex + 1).ToString());
                    fullDescription = fullDescription.Replace("$r", (rowIndex + 1).ToString());
                    fullDescription = fullDescription.Replace("$c", (columnIndex + 1).ToString());
#else
                    fullDescription = "Group " + (groupIndex + 1).ToString() + rowColumnData;
#endif

                    return fullDescription;
                }
            }

            private int index;
            public int Index
            {
                get { return index; }
                set
                {
                    SetProperty(ref index, value);
                }
            }

            private string number;
            public string Number
            {
                get { return number; }
                set
                {
                    SetProperty(ref number, value);

                    OnPropertyChanged("AccessibleName");
                }
            }


            private string originalNumber;
            public string OriginalNumber
            {
                get { return originalNumber; }
                set
                {
                    SetProperty(ref originalNumber, value);
                }
            }

            private bool numberShown;
            public bool NumberShown
            {
                get
                {
                    return numberShown;
                }
                set
                {
                    SetProperty(ref numberShown, value);

                    OnPropertyChanged("AccessibleName");
                }
            }

            private bool fixedNumber;
            public bool FixedNumber
            {
                get
                {
                    return fixedNumber;
                }
                set
                {
                    SetProperty(ref fixedNumber, value);
                }
            }

            protected bool SetProperty<T>(ref T backingStore, T value,
                [CallerMemberName] string propertyName = "",
                Action onChanged = null)
            {
                if (EqualityComparer<T>.Default.Equals(backingStore, value))
                    return false;

                backingStore = value;
                onChanged?.Invoke();
                OnPropertyChanged(propertyName);
                return true;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                var changed = PropertyChanged;
                if (changed == null)
                    return;

                changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
