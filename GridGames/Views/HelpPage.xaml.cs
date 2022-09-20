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

                MatchingGameHelpContent.IsVisible = true;

                PairsHelpEditor.Focus();
            }
            else if (currentPage is SquaresPage)
            {
                HelpPageTitle.Text = "Squares Help";

                SquaresGameHelpContent.IsVisible = true;

                SquaresHelpEditor.Focus();
            }
            else if (currentPage is WheresPage)
            {
                HelpPageTitle.Text = "Where's WCAG? Help";

                WheresGameHelpContent.IsVisible = true;

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