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

namespace GridGames.ViewModels
{
    public class Card : INotifyPropertyChanged
    {
        public int Index { get; set; }

        private string accessibleName;
        public string AccessibleName
        {
            get
            {
                string name = "";
                if (faceUp)
                {
                    name = (Matched ?
                        AppResources.ResourceManager.GetString("Matched") + " " : "") + 
                        accessibleName;
                }
                else
                {
                    name = AppResources.ResourceManager.GetString("FaceDown");
                }

                return name;
            }
            set
            {
                SetProperty(ref accessibleName, value);
            }
        }

        private string accessibleDescription;
        public string AccessibleDescription
        {
            get
            {
                return faceUp ? accessibleDescription : "";
            }
            set
            {
                SetProperty(ref accessibleDescription, value);
            }
        }

        private bool faceUp;
        public bool FaceUp
        {
            get
            {
                return faceUp;
            }
            set
            {
                SetProperty(ref faceUp, value);

                // Other properties may change as a result of this.
                OnPropertyChanged("AccessibleName");
                OnPropertyChanged("AccessibleDescription");
            }
        }

        private bool matched;
        public bool Matched
        {
            get
            {
                return matched;
            }
            set
            {
                SetProperty(ref matched, value);

                // Other properties may change as a result of this.
                OnPropertyChanged("AccessibleName");
            }
        }

        private ImageSource pictureImageSource;
        public ImageSource PictureImageSource
        {
            get
            {
                return pictureImageSource;
            }
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

    public class MatchingViewModel : BaseViewModel
    {
        private Card firstCardInMatchAttempt;
        private Card secondCardInMatchAttempt;

        public MatchingViewModel()
        {
            Title = AppResources.ResourceManager.GetString("Pairs");

            squareList = new ObservableCollection<Card>();

            TryAgainCount = 0;
        }

        public int MoveCount { get; set; }
        public int TryAgainCount { get; set; }

        private bool firstRunMatching = true;
        public bool FirstRunMatching
        {
            get
            {
                return firstRunMatching;
            }
            set
            {
                if (firstRunMatching != value)
                {
                    SetProperty(ref firstRunMatching, value);

                    Preferences.Set("FirstRunMatching", firstRunMatching);
                }
            }
        }

        // Barker: Remove this at some point, and only use the equivalent property in the
        // MatchingSettingsViewModel. When this is done, the bound property in the grid's
        // squares needs to update following a change made at the Matching Settings page.
        private Aspect pictureAspect;
        public Aspect PictureAspect
        {
            get
            {
                return pictureAspect;
            }
            set
            {
                SetProperty(ref pictureAspect, value);
            }
        }

        private double gridRowHeight;
        public double GridRowHeight
        {
            get
            {
                return gridRowHeight;
            }
            set
            {
                SetProperty(ref gridRowHeight, value);
            }
        }

        private ImageSource GetImageSourceForCard(string cardName)
        {
            return ImageSource.FromFile(cardName + ".jpg");
        }

        public void SetupDefaultMatchingCardList()
        {
            var resManager = AppResources.ResourceManager;

            squareList.Clear();

            // We have 8 pairs of cards.
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    // The card index runs from 1 to 16.
                    var cardIndex = (i * 2) + j + 1;
                    var resourceIndex = (i + 1);

                    squareList.Add(
                        new Card
                        {
                            Index = cardIndex,
                            AccessibleName = resManager.GetString("DefaultMatchingCard" + resourceIndex + "Name"),
                            AccessibleDescription = resManager.GetString("DefaultMatchingCard" + resourceIndex + "Description"),
                            PictureImageSource = GetImageSourceForCard("card" + resourceIndex)
                        });
                }
            }

            var shuffler = new Shuffler();
            shuffler.Shuffle(squareList);
        }

        public void SetupCustomMatchingCardList(Collection<Card> cards)
        {
            squareList.Clear();

            for (int i = 0; i < cards.Count; i++)
            {
                squareList.Add(cards[i]);
            }

            var shuffler = new Shuffler();
            shuffler.Shuffle(squareList);
        }

        private ObservableCollection<Card> squareList;
        public ObservableCollection<Card> SquareListCollection
        {
            get { return squareList; }
            set { this.squareList = value; }
        }

        public bool AttemptToTurnOverSquare(int squareIndex)
        {
            bool gameIsWon = false;

            // If we've already turned over two not-matching cards, turn them back now.
            if (secondCardInMatchAttempt != null)
            {
                ++TryAgainCount;

                firstCardInMatchAttempt.FaceUp = false;
                firstCardInMatchAttempt = null;

                secondCardInMatchAttempt.FaceUp = false;
                secondCardInMatchAttempt = null;

                RaiseNotificationEvent(AppResources.ResourceManager.GetString("UnmatchedTurnedBack"));
                
                return false;
            }

            // Take no action if the click is on a cell that's already face-up.
            var card = squareList[squareIndex];
            if (card.FaceUp)
            {
                return false;
            }

            // Is this the first card turned over in an attempt to find a match?
            if (firstCardInMatchAttempt == null)
            {
                firstCardInMatchAttempt = card;
                TurnUpCard(card);
            }
            else
            {
                // This must be the second card turned over in an attempt to find a match.
                secondCardInMatchAttempt = card;
                TurnUpCard(card);

                // Has a match been found?
                var cardNameFirst = firstCardInMatchAttempt.AccessibleName;
                var cardNameSecond = secondCardInMatchAttempt.AccessibleName;

                if (cardNameFirst == cardNameSecond)
                {
                    // We have a match!
                    firstCardInMatchAttempt.Matched = true;
                    secondCardInMatchAttempt.Matched = true;

                    RaiseNotificationEvent(AppResources.ResourceManager.GetString("ThatsMatch"));

                    firstCardInMatchAttempt = null;
                    secondCardInMatchAttempt = null;

                    // Has the game been won?
                    gameIsWon = GameIsWon();
                }
            }

            return gameIsWon;
        }

        public bool AttemptTurnUpBySelection(object currentSelection)
        {
            if (currentSelection == null)
            {
                return false;
            }

            Card selectedCard = currentSelection as Card;

            int currentSelectionIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (squareList[i] == selectedCard)
                {
                    currentSelectionIndex = i;
                    break;
                }
            }

            if (currentSelectionIndex < 0)
            {
                return false;
            }

            return AttemptToTurnOverSquare(currentSelectionIndex);
        }

        private void TurnUpCard(Card card)
        {
            card.FaceUp = true;

            RaiseNotificationEvent(
                AppResources.ResourceManager.GetString("TurnedUp") + " " + card.AccessibleName);
        }

        private bool GameIsWon()
        {
            for (int i = 0; i < this.squareList.Count; i++)
            {
                if (!this.squareList[i].Matched)
                {
                    return false;
                }
            }

            return true;
        }

        public void ResetGrid(bool shuffle)
        {
            TryAgainCount = 0;

            firstCardInMatchAttempt = null;
            secondCardInMatchAttempt = null;

            for (int i = 0; i < this.squareList.Count; i++)
            {
                this.squareList[i].FaceUp = false;
                this.squareList[i].Matched = false;
            }

            if (shuffle)
            {
                var shuffler = new Shuffler();
                shuffler.Shuffle(squareList);
            }
        }

        public class Shuffler
        {
            private readonly Random random;

            public Shuffler()
            {
                this.random = new Random();
            }

            public void Shuffle(ObservableCollection<Card> collection)
            {
                for (int i = collection.Count; i > 1;)
                {
                    int j = this.random.Next(i);

                    --i;

                    Card temp = collection[i];
                    collection[i] = collection[j];
                    collection[j] = temp;
                }
            }
        }
    }
}
