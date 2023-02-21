
namespace InvokePlatformCode.Services.PartialMethods
{
    public partial class GridGamesPlatformAction
    {
#if WINDOWS

        public partial void SetGridItemCollectionViewAccessibleData(CollectionView collectionView, int itemIndex, int row, int column);
        public partial void SetGridCollectionViewAccessibleData(CollectionView collectionView, bool includeGroupData);

        public partial Task<string> GetPairsPictureFolder();

        public partial void ShowFlyout(FlyoutBase contextFlyout,
            Border border);
#endif

#if WINDOWS
        public partial bool IsHighContrastActive(out Color highContrastBackgroundColor);
#endif
    }
}
