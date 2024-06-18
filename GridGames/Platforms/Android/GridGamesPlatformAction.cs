using System.Diagnostics;

namespace InvokePlatformCode.Services.PartialMethods;

public partial class GridGamesPlatformAction
{
#if ANDROID
    public partial async Task<Tuple<string, string>> GetPairsPictureFolder()
    {
        Tuple<string, string> result = new Tuple<string, string>("", "");
        
        try
        {
            PickOptions options = new()
            {
                PickerTitle = "Please select 8 pictures files and the text file with the picture names.",
            };

            var pickerResult = await FilePicker.PickMultipleAsync(options);
            if (pickerResult.Count() == 9)
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

                // The selected folder must contain exactly the required number of pictures in it.
                bool picturePathValid = true;

                string selectedFolder = "";

                foreach (var file in pickerResult)
                {
                    if (selectedFolder == "")
                    {
                        selectedFolder = file.FullPath;
                    }

                    var pictureFilename = file.FileName;

                    var extension = Path.GetExtension(pictureFilename);

                    // Is this a file of interest?
                    if (pictureFilename == "PairsGamePictureDetails.txt" ||
                        extension == ".jpg" || extension == ".png" || extension == ".bmp" || extension == ".jpeg")
                    {
                        var pictureFileDestination = Path.Combine(destinationFolder, pictureFilename);

                        File.Copy(file.FullPath, pictureFileDestination);
                    }
                    else
                    {
                        picturePathValid = false;

                        break;
                    }
                }

                if (picturePathValid)
                {
                    result = new Tuple<string, string>(
                                Path.Combine(destinationFolder, "PairsGamePictureDetails.txt"),
                                selectedFolder);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetPairsPictureFolder: " + ex.Message);
        }

        return result;
    }
#endif
}
