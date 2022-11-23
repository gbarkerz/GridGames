
using Windows.UI.ViewManagement;

namespace InvokePlatformCode.Services.PartialMethods;

public partial class GridGamesPlatformAction
{
    public partial Task<string> GetPairsPictureFolder()
    {
        return null;
    }

    // Returns the state of Windows high contrast theme.
    public partial bool IsHighContrastActive(out Color highContrastBackgroundColor)
    {
        highContrastBackgroundColor = Colors.Black;

        var accessibilitySettings = new AccessibilitySettings();
        bool highContrastIsActive = accessibilitySettings.HighContrast;
        if (highContrastIsActive)
        {
            // If the currently active high contrast gheme is "High Contrast White"
            // return a background colour of white. Otherwise return color black.
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
}
