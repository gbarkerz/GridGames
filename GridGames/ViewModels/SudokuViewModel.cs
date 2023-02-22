using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GridGames.ResX;

using Sudoku;

namespace GridGames.ViewModels
{
    // View model for the Sudoku page.
    public class SudokuViewModel : BaseViewModel
    {
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
            public string AccessibleName
            {
                get
                {
                    var name = (NumberShown ? Number + " " + (FixedNumber ? "Fixed" : "Guess") : "No number shown");

                    return name;
                }
            }

            public string AccessibleDescription
            {
                get
                {
                    int rowIndex = (index / 9);
                    int columnIndex = (index % 9);

                    int groupIndex = (3 * (int)(rowIndex / 3)) + (columnIndex / 3);

                    return "Group " + (groupIndex + 1).ToString();
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
