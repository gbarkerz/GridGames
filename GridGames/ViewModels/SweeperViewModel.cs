using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using GridGames.ResX;
using GridGames.Views;
using System.Diagnostics;

namespace GridGames.ViewModels
{
    public class SweeperViewModel : BaseViewModel
    {
        public int MoveCount { get; set; }

        private ObservableCollection<Square> sweeperList;
        public ObservableCollection<Square> SweeperListCollection
        {
            get { return sweeperList; }
            set { this.sweeperList = value; }
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

        private bool firstRunSweeper = true;
        public bool FirstRunSweeper
        {
            get
            {
                return firstRunSweeper;
            }
            set
            {
                if (firstRunSweeper != value)
                {
                    SetProperty(ref firstRunSweeper, value);

                    Preferences.Set("FirstRunSweeper", firstRunSweeper);
                }
            }
        }

        public SweeperViewModel()
        {
            // Show the "Please wait" message until we know it's not needed.
            GameIsLoading = true;

            Title = AppResources.ResourceManager.GetString("Squares");

            sweeperList = new ObservableCollection<Square>();

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
            sweeperList.Clear();

            this.CreateDefaultSquares();
        }

        public bool AttemptMoveBySelection(object currentSelection)
        {
            if (currentSelection == null)
            {
                return false;
            }

            Square selectedSquare = currentSelection as Square;

            int currentSelectionIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (sweeperList[i] == selectedSquare)
                {
                    currentSelectionIndex = i;
                    break;
                }
            }

            if (currentSelectionIndex < 0)
            {
                return false;
            }

            return ActOnInputOnSquare(currentSelectionIndex);
        }

        public bool ActOnInputOnSquare(int SquareIndex)
        {
            bool gameIsOver = false;

            var square = sweeperList[SquareIndex];
            if (square.HasLeaf)
            {
                gameIsOver = true;
            }
            else
            {
                //square.TurnedUp = true;

                TurnUpNearbyCards(SquareIndex);
            }

            return gameIsOver;
        }

        private void TurnUpNearbyCards(int SquareIndex)
        {
            var square = sweeperList[SquareIndex];

            if (square.HasLeaf)
            {
                return;
            }

            if (square.NearbyLeafCount > 0)
            {
                square.TurnedUp = true;

                square.AccessibleName = square.NearbyLeafCount + " nearby " + 
                    (square.NearbyLeafCount == 1 ? "leaf" : "leaves");
            }

            if (square.TurnedUp)
            {
                return;
            }

            square.TurnedUp = true;

            if (square.NearbyLeafCount == 0)
            {
                square.AccessibleName = "No nearby leaves";
            }

            bool leftEdgeSquare = (SquareIndex % 4 == 0);
            bool rightEdgeSquare = (SquareIndex % 4 == 3);

            if (SquareIndex > 3)
            {
                if (!leftEdgeSquare)
                {
                    TurnUpNearbyCards(SquareIndex - 5);
                }

                TurnUpNearbyCards(SquareIndex - 4);

                if (!rightEdgeSquare)
                {
                    TurnUpNearbyCards(SquareIndex - 3);
                }
            }

            if (SquareIndex % 4 > 0)
            {
                TurnUpNearbyCards(SquareIndex - 1);
            }

            if (SquareIndex % 4 < 3)
            {
                TurnUpNearbyCards(SquareIndex + 1);
            }

            if (SquareIndex < 12)
            {
                if (!leftEdgeSquare)
                {
                    TurnUpNearbyCards(SquareIndex + 3);
                }

                TurnUpNearbyCards(SquareIndex + 4);

                if (!rightEdgeSquare)
                {
                    TurnUpNearbyCards(SquareIndex + 5);
                }
            }
        }

        private void SetSquareNearbyLeafCount(int SquareIndex)
        {
            int count = 0;

            bool leftEdgeSquare = (SquareIndex % 4 == 0);
            bool rightEdgeSquare = (SquareIndex % 4 == 3);

            if (SquareIndex > 3)
            {
                if (!leftEdgeSquare)
                {
                    if (sweeperList[SquareIndex - 5].HasLeaf)
                    {
                        ++count;
                    }
                }

                if (sweeperList[SquareIndex - 4].HasLeaf)
                {
                    ++count;
                }

                if (!rightEdgeSquare)
                {
                    if (sweeperList[SquareIndex - 3].HasLeaf)
                    {
                        ++count;
                    }
                }
            }

            if (SquareIndex % 4 > 0)
            {
                if (sweeperList[SquareIndex - 1].HasLeaf)
                {
                    ++count;
                }
            }

            if (SquareIndex % 4 < 3)
            {
                if (sweeperList[SquareIndex + 1].HasLeaf)
                {
                    ++count;
                }
            }

            if (SquareIndex < 12)
            {
                if (!leftEdgeSquare)
                {
                    if (sweeperList[SquareIndex + 3].HasLeaf)
                    {
                        ++count;
                    }
                }

                if (sweeperList[SquareIndex + 4].HasLeaf)
                {
                    ++count;
                }

                if (!rightEdgeSquare)
                {
                    if (sweeperList[SquareIndex + 5].HasLeaf)
                    {
                        ++count;
                    }
                }
            }

            sweeperList[SquareIndex].NearbyLeafCount = count;
        }

        // Reset the grid to an initial game state.
        public void ResetGrid()
        {
            MoveCount = 0;

            //GBTEST Shuffle(sweeperList);
        }

        private void CreateDefaultSquares()
        {
            var resManager = AppResources.ResourceManager;

            // Future: If feedback suggests that exposing accessible HelpText for each
            // item would be helpful when playing the game, set that here through the
            // AccessibleDescription property.

            for (int cardIndex = 0; cardIndex < 16; ++cardIndex)
            {
                var resourceIndex = cardIndex + 1;

                sweeperList.Add(
                    new Square
                    {
                        TargetIndex = cardIndex,
                        AccessibleName = resManager.GetString("Leaf") + " " + resourceIndex, 
                    });
            }

            var random = new Random();
            int leafCount = 0;
            do
            {
                int r = random.Next(15);
                if (!sweeperList[r].HasLeaf)
                {
                    sweeperList[r].HasLeaf = true;
                    ++leafCount;
                }
            }
            while (leafCount < 2);

            for (int cardIndex = 0; cardIndex < 16; ++cardIndex)
            {
                if (!sweeperList[cardIndex].HasLeaf)
                {
                    SetSquareNearbyLeafCount(cardIndex);
                }
            }

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
            private int nearbyLeafCount = -1;
            public int NearbyLeafCount
            {
                get { return nearbyLeafCount; }
                set
                {
                    SetProperty(ref nearbyLeafCount, value);
                }
            }

            private bool hasLeaf;
            public bool HasLeaf
            {
                get { return hasLeaf; }
                set
                {
                    SetProperty(ref hasLeaf, value);
                }
            }

            private bool showsFlag = false;
            public bool ShowsFlag
            {
                get { return showsFlag; }
                set
                {
                    SetProperty(ref showsFlag, value);
                }
            }

            private bool turnedUp = false;
            public bool TurnedUp
            {
                get { return turnedUp; }
                set
                {
                    SetProperty(ref turnedUp, value);
                }
            }

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
