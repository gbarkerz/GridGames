using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

using GridGames.ResX;
using System.IO;
using System.Reflection;

namespace GridGames.ViewModels
{
    public class WheresCard : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string WCAGName { get; set; }

        // Barker todo: Check whether all the props below really need to raise PropertyChanged events.

        private string accessibleName;
        public string AccessibleName
        {
            get
            {
                string name = "";

                if (Index == 15)
                {
                    name = "Get a tip";
                }
                else
                {
                    name = WCAGNumber;

                    if (isFound)
                    {
                        name += " " + WCAGName;
                    }
                }

                return name;
            }
            set
            {
                SetProperty(ref accessibleName, value);
            }
        }

        private string wcagNumber;
        public string WCAGNumber
        {
            get
            {
                return wcagNumber;
            }
            set
            {
                SetProperty(ref wcagNumber, value);
            }
        }

        private bool isFound;
        public bool IsFound
        {
            get
            {
                return isFound;
            }
            set
            {
                SetProperty(ref isFound, value);

                // Other properties may change as a result of this.
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

    public class WheresViewModel : BaseViewModel
    {
        // Barker: Unify the use of the two arrays here.

        // Both arrays are used to initialise the cards. Once that's done, the related properties
        // on the cards don't change for the duration of a game, but the wcagNames array is randomly
        // rearranged to provide the questions.

        string[] wcagNumbers =
        {
            "1",
            "1.1",
            "1.2",
            "1.3",
            "1.4",

            "2",
            "2.1",
            "2.2",
            "2.3",
            "2.4",
            "2.5",

            "3",
            "3.1",
            "3.2",
            "3.3",
        };

        private string[] wcagNames =
        {
            "Perceivable",
            "Text Alternatives",
            "Time-based Media",
            "Adaptable",
            "Distinguishable",

            "Operable",
            "Keyboard Accessible",
            "Enough Time",
            "Seizures and Physical Reactions",
            "Navigable",
            "Input Modalities",

            "Understandable",
            "Readable",
            "Predictable",
            "Input Assistance"
        };

        public WheresViewModel()
        {
            Title = AppResources.ResourceManager.GetString("Wheres");

            wheresList = new ObservableCollection<WheresCard>();

            AnswerAttemptCount = 0;

            wheresSettingsVM = new WheresSettingsViewModel();
        }

        public int AnswerAttemptCount { get; set; }
        public int CurrentQuestionIndex { get; set; }

        private WheresSettingsViewModel wheresSettingsVM;
        public WheresSettingsViewModel WheresSettingsVM
        {
            get { return wheresSettingsVM; }
            set
            {
                SetProperty(ref wheresSettingsVM, value);
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

        private string currentQuestionWCAG;
        public string CurrentQuestionWCAG
        {
            get
            {
                return currentQuestionWCAG;
            }
            set
            {
                SetProperty(ref currentQuestionWCAG, value);
            }
        }

        private bool firstRunWheres = true;
        public bool FirstRunWheres
        {
            get
            {
                return firstRunWheres;
            }
            set
            {
                if (firstRunWheres != value)
                {
                    SetProperty(ref firstRunWheres, value);

                    Preferences.Set("FirstRunWheres", firstRunWheres);
                }
            }
        }

        public void SetupWheresCardList()
        {
            wheresList.Clear();

            // We have 15 cards and a tip.
            for (int i = 0; i < 16; ++i)
            {
                wheresList.Add(
                    new WheresCard
                    {
                        Index = i,
                        WCAGNumber = (i < 15 ? wcagNumbers[i] : "?"),
                        WCAGName = (i  < 15 ? wcagNames[i] : "Tip")
                    });
            }

            var shuffler = new Shuffler();
            shuffler.Shuffle(wcagNames);

            CurrentQuestionIndex = 0;
            CurrentQuestionWCAG = wcagNames[CurrentQuestionIndex];
        }

        private ObservableCollection<WheresCard> wheresList;
        public ObservableCollection<WheresCard> WheresListCollection
        {
            get { return wheresList; }
            set { this.wheresList = value; }
        }

        public bool AttemptToAnswerQuestion(int squareIndex, out bool answerIsCorrect)
        {
            bool gameIsWon = false;

            answerIsCorrect = false;

            // Take no action if the click is on a cell that's already found.
            var card = wheresList[squareIndex];
            if (card.IsFound)
            {
                return false;
            }

            // Does this card's WCAG match the current question?
            if (card.WCAGName == currentQuestionWCAG)
            {
                answerIsCorrect = true;

                card.IsFound = true;

                // Has the game been won?
                gameIsWon = GameIsWon();
                if (!gameIsWon)
                {
                    ++CurrentQuestionIndex;

                    CurrentQuestionWCAG = wcagNames[CurrentQuestionIndex];

                    var message = String.Format(
                        AppResources.ResourceManager.GetString("CorrectWCAG"),
                        card.WCAGName, CurrentQuestionWCAG);

                    RaiseNotificationEvent(message);
                }
            }
            else if (card.WCAGName != "Tip")
            {
                ++AnswerAttemptCount;

                var message = String.Format(
                    AppResources.ResourceManager.GetString("IncorrectWCAG"),
                    card.WCAGNumber, CurrentQuestionWCAG);

                RaiseNotificationEvent(message);
            }

            return gameIsWon;
        }

        public bool AttemptTurnUpBySelection(object currentSelection)
        {
            if (currentSelection == null)
            {
                return false;
            }

            WheresCard selectedCard = currentSelection as WheresCard;

            int currentSelectionIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (wheresList[i] == selectedCard)
                {
                    currentSelectionIndex = i;
                    break;
                }
            }

            if (currentSelectionIndex < 0)
            {
                return false;
            }

            bool answerIsCorrect;
            return AttemptToAnswerQuestion(currentSelectionIndex, out answerIsCorrect);
        }

        private bool GameIsWon()
        {
            for (int i = 0; i < this.wheresList.Count - 1; i++)
            {
                if (!this.wheresList[i].IsFound)
                {
                    return false;
                }
            }

            return true;
        }

        public void ResetGrid(bool shuffle)
        {
            AnswerAttemptCount = 0;

            if (shuffle)
            {
                var shuffler = new Shuffler();
                shuffler.Shuffle(wcagNames);
            }

            CurrentQuestionIndex = 0;
            CurrentQuestionWCAG = wcagNames[CurrentQuestionIndex];

            for (int i = 0; i < this.wheresList.Count; i++)
            {
                this.wheresList[i].IsFound = false;
            }
        }

        public class Shuffler
        {
            private readonly Random random;

            public Shuffler()
            {
                this.random = new Random();
            }

            public void Shuffle(string[] titles)
            {
                for (int i = titles.Length; i > 1;)
                {
                    int j = this.random.Next(i);

                    --i;

                    string temp = titles[i];
                    titles[i] = titles[j];
                    titles[j] = temp;
                }
            }
        }
    }
}
