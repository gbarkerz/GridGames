
namespace InvokePlatformCode.Services.PartialMethods
{
    public partial class GridGamesPlatformAction
    {
#if WINDOWS

        public partial void PrepareGamepadUsage();

        public partial void SetGridItemCollectionViewAccessibleData(CollectionView collectionView, int itemIndex, int row, int column);
        public partial void SetGridCollectionViewAccessibleData(CollectionView collectionView, bool includeGroupData, string dataFormat);

        public partial void ShowFlyout(FlyoutBase contextFlyout,
            Border border);
#endif

    public partial Task<string> GetPairsPictureFolder();

#if WINDOWS
        public partial bool IsHighContrastActive(out Color highContrastBackgroundColor);
#endif
    }
}
