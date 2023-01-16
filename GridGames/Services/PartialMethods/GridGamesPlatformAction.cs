
using Microsoft.Maui.Controls;
using Microsoft.UI.Xaml;

namespace InvokePlatformCode.Services.PartialMethods
{
    public partial class GridGamesPlatformAction
    {
#if WINDOWS
        public partial void ShowFlyout(FlyoutBase contextFlyout, Border border, bool showQueryFrog);

        public partial Task<string> GetPairsPictureFolder();
#endif

#if WINDOWS
        public partial bool IsHighContrastActive(out Color highContrastBackgroundColor);
#endif
    }
}
