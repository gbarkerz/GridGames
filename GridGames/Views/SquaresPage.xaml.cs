using GridGames.ResX;
using GridGames.ViewModels;
using System.Diagnostics;
using Size = System.Drawing.Size;

namespace GridGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SquaresPage : ContentPage
    {
        // Path to most recently fully loaded picture.
        private string previousLoadedPicture = "";

        public SquaresPage()
        {
            InitializeComponent();

            SquaresCollectionView.SizeChanged += SquaresCollectionView_SizeChanged;
        }

        // This gets called when switching from the Matching Game to the Squares Game,
        // and also when closing the Squares Settings page.
        protected override async void OnAppearing()
        {
            Debug.Write("Squares Game: OnAppearing called.");

            base.OnAppearing();

            Preferences.Set("InitialGame", "Squares");

            // Account for the app settings changing since the page was last shown.
            var vm = this.BindingContext as SquaresViewModel;
            vm.FirstRunSquares = Preferences.Get("FirstRunSquares", true);
            if (vm.FirstRunSquares)
            {
                // Barker: Update steps for custom announcements.
                //var service = DependencyService.Get<IMobileGridGamesPlatformAction>();
                //service.ScreenReaderAnnouncement(
                //    SquaresWelcomeTitleLabel.Text + ", " + SquaresWelcomeTitleInstructions.Text);
            }

            vm.ShowNumbers = Preferences.Get("ShowNumbers", true);
            vm.NumberHeight = Preferences.Get("NumberSizeIndex", 1);
            vm.ShowPicture = Preferences.Get("ShowPicture", false);
            vm.PicturePathSquares = Preferences.Get("PicturePathSquares", "");
            vm.PictureName = Preferences.Get("PictureName", "");

            // Barker: Remove the Hide Grid feature.
            //vm.HideGrid = Preferences.Get("HideGrid", false);

            // Has the state of the picture being shown changed since we were last changed?
            if (vm.ShowPicture && (vm.PicturePathSquares != previousLoadedPicture))
            {
                // Prevent input on the grid while the image is being loaded into the squares.

                // Barker: No loading for now.
                //vm.GameIsLoading = true;

                // Future: Without a delay here, the loading UI rarely shows up on iOS.
                // Investigate this further and remove this delay.
                await Task.Delay(200);

                // Restore the order of the squares in the grid.
                vm.RestoreEmptyGrid();

                // The loading of the images into the squares is made synchronously through the first 15 squares.

                // Barker: No image editor for now.
                //nextSquareIndexForImageSourceSetting = 0;

                // Check whether the image file exists before trying to load it into the ImageEditor.
                if (vm.IsImageFilePathValid(vm.PicturePathSquares))
                {
                    vm.RaiseNotificationEvent(PleaseWaitLabel.Text);

                    // Future: Verify that if the various event handlers are still being called from the
                    // previous attempt to load a picture, those event handlers will no longer be called
                    // once the loading of another picture begins.

                    // Barker: No image editor for now.
                    LoadCustomPictureIntoSquares(vm.PicturePathSquares, true);

                    //GridGameImageEditor.Source = ImageSource.FromFile(vm.PicturePathSquares);
                    //Debug.WriteLine("Grid Games: ImageEditor source now " + GridGameImageEditor.Source.ToString());
                }
                else
                {
                    Debug.WriteLine("Grid Games: Valid image file not found. " + vm.PicturePathSquares);

                    // We'll not attempt to load this picture again.
                    Preferences.Set("PicturePathSquares", "");
                    vm.PicturePathSquares = "";

                    vm.GameIsLoading = false;

                    if (Shell.Current != null)
                    {
                        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
                    }
                }
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (vm.FirstRunSquares || vm.GameIsLoading)
            {
                return;
            }

            var itemGrid = (Grid)sender;
            var itemAccessibleName = AutomationProperties.GetName(itemGrid);

            Debug.WriteLine("Grid Games: Tapped on Square " + itemAccessibleName);

            AttemptToMoveSquareByName(itemAccessibleName);
        }

        private async void AttemptToMoveSquareByName(string itemName)
        {
            var vm = this.BindingContext as SquaresViewModel;

            int itemIndex = GetItemCollectionIndexFromItemAccessibleName(itemName);
            if (itemIndex != -1)
            {
                bool gameIsWon = vm.AttemptToMoveSquare(itemIndex);
                if (gameIsWon)
                {
                    await OfferToRestartGame();
                }
            }
        }

        private int GetItemCollectionIndexFromItemAccessibleName(string ItemAccessibleName)
        {
            var vm = this.BindingContext as SquaresViewModel;

            int itemIndex = -1;
            for (int i = 0; i < 16; ++i)
            {
                if (vm.SquareListCollection[i].AccessibleName == ItemAccessibleName)
                {
                    itemIndex = i;
                    break;
                }
            }

            return itemIndex;
        }

        private async Task OfferToRestartGame()
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (!vm.FirstRunSquares)
            {
                var message = "";

                if (String.IsNullOrWhiteSpace(vm.PictureName))
                {
                    message = String.Format(
                        AppResources.ResourceManager.GetString("SquaresWonInGoes"), 8 + vm.MoveCount);
                }
                else
                {
                    message = String.Format(
                        AppResources.ResourceManager.GetString("CompletedSquaresPictureInMoves"),
                            vm.PictureName,
                            8 + vm.MoveCount);
                }

                var answer = await DisplayAlert(
                    AppResources.ResourceManager.GetString("Congratulations"),
                    message,
                    AppResources.ResourceManager.GetString("Yes"),
                    AppResources.ResourceManager.GetString("No"));
                if (answer)
                {
                    vm.ResetGrid();
                }
            }
        }

        // SelectionChanged handling only exists today in the app to support Android Switch Access. 
        // At some point SelectionChanged may also be a part of keyboard support, but currently the
        // rest of the app is not keyboard accessible, and focus feedback is unusable on the items.

        private async void SquaresGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Squares Grid Game: Selection changed. Selection count is " + e.CurrentSelection.Count);

            // Do nothing here if pictures have not been loaded yet onto the squares.
            var vm = this.BindingContext as SquaresViewModel;
            if (vm.FirstRunSquares || vm.GameIsLoading)
            {
                return;
            }

            // No action required here if there is no selected item.
            if (e.CurrentSelection.Count > 0)
            {
                bool gameIsWon = vm.AttemptMoveBySelection(e.CurrentSelection[0]);

                // Clear the selection now to support the same square moving again.
                SquaresCollectionView.SelectedItem = null;

                if (gameIsWon)
                {
                    await OfferToRestartGame();
                }
            }
        }

        private void SquaresWelcomeOKButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;
            vm.FirstRunSquares = false;
        }

        private async void SquaresGameSettingsButton_Clicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (!vm.FirstRunSquares)
            {
                var settingsPage = new SquaresSettingsPage();
                await Navigation.PushModalAsync(settingsPage);
            }
        }

        public async void ReactToKeyInputOnSelectedCard()
        {
            var item = SquaresCollectionView.SelectedItem as SquaresViewModel.Square;
            if (item != null)
            {
                await ReactToInputOnCard(item);
            }
        }

        private async Task ReactToInputOnCard(SquaresViewModel.Square item)
        {
            var vm = this.BindingContext as SquaresViewModel;
            if (vm.FirstRunSquares || vm.GameIsLoading)
            {
                return;
            }

            int itemIndex = GetItemCollectionIndexFromItemAccessibleName(item.AccessibleName);
            if (itemIndex != -1)
            {
                bool gameIsWon = vm.AttemptToMoveSquare(itemIndex);
                if (gameIsWon)
                {
                    await OfferToRestartGame();
                }
            }
        }

        private void SquaresCollectionView_SizeChanged(object sender, EventArgs e)
        {
            var vm = this.BindingContext as SquaresViewModel;

            if (vm.ShowPicture && !String.IsNullOrWhiteSpace(vm.PicturePathSquares))
            {
                LoadCustomPictureIntoSquares(vm.PicturePathSquares, false);
            }
        }

        double xImageScale;
        double yImageScale;

        // Copied with thanks from:
        // https://stackoverflow.com/questions/552467/how-do-i-reliably-get-an-image-dimensions-in-net-without-loading-the-image

        public static Size GetJpegImageSize(string filename)
        {
            FileStream stream = null;
            BinaryReader rdr = null;
            try
            {
                stream = File.OpenRead(filename);
                rdr = new BinaryReader(stream);
                // keep reading packets until we find one that contains Size info
                for (; ; )
                {
                    byte code = rdr.ReadByte();
                    if (code != 0xFF) throw new ApplicationException(
                            "Unexpected value in file " + filename);
                    code = rdr.ReadByte();
                    switch (code)
                    {
                        // filler byte
                        case 0xFF:
                            stream.Position--;
                            break;
                        // packets without data
                        case 0xD0:
                        case 0xD1:
                        case 0xD2:
                        case 0xD3:
                        case 0xD4:
                        case 0xD5:
                        case 0xD6:
                        case 0xD7:
                        case 0xD8:
                        case 0xD9:
                            break;
                        // packets with size information
                        case 0xC0:
                        case 0xC1:
                        case 0xC2:
                        case 0xC3:
                        case 0xC4:
                        case 0xC5:
                        case 0xC6:
                        case 0xC7:
                        case 0xC8:
                        case 0xC9:
                        case 0xCA:
                        case 0xCB:
                        case 0xCC:
                        case 0xCD:
                        case 0xCE:
                        case 0xCF:
                            ReadBEUshort(rdr);
                            rdr.ReadByte();
                            ushort h = ReadBEUshort(rdr);
                            ushort w = ReadBEUshort(rdr);
                            return new Size(w, h);
                        // irrelevant variable-length packets
                        default:
                            int len = ReadBEUshort(rdr);
                            stream.Position += len - 2;
                            break;
                    }
                }
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (stream != null) stream.Close();
            }
        }

        private static ushort ReadBEUshort(BinaryReader rdr)
        {
            ushort hi = rdr.ReadByte();
            hi <<= 8;
            ushort lo = rdr.ReadByte();
            return (ushort)(hi | lo);
        }

        double originalLoadedImageWidth;
        double originalLoadedImageHeight;

        private void LoadCustomPictureIntoSquares(string picturePathSquares, bool setSources)
        {
            var vm = this.BindingContext as SquaresViewModel;

            var originalLoadedImageSize = GetJpegImageSize(picturePathSquares);

            originalLoadedImageWidth = originalLoadedImageSize.Width;
            originalLoadedImageHeight = originalLoadedImageSize.Height;

            if (setSources)
            {
                for (int i = 0; i < 16; ++i)
                {
                    vm.SquareListCollection[i].PictureImageSource = ImageSource.FromFile(picturePathSquares);
                }
            }

            xImageScale = SquaresCollectionView.Width / originalLoadedImageWidth;
            yImageScale = SquaresCollectionView.Height / originalLoadedImageHeight;

            vm.ResetGrid();

            timer = new Timer(new TimerCallback((s) => NowTranslate()),
                               null,
                               TimeSpan.FromMilliseconds(100),
                               TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        // Worth noting details at
        // https://learn.microsoft.com/en-us/dotnet/maui/user-interface/graphics/transforms
        // For example, when to apply the transform relative to when the ImageSource was set.

        private Timer timer;

        private void NowTranslate()
        {
            var vm = this.BindingContext as SquaresViewModel;

            if (timer != null)
            {
                timer.Dispose();
            }

            var newThread = new System.Threading.Thread(() =>
            {
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    var imageCount = 0;

                    var descendants = SquaresCollectionView.GetVisualTreeDescendants();
                    for (int i = 0; i < descendants.Count; i++)
                    {
                        if (descendants[i] is Image)
                        {
                            var image = descendants[i] as Image;

                            image.WidthRequest = originalLoadedImageWidth;
                            image.HeightRequest = originalLoadedImageHeight;

                            image.ScaleX = xImageScale;
                            image.ScaleY = yImageScale;

                            var squareWidth = (SquaresCollectionView.Width / 4);
                            var squareHeight = (SquaresCollectionView.Height / 4);

                            var squareIndex = vm.SquareListCollection[imageCount].TargetIndex;

                            double xIndex = (squareIndex % 4);
                            double yIndex = (squareIndex / 4);

                            image.TranslationX = (1.5 - xIndex) * squareWidth;
                            image.TranslationY = (1.5 - yIndex) * squareHeight;

                            ++imageCount;
                        }
                    }

                    Debug.WriteLine("Number of images: " + imageCount);
                });
            });
     
            newThread.Start();
        }
    }
}
