using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GridGames.ResX;

namespace GridGames.ViewModels
{
    public class SquaresViewModel : BaseViewModel
    {
        public int MoveCount { get; set; }

        private ObservableCollection<Square> squareList;
        public ObservableCollection<Square> SquareListCollection
        {
            get { return squareList; }
            set { this.squareList = value; }
        }

        private int numberHeight;
        public int NumberHeight
        {
            get { return numberHeight; }
            set
            {
                SetProperty(ref numberHeight, value);
                OnPropertyChanged("numberHeightAdjust");
            }
        }

        private bool showNumbers;
        public bool ShowNumbers
        {
            get { return showNumbers; }
            set
            {
                SetProperty(ref showNumbers, value);
            }
        }

        private bool showPicture;
        public bool ShowPicture
        {
            get { return showPicture; }
            set
            {
                SetProperty(ref showPicture, value);
            }
        }

        private string picturePathSquares;
        public string PicturePathSquares
        {
            get { return picturePathSquares; }
            set
            {
                SetProperty(ref picturePathSquares, value);
            }
        }

        private string pictureName;
        public string PictureName
        {
            get { return pictureName; }
            set
            {
                SetProperty(ref pictureName, value);
            }
        }

        private bool firstRunSquares = true;
        public bool FirstRunSquares
        {
            get
            {
                return firstRunSquares;
            }
            set
            {
                if (firstRunSquares != value)
                {
                    SetProperty(ref firstRunSquares, value);

                    Preferences.Set("FirstRunSquares", firstRunSquares);
                }
            }
        }

        public SquaresViewModel()
        {
            // Show the "Please wait" message until we know it's not needed.
            GameIsLoading = true;

            Title = AppResources.ResourceManager.GetString("Squares");

            squareList = new ObservableCollection<Square>();

            this.CreateDefaultSquares();

            // If we won't be loading pictures into the squares, shuffle them now.
            ShowPicture = Preferences.Get("ShowPicture", false);
            PicturePathSquares = Preferences.Get("PicturePathSquares", "");
            PictureName = Preferences.Get("PictureName", "");

            if (!ShowPicture || !IsImageFilePathValid(PicturePathSquares))
            {
                ResetGrid();
            }
        }

        public bool IsImageFilePathValid(string imageFilePath)
        {
            bool ImageFilePathIsValid = false;

            var fileExists = File.Exists(imageFilePath);
            if (fileExists)
            {
                // The ImageEditor documentation states that only png, jpg and bmp formats
                // are supported, so check the extension suggests that the file is supported.
                var extension = Path.GetExtension(imageFilePath).ToLower();
                if ((extension == ".jpg") || (extension == ".jpeg") ||
                    (extension == ".png") || (extension == ".bmp"))
                {
                    ImageFilePathIsValid = true;
                }
            }

            return ImageFilePathIsValid;
        }

        public void RestoreEmptyGrid()
        {
            squareList.Clear();

            this.CreateDefaultSquares();
        }

        public bool AttemptMoveBySelection(object currentSelection, 
                                           out bool squareSwapped, 
                                           out int itemIndex,
                                           out int emptySquareIndex)
        {
            squareSwapped = false;

            itemIndex = -1;
            emptySquareIndex = -1;

            if (currentSelection == null)
            {
                return false;
            }

            Square selectedSquare = currentSelection as Square;

            int currentSelectionIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (squareList[i] == selectedSquare)
                {
                    currentSelectionIndex = i;
                    break;
                }
            }

            if (currentSelectionIndex < 0)
            {
                return false;
            }

            itemIndex = currentSelectionIndex;

            return AttemptToMoveSquare(currentSelectionIndex, out squareSwapped, out emptySquareIndex);
        }

        public bool AttemptToMoveSquare(int SquareIndex, out bool squareSwapped, out int emptySquareIndex)
        {
            squareSwapped = false;
            emptySquareIndex = -1;

            bool gameIsWon = false;

            Square adjacentSquare;
            string direction = "";

            var resManager = AppResources.ResourceManager;

            // Is the empty square adjacent to this square?

            // Check the square to the left.
            if (SquareIndex % 4 > 0)
            {
                adjacentSquare = squareList[SquareIndex - 1];
                if (adjacentSquare.TargetIndex == 15)
                {
                    emptySquareIndex = SquareIndex - 1;

                    direction = resManager.GetString("Left");
                }
            }

            // Check the square above.
            if ((emptySquareIndex == -1) && (SquareIndex > 3))
            {
                adjacentSquare = squareList[SquareIndex - 4];
                if (adjacentSquare.TargetIndex == 15)
                {
                    emptySquareIndex = SquareIndex - 4;

                    direction = resManager.GetString("Up");
                }
            }

            // Check the square to the right.
            if ((emptySquareIndex == -1) && (SquareIndex % 4 < 3))
            {
                adjacentSquare = squareList[SquareIndex + 1];
                if (adjacentSquare.TargetIndex == 15)
                {
                    emptySquareIndex = SquareIndex + 1;

                    direction = resManager.GetString("Right");
                }
            }

            // Check the square below.
            if ((emptySquareIndex == -1) && (SquareIndex < 12))
            {
                adjacentSquare = squareList[SquareIndex + 4];
                if (adjacentSquare.TargetIndex == 15)
                {
                    emptySquareIndex = SquareIndex + 4;

                    direction = resManager.GetString("Down");
                }
            }

            // Make an announcement regardless of whether a square is moved. But don't
            // make an announcement if the game is won in case that might impact the 
            // announcement of the Congratulations dialog.
            if (!gameIsWon)
            {
                // If we found an adjacent empty square, swap the clicked square with the empty square.
                if ((emptySquareIndex != -1) && (emptySquareIndex != SquareIndex))
                {
                    ++MoveCount;

                    var clickedSquareName = squareList[SquareIndex].AccessibleName;

                    // Now swap the item.
                    (squareList[emptySquareIndex], squareList[SquareIndex]) =
                        (squareList[SquareIndex], squareList[emptySquareIndex]);

                    squareSwapped = true;

                    // Has the game been won?
                    gameIsWon = GameIsWon(squareList);
                    if (!gameIsWon)
                    {
                        string announcement = resManager.GetString("Moved") +
                            " " + clickedSquareName + " " + direction + ".";
                        RaiseNotificationEvent(announcement);
                    }
                }
                else
                {
                    string announcement = resManager.GetString("NoMovePossible");
                    RaiseNotificationEvent(announcement);
                }
            }

            return gameIsWon;
        }

        // Reset the grid to an initial game state.
        public void ResetGrid()
        {
            MoveCount = 0;

            Shuffle(squareList);
        }

        private void CreateDefaultSquares()
        {
            var resManager = AppResources.ResourceManager;

            // Future: If feedback suggests that exposing accessible HelpText for each
            // item would be helpful when playing the game, set that here through the
            // AccessibleDescription property.

            for (int cardIndex = 0; cardIndex < 15; ++cardIndex)
            {
                var resourceIndex = cardIndex + 1;

                squareList.Add(
                    new Square
                    {
                        TargetIndex = cardIndex,
                        VisualLabel = resManager.GetString("DefaultSquare" + resourceIndex + "Name"),
                        AccessibleName = resManager.GetString("DefaultSquare" + resourceIndex + "Name")
                    });
            }

            squareList.Add(
                new Square
                {
                    TargetIndex = 15,
                    VisualLabel = "",
                    AccessibleName = resManager.GetString("DefaultSquareEmpty"),
                });
        }

        private bool GameIsWon(ObservableCollection<Square> collection)
        {
            bool gameIsWon = true;

            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].TargetIndex != i)
                {
                    gameIsWon = false;

                    break;
                }
            }

            return gameIsWon;
        }

        public class Square : INotifyPropertyChanged
        {
            private string accessibleName;
            public string AccessibleName
            {
                get { return accessibleName; }
                set
                {
                    SetProperty(ref accessibleName, value);
                }
            }

            private string accessibleDescription;
            public string AccessibleDescription
            {
                get { return accessibleDescription; }
                set
                {
                    SetProperty(ref accessibleDescription, value);
                }
            }

            private string visualLabel;
            public string VisualLabel
            {
                get { return visualLabel; }
                set
                {
                    SetProperty(ref visualLabel, value);
                }
            }

            public int targetIndex;
            public int TargetIndex
            {
                get { return targetIndex; }
                set
                {
                    SetProperty(ref targetIndex, value);
                }
            }

            private ImageSource pictureImageSource;
            public ImageSource PictureImageSource
            {
                get { return pictureImageSource; }
                set
                {
                    SetProperty(ref pictureImageSource, value);
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

        public void Shuffle(ObservableCollection<Square> collection)
        {
            var shuffler = new Shuffler();

            // Keep shuffling until the arrangement of squares can be solved.
            bool gameCanBeSolved;

            do
            {
                shuffler.Shuffle(collection);

                gameCanBeSolved = CanGameBeSolved(collection);
            }
            while (!gameCanBeSolved);
        }

        private bool CanGameBeSolved(ObservableCollection<Square> collection)
        {
            int parity = 0;
            int emptySquareRow = 0;

            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].TargetIndex == 15)
                {
                    // The empty square row is one-based.
                    emptySquareRow = (i / 4) + 1;

                    continue;
                }

                for (int j = i + 1; j < collection.Count; j++)
                {
                    if ((collection[j].TargetIndex != 15) &&
                        (collection[i].TargetIndex > collection[j].TargetIndex))
                    {
                        parity++;
                    }
                }
            }

            // The following only applies to an grid with an even number of rows and columns.
            bool gridCanBeSolved;

            if (emptySquareRow % 2 == 0)
            {
                gridCanBeSolved = (parity % 2 == 0);
            }
            else
            {
                gridCanBeSolved = (parity % 2 != 0);
            }

            return gridCanBeSolved;
        }

        public class Shuffler
        {
            private readonly Random random;

            public Shuffler()
            {
                this.random = new Random();
            }

            public void Shuffle(ObservableCollection<Square> collection)
            {
                for (int i = collection.Count; i > 1;)
                {
                    int j = this.random.Next(i);

                    --i;

                    (collection[j], collection[i]) = (collection[i], collection[j]);
                }
            }
        }
    }
}
