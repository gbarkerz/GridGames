
namespace InvokePlatformCode.Services.PartialMethods
{
    public partial class GridGamesPlatformAction
    {
#if IOS
        public partial Task<string> GetPairsPictureFolder();
#endif

#if WINDOWS
        public partial bool IsHighContrastActive(out Color highContrastBackgroundColor);
#endif
    }
}
