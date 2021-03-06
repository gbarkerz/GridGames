using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GridGames.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames.Services
{
    public interface IGridGamesPlatformAction
    {
        void ScreenReaderAnnouncement(string notification);

        Task<string> GetPairsPictureFolder();
    }
}
