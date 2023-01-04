using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Provider;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace GridGames;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public static TaskCompletionSource<string> GetPairsPictureFolderCompletion;

    protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        string result = "";

        base.OnActivityResult(requestCode, resultCode, data);

        try
        {
            // Has the player successfully made some selection when picking custom files for the Pairs game?
            if (((data != null)) && (requestCode == 1234) && (resultCode == Result.Ok))
            {
                // Check that the nine files were selected, (that is, 8 images and the description text file).
                if ((data.ClipData != null) && (data.ClipData.ItemCount == 9))
                {
                    // Copy all the files of interest to a dedicated folder beneath the app's temp folder.
                    var destinationFolder = Path.Combine(Path.GetTempPath(), "PairsGameCurrentPictures");

                    // First delete any temporary folder we may have created earlier.
                    if (Directory.Exists(destinationFolder))
                    {
                        Directory.Delete(destinationFolder, true);
                    }

                    // Now create a new temporary folder to use.
                    Directory.CreateDirectory(destinationFolder);

                    // Process each file in the source folder in turn.
                    for (int i = 0; i < 9; i++)
                    {
                        var file = data.ClipData.GetItemAt(i);

                        // Take the following action to find the filename for each source file.
                        Android.Net.Uri sourceUri = file.Uri;
                        ICursor returnCursor = ContentResolver.Query(sourceUri, null, null, null, null);
                        int nameIndex = returnCursor.GetColumnIndex(IOpenableColumns.DisplayName);
                        returnCursor.MoveToFirst();
                        var fileName = returnCursor.GetString(nameIndex);

                        await CopyFile(sourceUri, destinationFolder, fileName);

                        if (fileName == "PairsGamePictureDetails.txt")
                        {
                            result = Path.Combine(destinationFolder, fileName); ;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Android OnActivityResult(): {0}", ex.Message);
        }

        GetPairsPictureFolderCompletion.SetResult(result);

        return;
    }

    public async Task CopyFile(Android.Net.Uri sourceUri, string destinationFolder, string fileName)
    {
        var destinationFilePath = Path.Combine(destinationFolder, fileName);

        try
        {
            var sourceStream = this.ContentResolver.OpenInputStream(sourceUri);

            using (var writer = File.Create(destinationFilePath))
            {
                var buffer = new byte[0x4000];

                int b = buffer.Length;
                int length;

                while ((length = await sourceStream.ReadAsync(buffer, 0, b)) > 0)
                {
                    await writer.WriteAsync(buffer, 0, length);
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Android CopyFile(): {0}", ex.Message);
        }
    }
}
