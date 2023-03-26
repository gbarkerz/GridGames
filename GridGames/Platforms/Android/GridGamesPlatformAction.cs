using Android.Content;
using GridGames;

namespace InvokePlatformCode.Services.PartialMethods;

public partial class GridGamesPlatformAction
{
#if ANDROID
    public partial async Task<string> GetPairsPictureFolder()
    {
        string result = "";

        try
        {
            MainActivity.GetPairsPictureFolderCompletion = new TaskCompletionSource<string>();

            var intent = new Intent(Intent.ActionGetContent);

            // For Android, we require the player to select all the 8 image files
            // and the description text file.
            intent.PutExtra(Intent.ExtraAllowMultiple, true);
            intent.SetType("*/*");

            Platform.CurrentActivity.StartActivityForResult(intent, 11223344);

            // Now wait until the task is set to complete in OnActivityResult();
            result = await MainActivity.GetPairsPictureFolderCompletion.Task;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Android GetPairsPictureFolder(): {0}", ex.Message);
        }

        return result;
    }
#endif
}
