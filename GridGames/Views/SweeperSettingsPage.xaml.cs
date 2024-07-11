using GridGames.ViewModels;

namespace GridGames
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SweeperGameSettingsPage : ContentPage
    {
        private int previousRowCount = 4;

        public SweeperGameSettingsPage(SweeperSettingsViewModel sweeperSettingsVM)
        {
            InitializeComponent();

            // Count of rows and columns.
            for (int i = 4; i <= 8; i++)
            {
                RowColumnCountPicker.Items.Add(i.ToString());
            }

            // Count of frogs.
            for (int i = 2; i <= 16; i++)
            {
                FrogCountPicker.Items.Add(i.ToString());
            }

            this.BindingContext = sweeperSettingsVM;

            var vm = this.BindingContext as SweeperSettingsViewModel;

            vm.SideLength = (int)Preferences.Get("SideLength", 4);
            vm.FrogCount = (int)Preferences.Get("FrogCount", 2);

            previousRowCount = vm.SideLength;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var vm = this.BindingContext as BaseViewModel;
            vm.RaiseNotificationEvent(SweeperSettingsTitle.Text);
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            var sideCount = RowColumnCountPicker.SelectedIndex + 4;
            var frogCount = FrogCountPicker.SelectedIndex + 2;

            if (frogCount > (sideCount * sideCount) / 2)
            {
                await DisplayAlert(
                    "Leaf Sweeper",
                    "Please don't have frogs on more than half the stones.",
                    "OK");

                return;
            }

            await Navigation.PopModalAsync();
        }
   }
}