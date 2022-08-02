using GridGames.ResX;
using GridGames.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GridGames.Views;

public partial class WCAGPage : ContentPage
{
    private QAPair qaPair;
    private WheresPage wheresPage;
    private bool gameIsWon;
	private Collection<string> playerAnswers = new Collection<string>();
    private Collection<CheckBox> checkedAnswers = new Collection<CheckBox>();
    private bool verifyingAnswer = false;

    public WCAGPage()
	{
		InitializeComponent();

        WCAG1.IsVisible = false;
        WCAG2.IsVisible = false;
        WCAG3.IsVisible = false;
        WCAG4.IsVisible = false;
    }

    private void WCAGPage_Loaded(object sender, EventArgs e)
    {
        WCAGScrollView.IsVisible = true;
    }

    public void PrepareToAskQuestion(WheresPage wheresPage, bool gameIsWon, QAPair qaPair)
    {
        this.qaPair = qaPair;
        this.gameIsWon = gameIsWon;
        this.wheresPage = wheresPage;

        WCAGQuestion.Text = qaPair.Question;

        playerAnswers.Clear();
        checkedAnswers.Clear();

        verifyingAnswer = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        PerceivablePicker.SelectedItem = null;
        OperablePicker.SelectedItem = null;
        UnderstandablePicker.SelectedItem = null;
        RobustPicker.SelectedItem = null;

        PerceivablePicker.Focus();
    }

    private async void SubmitButton_Clicked(object sender, EventArgs e)
	{
        if (checkedAnswers.Count == 0)
        {
            await DisplayAlert("WCAG Bonus Question",
                AppResources.ResourceManager.GetString("BonusAnswerRequired"),
                "OK");

            return;
        }

        verifyingAnswer = true;

        foreach (CheckBox checkedAnswer in checkedAnswers)
        {
            var description = SemanticProperties.GetDescription(checkedAnswer);
            var numberSeparator = description.IndexOf(' ');
            var wcagNumber = description.Substring(0, numberSeparator);

            playerAnswers.Add(wcagNumber);

            checkedAnswer.IsChecked = false;
        }

        bool foundIncorrectAnswer = false;

		foreach (var correctAnswer in qaPair.Answers)
		{
            if (!playerAnswers.Contains(correctAnswer))
			{
				foundIncorrectAnswer = true;

                break;
			}

			playerAnswers.Remove(correctAnswer);
		}
		
        if (!foundIncorrectAnswer)
		{
			if (playerAnswers.Count > 0)
			{
				foundIncorrectAnswer = true;
            }
		}

	   await DisplayAlert("WCAG Bonus Question",
            foundIncorrectAnswer ? 
                "Sorry, your answer wasn't the same answer as the one that I have." :
	            "Congratulations! Your answer was the same answer as the one I have.",
            "OK");

        if (!foundIncorrectAnswer)
        {
            var vm = wheresPage.BindingContext as WheresViewModel;
            vm.BonusQuestionCount++;
        }

        await CloseWCAGPage();
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        verifyingAnswer = true;

        foreach (CheckBox checkedAnswer in checkedAnswers)
        {
            checkedAnswer.IsChecked = false;
        }

        await CloseWCAGPage();
    }

    private async Task CloseWCAGPage()
    {
        if (gameIsWon)
        {
            await wheresPage.OfferToRestartGame(true);
        }

        await Navigation.PopModalAsync();
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
        if (verifyingAnswer)
        {
            return;
        }

		var checkboxWCAG = (CheckBox)sender;

		if (checkboxWCAG.IsChecked)
		{
			checkedAnswers.Add(checkboxWCAG);
        }
		else
		{
            checkedAnswers.Remove(checkboxWCAG);
        }
    }

    private void JumpToPicker_SelectedIndexChanged(object sender, EventArgs e)
	{
        var picker = (Picker)sender;

        WCAG1.IsVisible = false;
        WCAG2.IsVisible = false;
        WCAG3.IsVisible = false;
        WCAG4.IsVisible = false;

        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            CheckBox targetCheckBox = null;

            switch (picker.ClassId)
			{
				case "Perceivable":

                    WCAG1.IsVisible = true;

                    CheckBox[] boxesPerceivable = {
                        Perceivable11CheckBox,
                        Perceivable12CheckBox,
                        Perceivable13CheckBox,
                        Perceivable14CheckBox
                    };

                    targetCheckBox = boxesPerceivable[selectedIndex];

                    break;

				case "Operable":

                    WCAG2.IsVisible = true;

                    CheckBox[] boxesOperable = {
						Operable21CheckBox,
						Operable22CheckBox,
						Operable23CheckBox,
						Operable24CheckBox,
						Operable25CheckBox
					};

					targetCheckBox = boxesOperable[selectedIndex];

					break;

				case "Understandable":

                    WCAG3.IsVisible = true;

                    CheckBox[] boxesUnderstandable = {
                        Understandable31CheckBox,
                        Understandable32CheckBox,
                        Understandable33CheckBox
                    };

                    targetCheckBox = boxesUnderstandable[selectedIndex];
                    
					break;

				case "Robust":

                    WCAG4.IsVisible = true;

                    CheckBox[] boxesRobust = {
                        Robust41CheckBox,
                    };

                    targetCheckBox = boxesRobust[selectedIndex];
                    
					break;

				default:

					break;
			}

			if (targetCheckBox != null)
			{
                targetCheckBox.Focus();

                timer = new Timer(new TimerCallback((s) => ScrollToWCAGCheckBox(targetCheckBox)),
                                   null, 
                                   TimeSpan.FromMilliseconds(500), 
                                   TimeSpan.FromMilliseconds(Timeout.Infinite));
            }
        }
    }

    private Timer timer;

    private void ScrollToWCAGCheckBox(CheckBox targetCheckBox)
    {
        timer.Dispose();

        var newThread = new System.Threading.Thread(() =>
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                Debug.WriteLine("Perform delayed scroll to WCAG CheckBox.");

                WCAGScrollView.ScrollToAsync(targetCheckBox, ScrollToPosition.Center, true);
            });
        });
        newThread.Start();
    }
}