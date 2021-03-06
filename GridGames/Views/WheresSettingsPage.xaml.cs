using GridGames.ViewModels;
using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WheresGameSettingsPage : ContentPage
    {
        public WheresGameSettingsPage(WheresSettingsViewModel wheresSettingsVM)
        {
            InitializeComponent();

            //this.BindingContext = new WheresSettingsViewModel();
            this.BindingContext = wheresSettingsVM;
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private void QuestionsClearButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as WheresSettingsViewModel;

            vm.BonusQuestionFile = "";
            vm.QuestionListCollection.Clear();
        }

        private async void QuestionsBrowseButton_Clicked(object sender, EventArgs e)
        {
            var options = new PickOptions
            {
                PickerTitle = "Please select a questions file."
            };

            var result = await FilePicker.PickAsync(options);
            if (result != null)
            {
                var vm = this.BindingContext as WheresSettingsViewModel;
                vm.LoadBonusQuestions(result.FullPath);
            }
        }
    }

    public class QAPair
    {
        private string question;
        public string Question
        {
            get { return question; }
            set { question = value; }
        }

        private Collection<string> answers = new Collection<string>();
        public Collection<string> Answers
        {
            get { return answers; }
            set { answers = value; }
        }
    }
}