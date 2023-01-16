﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GridGames.ResX;

namespace GridGames.ViewModels
{
    public class SweeperViewModel : BaseViewModel
    {
        private ObservableCollection<Square> sweeperList;
        public ObservableCollection<Square> SweeperListCollection
        {
            get { return sweeperList; }
            set { this.sweeperList = value; }
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

        private bool gameOver = false;
        public bool GameOver
        {
            get
            {
                return gameOver;
            }
            set
            {
                if (gameOver != value)
                {
                    SetProperty(ref gameOver, value);
                }
            }
        }

        public SweeperViewModel()
        {
            // Show the "Please wait" message until we know it's not needed.
            GameIsLoading = true;

            Title = AppResources.ResourceManager.GetString("Squares");

            sweeperList = new ObservableCollection<Square>();

            this.CreateGrid();
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
            if (square.HasFrog)
            {
                gameIsOver = true;
            }
            else
            {
                TurnUpNearbyCards(SquareIndex);
            }

            return gameIsOver;
        }

        private void TurnUpNearbyCards(int SquareIndex)
        {
            var square = sweeperList[SquareIndex];

            if (square.HasFrog)
            {
                return;
            }

            if (square.NearbyFrogCount > 0)
            {
                square.TurnedUp = true;
            }

            if (square.TurnedUp)
            {
                return;
            }

            square.TurnedUp = true;

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

        private void SetSquareNearbyFrogCount(int SquareIndex)
        {
            int count = 0;

            bool leftEdgeSquare = (SquareIndex % 4 == 0);
            bool rightEdgeSquare = (SquareIndex % 4 == 3);

            if (SquareIndex > 3)
            {
                if (!leftEdgeSquare)
                {
                    if (sweeperList[SquareIndex - 5].HasFrog)
                    {
                        ++count;
                    }
                }

                if (sweeperList[SquareIndex - 4].HasFrog)
                {
                    ++count;
                }

                if (!rightEdgeSquare)
                {
                    if (sweeperList[SquareIndex - 3].HasFrog)
                    {
                        ++count;
                    }
                }
            }

            if (SquareIndex % 4 > 0)
            {
                if (sweeperList[SquareIndex - 1].HasFrog)
                {
                    ++count;
                }
            }

            if (SquareIndex % 4 < 3)
            {
                if (sweeperList[SquareIndex + 1].HasFrog)
                {
                    ++count;
                }
            }

            if (SquareIndex < 12)
            {
                if (!leftEdgeSquare)
                {
                    if (sweeperList[SquareIndex + 3].HasFrog)
                    {
                        ++count;
                    }
                }

                if (sweeperList[SquareIndex + 4].HasFrog)
                {
                    ++count;
                }

                if (!rightEdgeSquare)
                {
                    if (sweeperList[SquareIndex + 5].HasFrog)
                    {
                        ++count;
                    }
                }
            }

            sweeperList[SquareIndex].NearbyFrogCount = count;
        }

        private void CreateGrid()
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
                        TargetIndex = cardIndex
                    });
            }
        }

        public void InitialiseGrid(int indexNoFrog)
        {
            var random = new Random();

            int frogCount = 0;

            do
            {
                int r = random.Next(15);

                if (!sweeperList[r].HasFrog && (sweeperList[r].targetIndex != indexNoFrog))
                {
                    sweeperList[r].HasFrog = true;

                    ++frogCount;
                }
            }
            while (frogCount < 2);

            for (int cardIndex = 0; cardIndex < 16; ++cardIndex)
            {
                if (!sweeperList[cardIndex].HasFrog)
                {
                    SetSquareNearbyFrogCount(cardIndex);
                }
            }
        }

        public void ResetGrid()
        {
            for (int i = 0; i < 16; ++i)
            {
                sweeperList[i].TurnedUp = false;
                sweeperList[i].HasFrog = false;
                sweeperList[i].ShowsQueryFrog = false;
            }
        }

        public class Square : INotifyPropertyChanged
        {
            public string AccessibleName
            {
                get 
                {
                    string accessibleName = "";

                    if (ShowsQueryFrog)
                    {
                        accessibleName = "Query Frog";
                    }
                    else if (TurnedUp)
                    {
                        if (HasFrog)
                        {
                            accessibleName = "Frog";
                        }
                        else if (NearbyFrogCount > 0)
                        {
                            accessibleName = NearbyFrogCount.ToString() + " nearby frog" +
                                (NearbyFrogCount > 1 ? "s" : "");
                        }
                        else
                        {
                            accessibleName = "No nearby frogs";
                        }
                    }
                    else
                    {
                        accessibleName = "Leaf " + (TargetIndex + 1).ToString();
                    }

                    return accessibleName; 
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

            public int targetIndex;
            public int TargetIndex
            {
                get { return targetIndex; }
                set
                {
                    SetProperty(ref targetIndex, value);
                }
            }

            private bool turnedUp = false;
            public bool TurnedUp
            {
                get { return turnedUp; }
                set
                {
                    SetProperty(ref turnedUp, value);

                    OnPropertyChanged("AccessibleName");
                }
            }

            private bool hasFrog;
            public bool HasFrog
            {
                get { return hasFrog; }
                set
                {
                    SetProperty(ref hasFrog, value);
                }
            }

            private int nearbyFrogCount = -1;
            public int NearbyFrogCount
            {
                get { return nearbyFrogCount; }
                set
                {
                    SetProperty(ref nearbyFrogCount, value);

                    OnPropertyChanged("AccessibleName");
                }
            }

            private bool showsQueryFrog = false;
            public bool ShowsQueryFrog
            {
                get { return showsQueryFrog; }
                set
                {
                    SetProperty(ref showsQueryFrog, value);

                    OnPropertyChanged("AccessibleName");
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
