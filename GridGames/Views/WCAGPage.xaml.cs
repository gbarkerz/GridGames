namespace GridGames.Views;

public partial class WCAGPage : ContentPage
{
	public WCAGPage()
	{
		InitializeComponent();
	}

	private async void CloseButton_Clicked(object sender, EventArgs e)
	{
        await Navigation.PopModalAsync();
    }
}