using GridGames.ResX;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GridGames.ViewModels
{
    // View model for the Where's WCAG Settings page in the app.
    public class WheresSettingsViewModel : BaseViewModel
    {
        private string[] validWCAGNumbers =
            {
                "1.1.1",
                "1.2.1",
                "1.2.2",
                "1.2.3",
                "1.2.4",
                "1.2.5",
                "1.2.6",
                "1.2.7",
                "1.2.8",
                "1.2.9",
                "1.3.1",
                "1.3.2",
                "1.3.3",
                "1.3.4",
                "1.3.5",
                "1.3.6",
                "1.4.1",
                "1.4.2",
                "1.4.3",
                "1.4.4",
                "1.4.5",
                "1.4.6",
                "1.4.7",
                "1.4.8",
                "1.4.9",
                "1.4.10",
                "1.4.11",
                "1.4.12",
                "1.4.13",
                "2.1.1",
                "2.1.2",
                "2.1.3",
                "2.1.4",
                "2.2.1",
                "2.2.2",
                "2.2.3",
                "2.2.4",
                "2.2.5",
                "2.2.6",
                "2.3.1",
                "2.3.2",
                "2.3.3",
                "2.4.1",
                "2.4.2",
                "2.4.3",
                "2.4.4",
                "2.4.5",
                "2.4.6",
                "2.4.7",
                "2.4.8",
                "2.4.9",
                "2.4.10",
                "2.4.11",
                "2.4.12",
                "2.4.13",
                "2.5.1",
                "2.5.2",
                "2.5.3",
                "2.5.4",
                "2.5.5",
                "2.5.6",
                "2.5.7",
                "2.5.8",
                "3.1.1",
                "3.1.2",
                "3.1.3",
                "3.1.4",
                "3.1.5",
                "3.1.6",
                "3.2.1",
                "3.2.2",
                "3.2.3",
                "3.2.4",
                "3.2.5",
                "3.2.6",
                "3.2.7",
                "3.3.1",
                "3.3.2",
                "3.3.3",
                "3.3.4",
                "3.3.5",
                "3.3.6",
                "3.3.7",
                "3.3.8",
            };

        public WheresSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("WheresSettings");

            this.QuestionListCollection = new ObservableCollection<QAPair>();

            PlaySoundOnMatch = Preferences.Get("WheresPlaySoundOnMatch", false);
            PlaySoundOnNotMatch = Preferences.Get("WheresPlaySoundOnNotMatch", false);
            
            ShowBonusQuestion = Preferences.Get("ShowBonusQuestion", false);
            BonusQuestionFile = Preferences.Get("BonusQuestionFile", "");

            LoadBonusQuestions(BonusQuestionFile);
        }

        private bool showBonusQuestion;
        public bool ShowBonusQuestion
        {
            get
            {
                return showBonusQuestion;
            }
            set
            {
                if (showBonusQuestion != value)
                {
                    SetProperty(ref showBonusQuestion, value);

                    Preferences.Set("ShowBonusQuestion", value);
                }
            }
        }

        private string bonusQuestionFile;
        public string BonusQuestionFile
        {
            get
            {
                return bonusQuestionFile;
            }
            set
            {
                if (bonusQuestionFile != value)
                {
                    SetProperty(ref bonusQuestionFile, value);

                    Preferences.Set("BonusQuestionFile", value);
                }
            }
        }

        private ObservableCollection<QAPair> questionList;
        public ObservableCollection<QAPair> QuestionListCollection
        {
            get { return questionList; }
            set { this.questionList = value; }
        }

        private bool playSoundOnMatch;
        public bool PlaySoundOnMatch
        {
            get
            {
                return playSoundOnMatch;
            }
            set
            {
                if (playSoundOnMatch != value)
                {
                    SetProperty(ref playSoundOnMatch, value);

                    Preferences.Set("WheresPlaySoundOnMatch", value);
                }
            }
        }

        private bool playSoundOnNotMatch;
        public bool PlaySoundOnNotMatch
        {
            get
            {
                return playSoundOnNotMatch;
            }
            set
            {
                if (playSoundOnNotMatch != value)
                {
                    SetProperty(ref playSoundOnNotMatch, value);

                    Preferences.Set("WheresPlaySoundOnNotMatch", value);
                }
            }
        }

        public async void LoadBonusQuestions(string pathQuestions)
        {
            QuestionListCollection.Clear();

            if (!String.IsNullOrWhiteSpace(pathQuestions))
            {
                try
                {
                    StreamReader streamReader = null;
                    if ((streamReader = new StreamReader(pathQuestions)) != null)
                    {
                        string content = null;
                        while ((content = streamReader.ReadLine()) != null)
                        {
                            if (!content.StartsWith("Q:"))
                            {
                                break;
                            }

                            var question = content.Substring(2).Trim();

                            if ((content = streamReader.ReadLine()) == null)
                            {
                                // The latest question is missing an answer.
                                break;
                            }

                            var qaPair = new QAPair();
                            qaPair.Question = question;

                            if (!content.StartsWith("A:"))
                            {
                                break;
                            }

                            var answer = content.Substring(2).Trim();

                            var foundInvalidAnswer = false;

                            var answerSeparator = answer.IndexOf(',');
                            while (answerSeparator != -1)
                            {
                                foundInvalidAnswer = ProcessNextAnswer(qaPair, answer, answerSeparator);
                                if (foundInvalidAnswer)
                                {
                                    break;
                                }

                                answer = answer.Substring(answerSeparator + 1).Trim();
                                answerSeparator = answer.IndexOf(',');
                            }

                            if (!foundInvalidAnswer)
                            {
                                foundInvalidAnswer = ProcessNextAnswer(qaPair, answer, answerSeparator);
                            }

                            if (!foundInvalidAnswer)
                            {
                                foundInvalidAnswer = (qaPair.Answers.Count == 0);
                            }

                            if (foundInvalidAnswer)
                            {
                                break;
                            }

                            QuestionListCollection.Add(qaPair);

                            if (QuestionListCollection.Count >= 15)
                            {
                                break;
                            }
                        }

                        streamReader.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GridGames: Browse exception: " + ex.Message);
                }

                if (QuestionListCollection.Count != 15)
                {
                    QuestionListCollection.Clear();

                    BonusQuestionFile = "";

                    // Barker: Present a helpful message somewhere.
                    //await DisplayAlert("Where's WCAG Bonus Questions",
                    //    "Sorry, I found something unexpected in the Q&A file.",
                    //    "OK");
                }
                else
                {
                    BonusQuestionFile = pathQuestions;
                }
            }
        }

        private bool ProcessNextAnswer(QAPair qaPair, string answer, int answerSeparator)
        {
            bool foundInvalidAnswer = true;

            string nextAnswer = answer;
            if (answerSeparator != -1)
            {
                nextAnswer = answer.Substring(0, answerSeparator).Trim();
            }

            nextAnswer = nextAnswer.Trim();
            if (nextAnswer.Length == 0)
            {
                // Don't consider this invalid, as we might have only encountered a trailing comma on the answer line.
                return false;
            }

            if (IsAnswerValid(nextAnswer))
            {
                qaPair.Answers.Add(nextAnswer);

                foundInvalidAnswer = false;
            }

            return foundInvalidAnswer;
        }

        private bool IsAnswerValid(string answer)
        {
            bool answerIsValid = false;

            foreach (string validWCAGNumber in validWCAGNumbers)
            {
                if (answer == validWCAGNumber)
                {
                    answerIsValid = true;

                    break;
                }
            }

            return answerIsValid;
        }
    }
}
