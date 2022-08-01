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

                PairsHelpEditor.Focus();
            }
            else if (currentPage is WheresPage)
            {
                HelpPageTitle.Text = "Where's WCAG? Help";

                MatchingGameHelpContent.IsVisible = false;

                WheresHelpEditor.Focus();
            }
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}