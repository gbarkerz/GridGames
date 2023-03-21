using Microsoft.Maui.LifecycleEvents;
using GridGames.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui;
using GridGames.ViewModels;

namespace GridGames;

public static class MauiProgram
{
    public static DateTime timeOfMostRecentArrowKeyPress = DateTime.Now;

#if WINDOWS
    private static bool addedKeyEventHandler = false;

    static void MyKeyEventHandler(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Barker: Review old feedback from original Windows Grid Game player.
        // Check that there was a request for Enter to activate an item.
        if ((e.Key == Windows.System.VirtualKey.Space) ||
            (e.Key == Windows.System.VirtualKey.Enter))
        {
            // Are we on the grid in either the Pairs game or Where's WCAG? game?
            if (e.OriginalSource is Microsoft.UI.Xaml.Controls.GridViewItem)
            {
                var currentPage = (Application.Current.MainPage as Microsoft.Maui.Controls.Shell).CurrentPage;
                if (currentPage is MatchingPage)
                {
                    var page = currentPage as MatchingPage;
                    page.ReactToKeyInputOnSelectedCard();
                    e.Handled = true;
                }
                else if (currentPage is SquaresPage)
                {
                    var page = currentPage as SquaresPage;
                    page.ReactToKeyInputOnSelectedCard();
                    e.Handled = true;
                }
                else if (currentPage is SweeperPage)
                {
                    var page = currentPage as SweeperPage;
                    page.ReactToKeyInputOnSelectedCard();
                    e.Handled = true;
                }
                else if (currentPage is SudokuPage)
                {
                    var page = currentPage as SudokuPage;
                    page.ReactToKeyInputOnSelectedCard();
                    e.Handled = true;
                }
                else if (currentPage is WheresPage)
                {
                    var page = currentPage as WheresPage;
                    page.ReactToKeyInputOnSelectedCard();
                    e.Handled = true;
                }
            }
        }
        else if (e.Key == Windows.System.VirtualKey.F1)
        {
            var currentPage = (Application.Current.MainPage as Microsoft.Maui.Controls.Shell).CurrentPage;
            if ((currentPage is MatchingPage) || (currentPage is SquaresPage) || 
                (currentPage is SweeperPage) || (currentPage is WheresPage) ||
                (currentPage is SudokuPage))
            {
                var page = currentPage as MatchingPage;
                if (page != null)
                {
                    page.ShowHelp();
                }
                else if(currentPage is SquaresPage)
                {
                    (currentPage as SquaresPage).ShowHelp();
                }
                else if (currentPage is WheresPage)
                {
                    (currentPage as WheresPage).ShowHelp();
                }
                else if (currentPage is SweeperPage)
                {
                    (currentPage as SweeperPage).ShowHelp();
                }
                else if (currentPage is SudokuPage)
                {
                    (currentPage as SudokuPage).ShowHelp();
                }

                e.Handled = true;
            }
        }
        else if (e.Key == Windows.System.VirtualKey.F5)
        {
            var currentPage = (Application.Current.MainPage as Microsoft.Maui.Controls.Shell).CurrentPage;
            if ((currentPage is MatchingPage) || (currentPage is SquaresPage) ||
                (currentPage is WheresPage) || (currentPage is SweeperPage) ||
                (currentPage is SudokuPage))
            {
                var page = currentPage as MatchingPage;
                if (page != null)
                {
                    page.RestartGame();
                }
                else if (currentPage is SquaresPage)
                {
                    (currentPage as SquaresPage).RestartGame();
                }
                else if (currentPage is SweeperPage)
                {
                    (currentPage as SweeperPage).RestartGame(false);
                }
                else if (currentPage is SudokuPage)
                {
                    (currentPage as SudokuPage).RestartGame();
                }
                else
                {
                    (currentPage as WheresPage).RestartGame();
                }

                e.Handled = true;
            }
        }
        else if ((e.Key == Windows.System.VirtualKey.Number1) || (e.Key == Windows.System.VirtualKey.NumberPad1) ||
                 (e.Key == Windows.System.VirtualKey.Number2) || (e.Key == Windows.System.VirtualKey.NumberPad2) ||
                 (e.Key == Windows.System.VirtualKey.Number3) || (e.Key == Windows.System.VirtualKey.NumberPad3) ||
                 (e.Key == Windows.System.VirtualKey.Number4) || (e.Key == Windows.System.VirtualKey.NumberPad4) ||
                 (e.Key == Windows.System.VirtualKey.Number5) || (e.Key == Windows.System.VirtualKey.NumberPad5) ||
                 (e.Key == Windows.System.VirtualKey.Number6) || (e.Key == Windows.System.VirtualKey.NumberPad6) ||
                 (e.Key == Windows.System.VirtualKey.Number7) || (e.Key == Windows.System.VirtualKey.NumberPad7) ||
                 (e.Key == Windows.System.VirtualKey.Number8) || (e.Key == Windows.System.VirtualKey.NumberPad8) ||
                 (e.Key == Windows.System.VirtualKey.Number9) || (e.Key == Windows.System.VirtualKey.NumberPad9))
        {
            var currentPage = (Application.Current.MainPage as Microsoft.Maui.Controls.Shell).CurrentPage;
            if (currentPage is SudokuPage)
            {
                (currentPage as SudokuPage).HandleNumberInput(e.Key);

                e.Handled = true;
            }
        }
        else if (e.Key == Windows.System.VirtualKey.Application)
        {
            var currentPage = (Application.Current.MainPage as Microsoft.Maui.Controls.Shell).CurrentPage;
            if (currentPage is SweeperPage)
            {
                (currentPage as SweeperPage).ShowContextMenu();

                e.Handled = true;
            }
        }
        else if ((e.Key == Windows.System.VirtualKey.Right) ||
                 (e.Key == Windows.System.VirtualKey.Left) ||
                 (e.Key == Windows.System.VirtualKey.Up) ||
                 (e.Key == Windows.System.VirtualKey.Down))
        {
            timeOfMostRecentArrowKeyPress = DateTime.Now; 

            if ((e.Key == Windows.System.VirtualKey.Right) ||
                (e.Key == Windows.System.VirtualKey.Left))
            {
                HandleLeftRightArrowPress(e);
            }
        }
    }

    static private void HandleLeftRightArrowPress(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        var currentPage = (Application.Current.MainPage as Microsoft.Maui.Controls.Shell).CurrentPage;
        if (currentPage is SudokuPage)
        {
            // If necessary, suppress the default action resulting from a press
            // of a left or right arrow key.
            (currentPage as SudokuPage).HandleLeftRightArrow(e);
        }
    }

#endif 

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp(true)
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FA");
            });

#if WINDOWS
        builder
            .UseMauiApp<App>()
            .ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windows => windows.OnPlatformMessage((window, args) =>
                {
                    if (!addedKeyEventHandler && window.Content != null)
                    {
                        addedKeyEventHandler = true;

                        window.Content.PreviewKeyDown += MyKeyEventHandler;
                    }
                }));
            });
#endif

        return builder.Build();
    }
}
