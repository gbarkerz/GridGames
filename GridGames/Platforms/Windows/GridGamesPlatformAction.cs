
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace InvokePlatformCode.Services.PartialMethods;

public partial class GridGamesPlatformAction
{
#if WINDOWS
    public partial void ShowFlyout(FlyoutBase contextFlyout, 
        Microsoft.Maui.Controls.Border border,
        bool showQueryFrog)
    {
        var flyout = contextFlyout.Handler.PlatformView as Microsoft.UI.Xaml.Controls.MenuFlyout;

        if (flyout != null)
        {
            var element = border.Handler.PlatformView;

            if (showQueryFrog)
            {
                flyout.Items[0].Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                flyout.Items[1].Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                flyout.Items[0].Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                flyout.Items[1].Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }

            flyout.ShowAt(element as FrameworkElement);
        }
    }

    public partial async Task<string> GetPairsPictureFolder()
    {
        string result = "";

        try
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();

            // Get the current window's HWND by passing in the Window object
            var hwnd = ((MauiWinUIWindow)Microsoft.Maui.Controls.Application.Current.Windows[0].Handler.PlatformView).WindowHandle;

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFolder pickedFolder = await picker.PickSingleFolderAsync();
            if (pickedFolder != null)
            {
                result = pickedFolder.Path + "\\PairsGamePictureDetails.txt";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetPairsPictureFolder: " + ex.Message);
        }

        return result;
    }

    // Returns the state of Windows high contrast theme.
    public partial bool IsHighContrastActive(out Color highContrastBackgroundColor)
    {
        highContrastBackgroundColor = Colors.Black;

        var accessibilitySettings = new AccessibilitySettings();
        bool highContrastIsActive = accessibilitySettings.HighContrast;
        if (highContrastIsActive)
        {
            // If the currently active high contrast theme is "High Contrast White"
            // return a background colour of white. Otherwise return color black.
            // Windows has had four high contrast themes for a long time, and despite
            // what their friendly names might be, the scheme names returned here 
            // are High Contrast Black, High Contrast White, High Contrast #1, and 
            // High Contrast #2.
            var currentScheme = accessibilitySettings.HighContrastScheme;
            if (currentScheme.ToLower().Contains("white"))
            {
                highContrastBackgroundColor = Colors.White;
            }
        }

        // If we attempt to set up an event handler for changes to high contrast theme, the attempt crashes.
        //accessibilitySettings.HighContrastChanged += AccessibilitySettings_HighContrastChanged;

        return highContrastIsActive;
    }
#endif
}
