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

namespace GridGames.ViewModels
{
    public class Card : INotifyPropertyChanged
    {
        public int Index { get; set; }

        public string OriginalAccessibleName { get; set; }
        public string OriginalAccessibleDescription { get; set; }

        private string currentAccessibleName;
        public string CurrentAccessibleName
        {
            get
            {
                return currentAccessibleName;
            }
            set
            {
                SetProperty(ref currentAccessibleName, value);
            }
        }

        private string currentAccessibleDescription;
        public string CurrentAccessibleDescription
        {
            get
            {
                return currentAccessibleDescription;
            }
            set
            {
                SetProperty(ref currentAccessibleDescription, value);
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
                OnPropertyChanged("CurrentAccessibleName");
                OnPropertyChanged("CurrentAccessibleDescription");
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
                OnPropertyChanged("CurrentAccessibleName");
                OnPropertyChanged("CurrentAccessibleDescription");
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

        private MatchingPage matchingPage;

        public void SetMatchingPage(MatchingPage matchingPage)
        {
            this.matchingPage = matchingPage;
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

                    var card = new Card
                        {
                            Index = cardIndex,
                            OriginalAccessibleName = resManager.GetString("DefaultMatchingCard" + resourceIndex + "Name"),
                            CurrentAccessibleName = resManager.GetString("DefaultMatchingCard" + resourceIndex + "Name"),
                            OriginalAccessibleDescription = resManager.GetString("DefaultMatchingCard" + resourceIndex + "Description"),
                            CurrentAccessibleDescription = resManager.GetString("DefaultMatchingCard" + resourceIndex + "Description"),
                            PictureImageSource = GetImageSourceForCard("card" + resourceIndex)
                        };

                    squareList.Add(card);
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
                
                firstCardInMatchAttempt.CurrentAccessibleName = "Face down";
                firstCardInMatchAttempt.CurrentAccessibleDescription = "";

                SetAccessibleDetails(firstCardInMatchAttempt);
                firstCardInMatchAttempt = null;

                secondCardInMatchAttempt.FaceUp = false;

                secondCardInMatchAttempt.CurrentAccessibleName = "Face down";
                secondCardInMatchAttempt.CurrentAccessibleDescription = "";

                SetAccessibleDetails(secondCardInMatchAttempt);
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

                card.CurrentAccessibleName = card.OriginalAccessibleName;
                card.CurrentAccessibleDescription = card.OriginalAccessibleDescription;

                matchingPage.SetAccessibleDetailsOnItem(card);
            }
            else
            {
                // This must be the second card turned over in an attempt to find a match.
                secondCardInMatchAttempt = card;
                TurnUpCard(card);

                // Has a match been found?
                var cardNameFirst = firstCardInMatchAttempt.OriginalAccessibleName;
                var cardNameSecond = secondCardInMatchAttempt.OriginalAccessibleName;

                if (cardNameFirst == cardNameSecond)
                {
                    // We have a match!
                    firstCardInMatchAttempt.Matched = true;
                    firstCardInMatchAttempt.CurrentAccessibleName = "Matched " + firstCardInMatchAttempt.OriginalAccessibleName;
                    firstCardInMatchAttempt.CurrentAccessibleDescription = firstCardInMatchAttempt.OriginalAccessibleDescription;

                    matchingPage.SetAccessibleDetailsOnItem(firstCardInMatchAttempt);

                    secondCardInMatchAttempt.Matched = true;
                    secondCardInMatchAttempt.CurrentAccessibleName = "Matched " + secondCardInMatchAttempt.OriginalAccessibleName;
                    secondCardInMatchAttempt.CurrentAccessibleDescription = secondCardInMatchAttempt.OriginalAccessibleDescription;

                    matchingPage.SetAccessibleDetailsOnItem(secondCardInMatchAttempt);

                    RaiseNotificationEvent(AppResources.ResourceManager.GetString("ThatsMatch"));

                    firstCardInMatchAttempt = null;
                    secondCardInMatchAttempt = null;

                    // Has the game been won?
                    gameIsWon = GameIsWon();
                }
                else
                {
                    card.CurrentAccessibleName = card.OriginalAccessibleName;
                    card.CurrentAccessibleDescription = card.OriginalAccessibleDescription;

                    matchingPage.SetAccessibleDetailsOnItem(card);
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
                AppResources.ResourceManager.GetString("TurnedUp") + " " + 
                card.OriginalAccessibleName);
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
                var card = new Card
                {
                    Index = squareList[i].Index,
                    OriginalAccessibleName = squareList[i].OriginalAccessibleName,
                    OriginalAccessibleDescription = squareList[i].OriginalAccessibleDescription,
                    PictureImageSource = squareList[i].PictureImageSource
                };

                card.FaceUp = false;
                card.Matched = false;

                card.CurrentAccessibleName = "Face down";
                card.CurrentAccessibleDescription = "";

                this.squareList[i] = card;
            }

            if (shuffle)
            {
                var shuffler = new Shuffler();
                shuffler.Shuffle(squareList);
            }

            for (int i = 0; i < this.squareList.Count; i++)
            {
                SetAccessibleDetails(this.squareList[i]);
            }
        }

        private void SetAccessibleDetails(Card card)
        {
            this.matchingPage.SetAccessibleDetailsOnItem(card);
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
