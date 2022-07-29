﻿using GridGames.Services;
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
    // Barker TODO: There's loads of duplicated code across the WheresPage and MatchingPage. Remove this duplication.

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WheresPage : ContentPage
    {
        private bool restartGame = true;

        public WheresPage()
        {
            InitializeComponent();

            WelcomeFrame.Loaded += WelcomeFrame_Loaded;

            SquaresCollectionView.SizeChanged += SquaresCollectionView_SizeChanged;
            SquaresCollectionView.Focused += SquaresCollectionView_Focused;

            Application.Current.RequestedThemeChanged += (s, a) =>
            {
                var currentTheme = a.RequestedTheme;
                if (currentTheme == AppTheme.Unspecified)
                {
                    currentTheme = Application.Current.PlatformAppTheme;
                }

                var vm = this.BindingContext as WheresViewModel;
                vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);
            };
        }

        private void WelcomeFrame_Loaded(object sender, EventArgs e)
        {
            if ((sender as Frame).IsVisible)
            {
                WheresSettingsButton.Focus();

                var vm = this.BindingContext as WheresViewModel;
                vm.RaiseDelayedNotificationEvent(
                    WheresWelcomeTitleLabel.Text + ", " + 
                    WheresWelcomeTitleInstructions.FormattedText);

                SquaresCollectionView.IsVisible = false;
            }
        }

        private void SquaresCollectionView_Focused(object sender, FocusEventArgs e)
        {
            // If the grid has no selected item by the time it gets focus, 
            // select the first square now. The grid must always have a 
            // selected item if it's to respond to keyboard input.
            var item = SquaresCollectionView.SelectedItem as WheresCard;
            if (item == null)
            {
                var vm = this.BindingContext as WheresViewModel;
                SquaresCollectionView.SelectedItem = vm.WheresListCollection[0];
            }
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

            vm.FirstRunWheres = Preferences.Get("FirstRunWheres", true);

            var currentTheme = Application.Current.UserAppTheme;
            if (currentTheme == AppTheme.Unspecified)
            {
                currentTheme = Application.Current.PlatformAppTheme;
            }

            vm.ShowDarkTheme = (currentTheme == AppTheme.Dark);

            if (restartGame)
            {
                restartGame = false;

                // Reset all cached game progress setting, but don't bother to shuffle.
                vm.ResetGrid(false);

                vm.SetupWheresCardList();
            }

            // Try to always set keyboard focus to the cards when the page appears.
            SquaresCollectionView.Focus();
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

        public async void ShowHelp()
        {
            var vm = this.BindingContext as WheresViewModel;
            if (!vm.FirstRunWheres)
            {
                await Navigation.PushModalAsync(new HelpPage(this));

                SquaresCollectionView.Focus();
            }
        }

        public void RestartGame()
        {
            var vm = this.BindingContext as WheresViewModel;
            if (!vm.FirstRunWheres)
            {
                vm.ResetGrid(true);
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
            if (answerIsCorrect)
            {
                if (vm.WheresSettingsVM.ShowBonusQuestion)
                {
                    int questionIndex = 0;

                    foreach (WheresCard card in vm.WheresListCollection)
                    {
                        if (card.IsFound)
                        {
                            ++questionIndex;
                        }
                    }

                    if (vm.WheresSettingsVM.QuestionListCollection.Count == 15)
                    {
                        // Barker: IMPORTANT! Reduce the time it takes to present the bonus question page.
                        AppShell.AppWCAGPage.PrepareToAskQuestion(
                            this,
                            gameIsWon,
                            vm.WheresSettingsVM.QuestionListCollection[questionIndex - 1]);
                    }
                    else
                    {
                        // Ask a default question.
                        AppShell.AppWCAGPage.PrepareToAskQuestion(
                            this,
                            gameIsWon,
                            vm.WheresSettingsVM.DefaultBonusQAList[questionIndex - 1]);
                    }

                    try
                    {
                        await Navigation.PushModalAsync(AppShell.AppWCAGPage);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("WCAGPage failed: " + ex.Message);
                    }
                }

                vm.MoveToNextQuestion();
            }
            else
            {
                await DisplayAlert(
                    AppResources.ResourceManager.GetString("Wheres"),
                    "Sorry, \"" + vm.CurrentQuestionWCAG + "\" isn't WCAG " +
                     vm.WheresListCollection[itemIndex].WCAGNumber + ".\r\n\r\nPlease do try again!",
                    AppResources.ResourceManager.GetString("OK"));
            }

            if (!vm.WheresSettingsVM.ShowBonusQuestion)
            {
                if (gameIsWon)
                {
                    await OfferToRestartGame(false);
                }
                else if (answerIsCorrect)
                {
                    var message = String.Format(
                        AppResources.ResourceManager.GetString("CorrectWCAG"),
                        vm.WheresListCollection[itemIndex].WCAGName,
                        vm.CurrentQuestionWCAG);

                    vm.RaiseNotificationEvent(message);
                }
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

        public async Task OfferToRestartGame(bool showBonusQuestionCount)
        {
            var vm = this.BindingContext as WheresViewModel;
            if (!vm.FirstRunWheres)
            {
                var message = showBonusQuestionCount ?
                    String.Format(
                        AppResources.ResourceManager.GetString("WonInMovesWithBonusQuestionCount"),
                        15 + vm.AnswerAttemptCount, vm.BonusQuestionCount) :
                    String.Format(
                        AppResources.ResourceManager.GetString("WonInMoves"),
                        15 + vm.AnswerAttemptCount);

                // Barker: Tidy this up... 
                message.Replace("1 bonus questions", "1 bonus question");

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

        private void WheresWelcomeOKButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as WheresViewModel;
            vm.FirstRunWheres = false;

            SquaresCollectionView.IsVisible = true;

            vm.RaiseNotificationEvent("Your first question is, Where's " + vm.CurrentQuestionWCAG);

            SquaresCollectionView.Focus();
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
