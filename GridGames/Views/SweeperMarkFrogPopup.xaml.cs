using CommunityToolkit.Maui.Views;

namespace GridGames.Views;

public partial class SweeperMarkFrogPopup : Popup
{
	public SweeperMarkFrogPopup()
	{
		InitializeComponent();
	}

    private void SweepButton_Clicked(object sender, EventArgs e) 
    {
        Close("Sweep");
    }

    private void MarkFrogButton_Clicked(object sender, EventArgs e) 
    {
        Close("MarkFrog");
    }

    private void CancelButton_Clicked(object sender, EventArgs e) 
    {
        Close("");
    }
}
