using CommunityToolkit.Maui.Views;

namespace GridGames;

public partial class SudokuInputPopup : Popup
{
	public SudokuInputPopup()
	{
		InitializeComponent();
    }

    private void NumberButton_Clicked(object sender, EventArgs e) 
	{
		var numberButton = sender as Button;

		Close(numberButton.Text);
	}

    private void CloseButton_Clicked(object sender, EventArgs e) 
	{
        Close("");
    }
}
