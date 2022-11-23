using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GridGames.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace InvokePlatformCode.Services.PartialMethods
{
    public partial class GridGamesPlatformAction
    {
#if !ANDROID
        public partial Task<string> GetPairsPictureFolder();
        public partial bool IsHighContrastActive(out Color highContrastBackgroundColor);
#endif
    }
}
