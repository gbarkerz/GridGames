using GridGames.Views;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpPage : ContentPage
    {
        public HelpPage(Page currentPage)
        {
            InitializeComponent();

            if (currentPage is MatchingPage)
            {
                MatchingGameHelpContent.IsVisible = true;
            }
            else if (currentPage is WheresPage)
            {
                WheresGameHelpContent.IsVisible = true;
            }
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}