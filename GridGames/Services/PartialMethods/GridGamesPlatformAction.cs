
namespace InvokePlatformCode.Services.PartialMethods
{
    public partial class GridGamesPlatformAction
    {
#if WINDOWS
        public partial Task<string> GetPairsPictureFolder();

        public partial void ShowFlyout(FlyoutBase contextFlyout,
            Border border);
#endif

#if WINDOWS
        public partial bool IsHighContrastActive(out Color highContrastBackgroundColor);
#endif
    }
}
