namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpPage : ContentPage
    {
        //public HelpPage(bool matchingPage)
        public HelpPage()
        {
            bool matchingPage = true; // GBTest

            InitializeComponent();

            if (matchingPage)
            {
                //SquaresGameHelpTitle.IsVisible = false;
                SquaresGameHelpContent.IsVisible = false;
            }
            else
            {
                //MatchingGameHelpTitle.IsVisible = false;
                MatchingGameHelpContent.IsVisible = false;
            }
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}