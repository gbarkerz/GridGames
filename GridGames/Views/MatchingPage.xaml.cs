using GridGames.ResX;
using GridGames.Services;
using GridGames.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MatchingPage : ContentPage
    {
        private bool previousShowCustomPictures;
        private string previousPicturePathMatching;

        public MatchingPage()
        {
            InitializeComponent();

            SquaresCollectionView.SizeChanged += SquaresCollectionView_SizeChanged;
            SquaresCollectionView.Focused += SquaresCollectionView_Focused;

            Application.Current.RequestedThemeChanged += (s, a) =>
            {
                var currentTheme = a.RequestedTheme;
                if (currentTheme == AppTheme.Unspecified)
                {
                    currentTheme = Application.Current.PlatformAppTheme;
                }

                var vm = this.BindingContext as MatchingViewModel;
                vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);
            };

            (this.BindingContext as MatchingViewModel).SetMatchingPage(this);
        }

        private void SquaresCollectionView_Focused(object sender, FocusEventArgs e)
        {
            // If the grid has no selected item by the time it gets focus, 
            // select the first square now. The grid must always have a 
            // selected item if it's to respond to keyboard input.
            var item = SquaresCollectionView.SelectedItem as Card;
            if (item == null)
            {
                var vm = this.BindingContext as MatchingViewModel;
                SquaresCollectionView.SelectedItem = vm.SquareListCollection[0];
            }
        }

        // Barker: TEMPORARY. It seems that https://github.com/dotnet/maui/issues/8722 is impacting
        // the ability to update items' accessible names. Until this is resolved, this app takes
        // a variety of steps which seems to get things working well enough for the player. The steps
        // may seems excessive, but leave them here until issue 8722 is resolved, and this whole
        // thing can be re-examined, (and hopefully all the temporary code removed).
        private bool doneSetAccessibleNamesOnItems = false;
        private int countSquaresAddedToCollection = 0;

        private void SquaresCollectionView_ChildAdded(object sender, ElementEventArgs e)
        {
            // When the set of items is first being set up, once all the squares have
            // been added, set the initial accessible details on all the items.
            if (!doneSetAccessibleNamesOnItems && (e.Element is Grid))
            {
                ++countSquaresAddedToCollection;

                if (countSquaresAddedToCollection > 15)
                {
                    doneSetAccessibleNamesOnItems = true;

                    SetInitialAccessibleNamesOnItems();
                }
            }
        }

        public void SetInitialAccessibleNamesOnItems()
        {
            var vm = this.BindingContext as MatchingViewModel;

            for (int i = 0; i < 16; i++)
            {
                vm.SetFaceDownAccessibleDetails(vm.SquareListCollection[i]);

                SetAccessibleDetailsOnItem(vm.SquareListCollection[i]);
            }
        }

        // Take excessive action when setting the accessible name and description on an item.
        // This seems to leave the item in a state that's usable for the player.
        public void SetAccessibleDetailsOnItem(Card card)
        {
            // Set the accessible name twice, which apparently leaves the bound UI in a usable state.
            var temp = card.CurrentAccessibleName;
            card.CurrentAccessibleName = card.CurrentAccessibleName + ".";
            card.CurrentAccessibleName = temp;

            // Set the accessible description twice, which apparently leaves the bound UI in a usable state.
            temp = card.CurrentAccessibleDescription;
            card.CurrentAccessibleDescription = card.CurrentAccessibleDescription + ".";
            card.CurrentAccessibleDescription = temp;
        }

        // Barker: End of TEMPORARY code.

        private void SquaresCollectionView_SizeChanged(object sender, EventArgs e)
        {
            if (SquaresCollectionView.Height > 0)
            {
                var vm = this.BindingContext as MatchingViewModel;
                vm.GridRowHeight = (SquaresCollectionView.Height / 4) - 8;
            }
        }

        private async void MatchingGameSettingsButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                var settingsPage = new MatchingGameSettingsPage();
                await Navigation.PushModalAsync(settingsPage);
            }
        }

        // This gets called when switching from the Squares Game to the Matching Game,
        // and also when closing the Matching Settings page.
        protected override void OnAppearing()
        {
            Debug.Write("Matching Game: OnAppearing called.");

            base.OnAppearing();

            Preferences.Set("InitialGame", "Matching");

            // We must account for the app settings changing since the page was last shown.

            var vm = this.BindingContext as MatchingViewModel;

            vm.FirstRunMatching = Preferences.Get("FirstRunMatching", true);
            if (vm.FirstRunMatching)
            {
                vm.RaiseNotificationEvent(
                    MatchingWelcomeTitleLabel.Text + ", " + MatchingWelcomeTitleInstructions.Text);
            }

            var currentTheme = Application.Current.UserAppTheme;
            if (currentTheme == AppTheme.Unspecified)
            {
                currentTheme = Application.Current.PlatformAppTheme;
            }

            vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);

            // Default to Fill and Clip.
            vm.PictureAspect = (Aspect)Preferences.Get("PictureAspect", 1);

            // Has something changed related to custom picture use since the last time
            // we were in OnAppearing()?
            var showCustomPictures = Preferences.Get("ShowCustomPictures", false);
            var picturePathMatching = Preferences.Get("PicturePathMatching", "");

            if ((showCustomPictures != previousShowCustomPictures) ||
                (picturePathMatching != previousPicturePathMatching))
            {
                SetUpCards();

                previousShowCustomPictures = showCustomPictures;
                previousPicturePathMatching = picturePathMatching;
            }
        }

        public void SetUpCards()
        {
            var vm = this.BindingContext as MatchingViewModel;

            var showCustomPictures = Preferences.Get("ShowCustomPictures", false);
            var picturePathMatching = Preferences.Get("PicturePathMatching", "");

            // Should we be showing the default pictures?
            if (!showCustomPictures || String.IsNullOrWhiteSpace(picturePathMatching))
            {
                vm.SetupDefaultMatchingCardList();
            }
            else
            {
                // Attempt to load up the custom pictures and associated accessible data.
                var customPictures = new Collection<Card>();

                bool resetToUseDefaultPictures = false;

                // We should have 8 pairs of cards.
                for (int i = 0; i < 8; i++)
                {
                    // For each of the 2 cards in each pair...
                    for (int j = 0; j < 2; j++)
                    {
                        var card = new Card();

                        // The index is from 1-16.
                        card.Index = (i * 2) + j + 1;

                        string settingName = "Card" + (i + 1) + "Path";
                        var cardPath = Preferences.Get(settingName, "");
                        if (!File.Exists(cardPath))
                        {
                            Debug.WriteLine("Pairs: Card path missing.");

                            resetToUseDefaultPictures = true;

                            break;
                        }

                        try
                        {
                            card.PictureImageSource = ImageSource.FromFile(cardPath);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Pairs: Failed to load image. " + ex.Message);

                            resetToUseDefaultPictures = true;

                            break;
                        }

                        settingName = "Card" + (i + 1) + "Name";
                        card.OriginalAccessibleName = Preferences.Get(settingName, "");
                        if (String.IsNullOrWhiteSpace(card.OriginalAccessibleName))
                        {
                            Debug.WriteLine("Pairs: Accessible name missing.");

                            resetToUseDefaultPictures = true;

                            break;
                        }

                        settingName = "Card" + (i + 1) + "Description";
                        card.OriginalAccessibleDescription = Preferences.Get(settingName, "");

                        vm.SetFaceDownAccessibleDetails(card);

                        customPictures.Add(card);
                    }

                    if (resetToUseDefaultPictures)
                    {
                        break;
                    }
                }

                // Now use the custom pictures is we have all the required data.
                if (!resetToUseDefaultPictures)
                {
                    vm.SetupCustomMatchingCardList(customPictures);
                }
                else
                {
                    vm.SetupDefaultMatchingCardList();
                }
            }
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            int itemIndex = (int)(e as Microsoft.Maui.Controls.TappedEventArgs).Parameter;
            await ReactToInputOnCard(itemIndex);
        }

        public async void ReactToKeyInputOnSelectedCard()
        {
            var item = SquaresCollectionView.SelectedItem as Card;
            if (item != null)
            {
                await ReactToInputOnCard(item.Index);
            }
        }

        public async void ShowHelp()
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                await Navigation.PushModalAsync(new HelpPage(this));

                SquaresCollectionView.Focus();
            }
        }

        public void RestartGame()
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                vm.ResetGrid(true);
            }
        }

        private async Task ReactToInputOnCard(int itemIndex)
        {
            Debug.WriteLine("Grid Games: Input on Square " + itemIndex);

            var vm = this.BindingContext as MatchingViewModel;
            if (vm.FirstRunMatching)
            {
                return;
            }

            int itemCollectionIndex = GetItemCollectionIndexFromItemIndex(itemIndex);
            if (itemCollectionIndex == -1)
            {
                return;
            }

            bool gameIsWon = vm.AttemptToTurnOverSquare(itemCollectionIndex);
            if (gameIsWon)
            {
                await OfferToRestartGame();
            }
        }

        private async Task OfferToRestartGame()
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (!vm.FirstRunMatching)
            {
                var message = String.Format(
                    AppResources.ResourceManager.GetString("MatchingWonInMoves"), 8 + vm.TryAgainCount);

                var answer = await DisplayAlert(
                    AppResources.ResourceManager.GetString("Congratulations"),
                    message,
                    AppResources.ResourceManager.GetString("Yes"),
                    AppResources.ResourceManager.GetString("No"));
                if (answer)
                {
                    vm.ResetGrid(true);
                }
            }
        }

        public int GetItemCollectionIndexFromItemIndex(int itemIndex)
        {
            var vm = this.BindingContext as MatchingViewModel;

            int itemCollectionIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (vm.SquareListCollection[i].Index == itemIndex)
                {
                    itemCollectionIndex = i;
                    break;
                }
            }

            return itemCollectionIndex;
        }

        private async void MatchingGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = this.BindingContext as MatchingViewModel;
            if (vm.FirstRunMatching)
            {
                return;
            }

            Debug.WriteLine("Matching Grid Game: Selection changed. Selection count is " + e.CurrentSelection.Count);

            // No action required here if there is no selected item.
            if (e.CurrentSelection.Count > 0)
            {
                bool gameIsWon = vm.AttemptTurnUpBySelection(e.CurrentSelection[0]);

                // Clear the selection now to support the same square moving again.
                SquaresCollectionView.SelectedItem = null;

                if (gameIsWon)
                {
                    await OfferToRestartGame();
                }
            }
        }

        private void MatchingWelcomeOKButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as MatchingViewModel;
            vm.FirstRunMatching = false;
        }

        private async void FallthroughGrid_Tapped(object sender, EventArgs e)
        {
            await DisplayAlert(
                AppResources.ResourceManager.GetString("GridGames"),
                AppResources.ResourceManager.GetString("FallthroughTapMessage"),
                AppResources.ResourceManager.GetString("OK"));
        }
    }
}
