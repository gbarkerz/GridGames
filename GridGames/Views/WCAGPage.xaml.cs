using GridGames.ViewModels;
using System.Collections.ObjectModel;

namespace GridGames.Views;

public partial class WCAGPage : ContentPage
{
	private QAPair qaPair;
	private Collection<string> playerAnswers = new Collection<string>();

	public WCAGPage(QAPair qaPair)
	{
		InitializeComponent();

		this.qaPair = qaPair;

		WCAGQuestion.Text = qaPair.Question;
    }

	private async void SubmitButton_Clicked(object sender, EventArgs e)
	{
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
			"Your answer was " + 
	           (foundIncorrectAnswer ? "not correct." : "correct."),
            "OK");

		await Navigation.PopModalAsync();
    }

	private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var checkboxWCAG = (CheckBox)sender;

        var description = SemanticProperties.GetDescription(checkboxWCAG);
		var numberSeparator = description.IndexOf(' ');
		var wcagNumber = description.Substring(0, numberSeparator);

		if (checkboxWCAG.IsChecked)
		{
			playerAnswers.Add(wcagNumber);
        }
		else
		{
			playerAnswers.Remove(wcagNumber);
        }
    }
}