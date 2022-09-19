using GridGames.Views;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpPage : ContentPage
    {
        private Page currentPage;

        public HelpPage(Page currentPage)
        {
            this.currentPage = currentPage;

            InitializeComponent();

            this.Loaded += HelpPage_Loaded;
        }

        private void HelpPage_Loaded(object sender, EventArgs e)
        {
            if (currentPage is MatchingPage)
            {
                HelpPageTitle.Text = "Pairs Help";

                WheresGameHelpContent.IsVisible = false;
                SquaresGameHelpContent.IsVisible = false;

                PairsHelpEditor.Focus();
            }
            else if (currentPage is SquaresPage)
            {
                HelpPageTitle.Text = "Squares Help";

                WheresGameHelpContent.IsVisible = false;
                MatchingGameHelpContent.IsVisible = false;

                SquaresHelpEditor.Focus();
            }
            else if (currentPage is WheresPage)
            {
                HelpPageTitle.Text = "Where's WCAG? Help";

                MatchingGameHelpContent.IsVisible = false;
                SquaresGameHelpContent.IsVisible = false;

                WheresHelpEditor.Focus();
            }

            SemanticProperties.SetDescription(WheresHelpEditor, HelpPageTitle.Text); 
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}