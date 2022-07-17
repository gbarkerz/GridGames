using GridGames.Services;
using GridGames.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using GridGames.ResX;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WheresPage : ContentPage
    {
        private bool restartGame = true;

        public WheresPage()
        {
            InitializeComponent();

            SquaresCollectionView.SizeChanged += SquaresCollectionView_SizeChanged;
        }

        private void SquaresCollectionView_SizeChanged(object sender, EventArgs e)
        {
            if (SquaresCollectionView.Height > 0)
            {
                var vm = this.BindingContext as WheresViewModel;
                vm.GridRowHeight = (SquaresCollectionView.Height / 4) - 8;
            }
        }

        protected override void OnAppearing()
        {
            Debug.Write("Wheres Game: OnAppearing called.");

            base.OnAppearing();

            Preferences.Set("InitialGame", "Wheres");

            var vm = this.BindingContext as WheresViewModel;
            //vm.FirstRunWheres = Preferences.Get("FirstRunWheres", true);
            vm.FirstRunWheres = false;
            if (vm.FirstRunWheres)
            {
                var service = DependencyService.Get<IGridGamesPlatformAction>();
                service.ScreenReaderAnnouncement(
                    WheresWelcomeTitleLabel.Text + ", " + WheresWelcomeTitleInstructions.Text);
            }

            if (restartGame)
            {
                restartGame = false;

                // Reset all cached game progress setting, but don't bother to shuffle.
                vm.ResetGrid(false);

                vm.SetupWheresCardList();
            }
        }

        private async void WheresGameSettingsButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as WheresViewModel;
            if (!vm.FirstRunWheres)
            {
                var settingsPage = new WheresGameSettingsPage(vm.WheresSettingsVM);
                await Navigation.PushModalAsync(settingsPage);
            }
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            int itemIndex = (int)(e as TappedEventArgs).Parameter;

            await ReactToInputOnCard(itemIndex);
        }

        public async void ReactToKeyInputOnSelectedCard()
        {
            var item = SquaresCollectionView.SelectedItem as WheresCard;
            if (item != null)
            {
                await ReactToInputOnCard(item.Index);
            }
        }

        private async Task ReactToInputOnCard(int itemIndex)
        {
            Debug.WriteLine("Grid Games: Input on Square " + itemIndex);

            var vm = this.BindingContext as WheresViewModel;
            if (vm.FirstRunWheres)
            {
                return;
            }

            if (itemIndex == 15)
            {
                await ShowTipPage();

                return;
            }

            int itemCollectionIndex = GetItemCollectionIndexFromItemIndex(itemIndex);
            if (itemCollectionIndex == -1)
            {
                return;
            }

            bool answerIsCorrect;
            bool gameIsWon = vm.AttemptToAnswerQuestion(itemCollectionIndex, out answerIsCorrect);

            // Show a bonus question if appropriate.
            if (answerIsCorrect && vm.WheresSettingsVM.ShowBonusQuestion && 
                (vm.WheresSettingsVM.QuestionListCollection.Count == 15))
            {
                int questionIndex = 0;

                foreach (WheresCard card in vm.WheresListCollection)
                {
                    if (card.IsFound)
                    {
                        ++questionIndex;
                    }
                }

                await Navigation.PushModalAsync(new WCAGPage(
                    vm.WheresSettingsVM.QuestionListCollection[questionIndex - 1]));
            }

            if (gameIsWon)
            {
                await OfferToRestartGame();
            }
        }

        private async Task ShowTipPage()
        {
            var vm = this.BindingContext as WheresViewModel;
            if (!vm.FirstRunWheres)
            {
                string name = "";
                string group = "";
                string number = "";

                // Barker Todo: This is all a bit hard-coded, so clean it up.
                for (int i = 0; i < vm.WheresListCollection.Count; ++i)
                {
                    if (vm.WheresListCollection[i].WCAGName == vm.CurrentQuestionWCAG)
                    {
                        name = vm.WheresListCollection[i].WCAGName;
                        number = vm.WheresListCollection[i].WCAGNumber.Substring(0, 1);

                        if (i < 5)
                        {
                            group = "Perceivable";
                        }
                        else if (i < 11)
                        {
                            group = "Operable";
                        }
                        else
                        {
                            group = "Understandable";
                        }

                        break;
                    }
                }

                var tipPage = new WheresTipPage(name, group, number);

                await Navigation.PushModalAsync(tipPage);
            }
        }

        private async Task OfferToRestartGame()
        {
            var vm = this.BindingContext as WheresViewModel;
            if (!vm.FirstRunWheres)
            {
                var message = String.Format(
                    AppResources.ResourceManager.GetString("WonInMoves"), 
                    15 + vm.AnswerAttemptCount);

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

        private int GetItemCollectionIndexFromItemIndex(int itemIndex)
        {
            var vm = this.BindingContext as WheresViewModel;

            int itemCollectionIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (vm.WheresListCollection[i].Index == itemIndex)
                {
                    itemCollectionIndex = i;
                    break;
                }
            }

            return itemCollectionIndex;
        }

        private async void WheresGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = this.BindingContext as WheresViewModel;
            if (vm.FirstRunWheres)
            {
                return;
            }

            Debug.WriteLine("Wheres Grid Game: Selection changed. Selection count is " + e.CurrentSelection.Count);

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

        private void WheresWelcomeOKButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as WheresViewModel;
            vm.FirstRunWheres = false;
        }
    }
}
