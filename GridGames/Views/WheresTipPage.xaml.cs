using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WheresTipPage : ContentPage
    {
        public string wcagName
        {
            get
            {
                return name;
            }
        }

        public string wcagGroupName
        {
            get
            {
                return groupName.ToLower();
            }
        }

        public string wcagGroupNumber
        {
            get
            {
                return groupNumber;
            }
        }

        private string name = "";
        private string groupName = "";
        private string groupNumber = "";

        public WheresTipPage(string wcagName, string wcagGroup, string leadingNumber)
        {
            name = wcagName;
            groupName = wcagGroup;
            groupNumber = leadingNumber;

            InitializeComponent();

            BindingContext = this;
        }

        private async void WheresTipCloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void LearnMoreButton_Clicked(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://www.w3.org/TR/WCAG21/#" + groupName.ToLower());
        }
    }
}