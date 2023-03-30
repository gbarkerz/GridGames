﻿using GridGames.ResX;
using GridGames.ViewModels;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SudokuSettingsPage : ContentPage
    {
        public SudokuSettingsPage(SudokuSettingsViewModel sudokuSettingsVM)
        {
            InitializeComponent();

            for (int i = 1; i <= 16; i++)
            {
                BlankSquareCountPicker.Items.Add((i * 5).ToString());
            }

            var resMgr = AppResources.ResourceManager;

            string[] responseOptions = new string[]
            {
                resMgr.GetString("NoResponse"),
                resMgr.GetString("PlaySound"),
                resMgr.GetString("ScreenReaderAnnouncement"),
                resMgr.GetString("PlaySoundAndScreenReaderAnnouncement")
            };

            for (int i = 0; i < 4 ; i++)
            {
                SudokuResponseWhenNoMoveAvailablePicker.Items.Add(responseOptions[i]);
            }

            this.BindingContext = sudokuSettingsVM;
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
   }
}
