
namespace InvokePlatformCode.Services.PartialMethods
{
    public partial class GridGamesPlatformAction
    {
#if WINDOWS
        public partial Task<string> GetPairsPictureFolder();
#endif

#if WINDOWS
        public partial bool IsHighContrastActive(out Color highContrastBackgroundColor);
#endif
    }
}
