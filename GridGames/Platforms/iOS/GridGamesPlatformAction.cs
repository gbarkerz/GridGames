
using Foundation;
using MobileCoreServices;
using UIKit;

namespace InvokePlatformCode.Services.PartialMethods;

public partial class GridGamesPlatformAction
{
    public partial Task<string> GetPairsPictureFolder()
    {
        string result = "";

        var tcs = new TaskCompletionSource<string>();

        try
        {
            // Ask the player to select a folder.
            var docPicker = new UIDocumentPickerViewController(
                new string[] { UTType.Folder }, UIDocumentPickerMode.Open);

            var currentViewController = GetCurrentUIController();
            if (currentViewController != null)
            {
                currentViewController.PresentViewController(docPicker, true, null);
            }

            docPicker.WasCancelled += (sender, wasCancelledArgs) =>
            {
                tcs.SetResult("");
            };

            docPicker.DidPickDocumentAtUrls += (object sender, UIDocumentPickedAtUrlsEventArgs e) =>
            {
                Console.WriteLine("url = {0}", e.Urls[0].AbsoluteString);

                // Wrap all file/folder access here in Start/StopAccessingSecurityScopedResource.
                var start = e.Urls[0].StartAccessingSecurityScopedResource();

                // Copy all the files of interest to a dedicated folder beneath the app's temp folder.
                var targetFolder = Path.Combine(Path.GetTempPath(), "PairsGameCurrentPictures");

                // First delete any temporary folder we may have created earlier.
                if (Directory.Exists(targetFolder))
                {
                    Directory.Delete(targetFolder, true);
                }

                // Now create a new temporary folder to use.
                targetFolder = Path.Combine(targetFolder, e.Urls[0].LastPathComponent);
                Directory.CreateDirectory(targetFolder);

                // Now enumerate the folder selected by the player.
                var filePathUrl = e.Urls[0].FilePathUrl;

                NSError err;
                var selectedContent = NSFileManager.DefaultManager.GetDirectoryContent(
                    e.Urls[0],
                    null,
                    NSDirectoryEnumerationOptions.SkipsHiddenFiles,
                    out err);

                int imageCount = 0;
                for (int i = 0; i < selectedContent.Count(); ++i)
                {
                    Console.WriteLine("File = {0}", selectedContent[i].AbsoluteString);

                    var filename = selectedContent[i].LastPathComponent;
                    var targetFilename = Path.Combine(targetFolder, filename);
                    var targetFilenameUrl = NSUrl.FromFilename(targetFilename);

                    var fileManager = new NSFileManager();
                    fileManager.Copy(selectedContent[i], targetFilenameUrl, out err);

                    var item = new GridGames.ViewModels.PictureData();
                    item.Index = imageCount + 1;
                    item.FullPath = targetFilename;
                    item.FileName = filename;

                    ++imageCount;

                    if (selectedContent[i].PathExtension == "txt")
                    {
                        result = targetFilename;
                    }
                }

                e.Urls[0].StopAccessingSecurityScopedResource();

                tcs.SetResult(result);
            };

        }
        catch (Exception ex)
        {
            Console.WriteLine("url = {0}", ex.Message);

            tcs.SetResult("");
        }

        return tcs.Task;
    }

    // Make sure the app is ready to present the iOS folder picker.
    public UIViewController GetCurrentUIController()
    {
        var window = UIApplication.SharedApplication.KeyWindow;
        if (window == null)
        {
            return null;
        }

        // This code ported from the Xamarin app to the Maui doesn't compile,
        // so for now remove it. Things still seems to work.
        /*
        if (window.RootViewController.PresentedViewController == null)
        {
            window = UIApplication.SharedApplication.Windows
                     .First(i => i.RootViewController != null &&
                                 i.RootViewController.GetType().FullName
                                 .Contains(typeof(Xamarin.Forms.Platform.iOS.Platform).FullName));
        }*/

        UIViewController viewController = window.RootViewController;

        while (viewController.PresentedViewController != null)
        {
            viewController = viewController.PresentedViewController;
        }

        return viewController;
    }

    public partial bool IsHighContrastActive(out Color highContrastBackgroundColor)
    {
        highContrastBackgroundColor = Colors.Black;

        return false;
    }
}
