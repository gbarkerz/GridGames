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

        private SweeperSettingsViewModel sweeperSettingsVM;
        public SweeperSettingsViewModel SweeperSettingsVM
        {
            get { return sweeperSettingsVM; }
            set
            {
                SetProperty(ref sweeperSettingsVM, value);
            }
        }

        private int sideLength;
        public int SideLength
        {
            get
            {
                return sideLength;
            }
            set
            {
                if (sideLength != value)
                {
                    SetProperty(ref sideLength, value);

                    Preferences.Set("SideLength", sideLength);
                }
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

        private bool gameWon = false;
        public bool GameWon
        {
            get
            {
                return gameWon;
            }
            set
            {
                if (gameWon != value)
                {
                    SetProperty(ref gameWon, value);

                    OnPropertyChanged("GameOver");
                }
            }
        }

        private bool gameLost = false;
        public bool GameLost
        {
            get
            {
                return gameLost;
            }
            set
            {
                if (gameLost != value)
                {
                    SetProperty(ref gameLost, value);

                    OnPropertyChanged("GameOver");
                }
            }
        }

        public bool GameOver
        {
            get
            {
                return (gameWon || gameLost);
            }
        }

        public SweeperViewModel()
        {
            // Show the "Please wait" message until we know it's not needed.
            GameIsLoading = true;

            Title = AppResources.ResourceManager.GetString("Sweeper");

            sweeperSettingsVM = new SweeperSettingsViewModel();

            this.SideLength = sweeperSettingsVM.SideLength;

            sweeperList = new ObservableCollection<Square>();

            this.CreateGrid();
        }

        private void CreateGrid()
        {
            // Future: If feedback suggests that exposing accessible HelpText for each
            // item would be helpful when playing the game, set that here through the
            // AccessibleDescription property.

            for (int cardIndex = 0; cardIndex < (sweeperSettingsVM.SideLength * sweeperSettingsVM.SideLength); ++cardIndex)
            {
                sweeperList.Add(
                    new Square
                    {
                        TargetIndex = cardIndex
                    });
            }
        }

        public void InitialiseGrid(int indexNoFrog)
        {
            for (int i = 0; i < (sweeperSettingsVM.SideLength * sweeperSettingsVM.SideLength); ++i)
            {
                sweeperList[i].TurnedUp = false;
                sweeperList[i].HasFrog = false;
                sweeperList[i].ShowsQueryFrog = false;
                sweeperList[i].NearbyFrogCount = 0;
            }

            var random = new Random();

            int frogsFound = 0;

            do
            {
                int r = random.Next((sweeperSettingsVM.SideLength * sweeperSettingsVM.SideLength) - 1);

                if (!sweeperList[r].HasFrog && (sweeperList[r].targetIndex != indexNoFrog))
                {
                    sweeperList[r].HasFrog = true;

                    ++frogsFound;
                }
            }
            while (frogsFound < sweeperSettingsVM.FrogCount);

            int frogCount = 0;

            for (int cardIndex = 0; cardIndex < (sweeperSettingsVM.SideLength * sweeperSettingsVM.SideLength); ++cardIndex)
            {
                if (!sweeperList[cardIndex].HasFrog)
                {
                    SetSquareNearbyFrogCount(cardIndex);
                }
                else
                {
                    // Debug purposes:
                    ++frogCount;
                }
            }

            // Debug purposes:
            if (frogCount != sweeperSettingsVM.FrogCount)
            {
                System.Diagnostics.Debug.Write("Unexpected initial frog count: Actual " +
                    frogCount + ", Expected " + sweeperSettingsVM.FrogCount);
            }
        }

        private void SetSquareNearbyFrogCount(int SquareIndex)
        {
            int count = 0;

            bool leftEdgeSquare = (SquareIndex % sweeperSettingsVM.SideLength == 0);
            bool rightEdgeSquare = (SquareIndex % sweeperSettingsVM.SideLength == (sweeperSettingsVM.SideLength - 1));

            if (SquareIndex > (sweeperSettingsVM.SideLength - 1))
            {
                if (!leftEdgeSquare)
                {
                    if (sweeperList[SquareIndex - (sweeperSettingsVM.SideLength + 1)].HasFrog)
                    {
                        ++count;
                    }
                }

                if (sweeperList[SquareIndex - sweeperSettingsVM.SideLength].HasFrog)
                {
                    ++count;
                }

                if (!rightEdgeSquare)
                {
                    if (sweeperList[SquareIndex - (sweeperSettingsVM.SideLength - 1)].HasFrog)
                    {
                        ++count;
                    }
                }
            }

            if (SquareIndex % sweeperSettingsVM.SideLength > 0)
            {
                if (sweeperList[SquareIndex - 1].HasFrog)
                {
                    ++count;
                }
            }

            if (SquareIndex % sweeperSettingsVM.SideLength < (sweeperSettingsVM.SideLength - 1))
            {
                if (sweeperList[SquareIndex + 1].HasFrog)
                {
                    ++count;
                }
            }

            if (SquareIndex < ((sweeperSettingsVM.SideLength * sweeperSettingsVM.SideLength) - sweeperSettingsVM.SideLength))
            {
                if (!leftEdgeSquare)
                {
                    if (sweeperList[SquareIndex + (sweeperSettingsVM.SideLength - 1)].HasFrog)
                    {
                        ++count;
                    }
                }

                if (sweeperList[SquareIndex + sweeperSettingsVM.SideLength].HasFrog)
                {
                    ++count;
                }

                if (!rightEdgeSquare)
                {
                    if (sweeperList[SquareIndex + (sweeperSettingsVM.SideLength + 1)].HasFrog)
                    {
                        ++count;
                    }
                }
            }

            sweeperList[SquareIndex].NearbyFrogCount = count;
        }

        public void ResetGrid()
        {
            sweeperList.Clear();

            CreateGrid();    

            for (int i = 0; i < (sweeperSettingsVM.SideLength * sweeperSettingsVM.SideLength); ++i)
            {
                sweeperList[i].TurnedUp = false;
                sweeperList[i].HasFrog = false;
                sweeperList[i].ShowsQueryFrog = false;
                sweeperList[i].NearbyFrogCount = 0;
            }
        }

        public bool ActOnInputOnSquare(int SquareIndex)
        {
            bool gameIsLost = false;

            var square = sweeperList[SquareIndex];
            if (square.HasFrog)
            {
                gameIsLost = true;
            }
            else
            {
                TurnUpNearbyCards(SquareIndex);
            }

            return gameIsLost;
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

            bool leftEdgeSquare = (SquareIndex % sweeperSettingsVM.SideLength == 0);
            bool rightEdgeSquare = (SquareIndex % sweeperSettingsVM.SideLength == (sweeperSettingsVM.SideLength - 1));

            if (SquareIndex > (sweeperSettingsVM.SideLength - 1))
            {
                if (!leftEdgeSquare)
                {
                    TurnUpNearbyCards(SquareIndex - (sweeperSettingsVM.SideLength + 1));
                }

                TurnUpNearbyCards(SquareIndex - sweeperSettingsVM.SideLength);

                if (!rightEdgeSquare)
                {
                    TurnUpNearbyCards(SquareIndex - (sweeperSettingsVM.SideLength - 1));
                }
            }

            if (SquareIndex % sweeperSettingsVM.SideLength > 0)
            {
                TurnUpNearbyCards(SquareIndex - 1);
            }

            if (SquareIndex % sweeperSettingsVM.SideLength < (sweeperSettingsVM.SideLength - 1))
            {
                TurnUpNearbyCards(SquareIndex + 1);
            }

            if (SquareIndex < ((sweeperSettingsVM.SideLength * sweeperSettingsVM.SideLength) - sweeperSettingsVM.SideLength))
            {
                if (!leftEdgeSquare)
                {
                    TurnUpNearbyCards(SquareIndex + (sweeperSettingsVM.SideLength - 1));
                }

                TurnUpNearbyCards(SquareIndex + sweeperSettingsVM.SideLength);

                if (!rightEdgeSquare)
                {
                    TurnUpNearbyCards(SquareIndex + (sweeperSettingsVM.SideLength + 1));
                }
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
                        accessibleName = "Leaf";
                    }

                    // Important: At the time of writing this, .NET MAUI does not expose CollectionView
                    // items supporting the UIA GridItem pattern. As such, include an index in the
                    // accessible name. Once the GridItem pattern is supported, and screen readers 
                    // can announce the item's row and column, remove the use of an index here.
                    accessibleName += " " + 
                        (accessibleName.Contains("nearby") ? "for" : "on") +
                        " stone " + (TargetIndex + 1).ToString();

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
