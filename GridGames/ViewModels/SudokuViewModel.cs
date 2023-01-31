using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GridGames.ResX;

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

        public SudokuViewModel()
        {
            Title = AppResources.ResourceManager.GetString("Sudoku");

            sudokuSettingsVM = new SudokuSettingsViewModel();

            sudokuList = new ObservableCollection<Square>();

            this.CreateGrid();
        }

        private void CreateGrid()
        {
            for (int cardIndex = 0; cardIndex < (gridDimensions * gridDimensions); ++cardIndex)
            {
                sudokuList.Add(
                    new Square
                    {
                        index = cardIndex,
                        answer = (cardIndex % gridDimensions) + 1
                    });
            }
        }

        public class Square : INotifyPropertyChanged
        {
            // Support the ability to customise the UIA Name property of the square.
            public string AccessibleName
            {
                get
                {                 
                    return (index + 1).ToString(); 
                }
            }

            // Support the ability to customise the UIA HelpText property of the square.
            private string accessibleDescription;
            public string AccessibleDescription
            {
                get { return accessibleDescription; }
                set
                {
                    SetProperty(ref accessibleDescription, value);
                }
            }

            public int index;
            public int Index
            {
                get { return index; }
                set
                {
                    SetProperty(ref index, value);
                }
            }

            public int answer;
            public int Answer
            {
                get { return answer; }
                set
                {
                    SetProperty(ref answer, value);
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
