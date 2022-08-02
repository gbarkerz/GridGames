using GridGames.ResX;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
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
            "4.1.1",
            "4.1.2",
            "4.1.3",
        };

        // Barker: Load this Q&A from a file.
        private string[] defaultQuestionListCollection =
        {
"Q:Which WCAG relates to a screen reader being able to announce the header associated with a cell in a table?",
"A:1.3.1",
"Q:If a product sets a time limit for customer input, which WCAG relates to customers being able to adjust or turn off the time limit?",
"A:2.2.1",
"Q:Which WCAG relates to providing instructions to customers when they need to input data?",
"A:3.3.2",
"Q:Which WCAG relates to a screen reader being able to announce \"Save, button\" when keyboard focus moves to a Save button?",
"A:4.1.2",
"Q:Which Level AA WCAG relate to the contrast of text and icons against their background?",
"A:1.4.3,1.4.11",
"Q:Which WCAG relates to being able to click a button using the keyboard?",
"A:2.1.1",
"Q:Which WCAG relates to providing helpful suggestions after an error is detected following customer input?",
"A:3.3.3",
"Q:Which Level AA WCAG relates to helping customers understand what information is contained on a page and how that information is organized.",
"A:2.4.6",
"Q:Which WCAG relates to helping customers know the meaning of acronyms shown in content?",
"A:3.1.4",
"Q:Which WCAG relate to providing prerecorded and live captions?",
"A:1.2.2,1.2.4",
"Q:Which WCAG relates to not making a major change in the content of a page simply because keyboard focus moves to an element?",
"A:3.2.1",
"Q:Which WCAG relates to customers being able to turn off animation that occurs when the customer clicks a button?",
"A:2.3.3",
"Q:Which WCAG relates to screen readers announcing a status message even when keyboard focus hasn't moved to the status message?",
"A:4.1.3",
"Q:Which WCAG relate to customers navigating through links with the keyboard following an intuitive path, and knowing which link has keyboard focus?",
"A:2.4.3,2.4.7",
"Q:Which WCAG relates to screen readers being able to navigate through a word cloud, with the navigation path being based on the importance of the words?",
"A:1.3.2",
        };

        public WheresSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("WheresSettings");

            QuestionListCollection = new ObservableCollection<QAPair>();

            DefaultBonusQAList = new Collection<QAPair>();
            LoadDefaultBonusQuestions();

            ShowBonusQuestion = Preferences.Get("ShowBonusQuestion", false);
            BonusQuestionFile = Preferences.Get("BonusQuestionFile", "");

            LoadBonusQuestions(BonusQuestionFile);
        }

        private void LoadDefaultBonusQuestions()
        {
            for (int i = 0; i < 15; i++)
            {
                int qaIndex = i * 2;

                var content = defaultQuestionListCollection[qaIndex];

                var question = content.Substring(2).Trim();

                var qaPair = new QAPair();
                qaPair.Question = question;

                content = defaultQuestionListCollection[qaIndex + 1];
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

                DefaultBonusQAList.Add(qaPair);
            }
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

        private Collection<QAPair> defaultBonusQAList;
        public Collection<QAPair> DefaultBonusQAList
        {
            get { return defaultBonusQAList; }
            set { this.defaultBonusQAList = value; }
        }

        private ObservableCollection<QAPair> questionList;
        public ObservableCollection<QAPair> QuestionListCollection
        {
            get { return questionList; }
            set { this.questionList = value; }
        }

        public void LoadBonusQuestions(string pathQuestions)
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
