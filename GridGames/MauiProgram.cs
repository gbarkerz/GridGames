using Microsoft.Maui.LifecycleEvents;
using GridGames.Views;

namespace GridGames;

public static class MauiProgram
{
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
                if ((currentPage is MatchingPage) || (currentPage is WheresPage))
                {
                    var page = currentPage as MatchingPage;
                    if (page != null)
                    {
                        page.ReactToKeyInputOnSelectedCard();
                    }
                    else
                    {
                        (currentPage as WheresPage).ReactToKeyInputOnSelectedCard();
                    }

                    e.Handled = true;
                }
            }
        }
    }
#endif

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
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
