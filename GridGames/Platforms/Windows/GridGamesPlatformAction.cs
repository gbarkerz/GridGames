using GridGames.ResX;
using Microsoft.Maui.Controls;
using Microsoft.UI.Composition.Interactions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.ViewManagement;
using AutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace InvokePlatformCode.Services.PartialMethods;

public partial class GridGamesPlatformAction
{
#if WINDOWS

    // Set any platform-specific accessibility properties on the grid and its items.
    public partial void SetGridCollectionViewAccessibleData(CollectionView collectionView)
    {
        try
        {
            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var handler = collectionView.Handler;
                if (handler != null)
                {
                    var grid = collectionView.Handler.PlatformView as GridView;

                    var resManager = AppResources.ResourceManager;

                    // First set the properties on the grid itself.
                    AutomationProperties.SetAutomationControlType(grid, AutomationControlType.Custom);
                    AutomationProperties.SetLocalizedControlType(grid, resManager.GetString("GridLocalizedControlType"));

                    // Now set the properties on the items in the grid.
                    var cellLocalizedControlType = resManager.GetString("CellLocalizedControlType");

                    // Assume the grid is square.
                    var countItemsTotal = grid.Items.Count;
                    var countItemsInRow = (int)Math.Sqrt(countItemsTotal);

                    var rowString = resManager.GetString("Row");
                    var columnString = resManager.GetString("Column");

                    for (int i = 0; i < countItemsTotal; ++i)
                    {
                        var item = grid.Items[i];
                        if (item != null)
                        {
                            var container = grid.ContainerFromItem(item) as UIElement;
                            if (container != null)
                            {
                                AutomationProperties.SetAutomationControlType(container, AutomationControlType.Custom);
                                AutomationProperties.SetLocalizedControlType(container, cellLocalizedControlType);

                                int rowIndex = (i / countItemsInRow);
                                int columnIndex = (i % countItemsInRow);

                                int groupIndex = (3 * (int)(rowIndex / 3)) + (columnIndex / 3);

                                var helpText = "Group " + (groupIndex + 1).ToString() + ", " + 
                                    rowString + " " + (rowIndex + 1) + " " +
                                        columnString + " " + (columnIndex + 1);

                                // Assume it's ok from a localization perspective to have a fixed order for the elements of this HelpText.
                                AutomationProperties.SetHelpText(container, helpText);
                            }
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SetGridCollectionViewAccessibleData: " + ex.Message);
        }
    }

    public partial void SetGridItemCollectionViewAccessibleData(CollectionView collectionView, int itemIndex, int row, int column)
    {
        // Always run this on the UI thread.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var resManager = AppResources.ResourceManager;

            var grid = collectionView.Handler.PlatformView as GridView;

            var item = grid.Items[itemIndex];
            if (item != null)
            {
                var container = grid.ContainerFromItem(item) as UIElement;
                if (container != null)
                {
                    var cellLocalizedControlType = resManager.GetString("CellLocalizedControlType");

                    AutomationProperties.SetAutomationControlType(container, AutomationControlType.Custom);
                    AutomationProperties.SetLocalizedControlType(container, cellLocalizedControlType);

                    // Assume it's ok from a localization perspective to have a fixed order for the elements of this HelpText.
                    AutomationProperties.SetHelpText(container,
                        resManager.GetString("Row") + " " + row + " " +
                        resManager.GetString("Column") + " " + column);
                }
            }
        });
    }

    public partial void ShowFlyout(FlyoutBase contextFlyout,
        Microsoft.Maui.Controls.Border border)
    {
        var flyout = contextFlyout.Handler.PlatformView as Microsoft.UI.Xaml.Controls.MenuFlyout;

        if (flyout != null)
        {
            var element = border.Handler.PlatformView;

            flyout.ShowAt(element as FrameworkElement);
        }
    }

    public partial async Task<string> GetPairsPictureFolder()
    {
        string result = "";

        try
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();

            // Get the current window's HWND by passing in the Window object
            var hwnd = ((MauiWinUIWindow)Microsoft.Maui.Controls.Application.Current.Windows[0].Handler.PlatformView).WindowHandle;

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFolder pickedFolder = await picker.PickSingleFolderAsync();
            if (pickedFolder != null)
            {
                result = pickedFolder.Path + "\\PairsGamePictureDetails.txt";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetPairsPictureFolder: " + ex.Message);
        }

        return result;
    }

    // Returns the state of Windows high contrast theme.
    public partial bool IsHighContrastActive(out Color highContrastBackgroundColor)
    {
        highContrastBackgroundColor = Colors.Black;

        var accessibilitySettings = new AccessibilitySettings();
        bool highContrastIsActive = accessibilitySettings.HighContrast;
        if (highContrastIsActive)
        {
            // If the currently active high contrast theme is "High Contrast White"
            // return a background colour of white. Otherwise return color black.
            // Windows has had four high contrast themes for a long time, and despite
            // what their friendly names might be, the scheme names returned here 
            // are High Contrast Black, High Contrast White, High Contrast #1, and 
            // High Contrast #2.
            var currentScheme = accessibilitySettings.HighContrastScheme;
            if (currentScheme.ToLower().Contains("white"))
            {
                highContrastBackgroundColor = Colors.White;
            }
        }

        // If we attempt to set up an event handler for changes to high contrast theme, the attempt crashes.
        //accessibilitySettings.HighContrastChanged += AccessibilitySettings_HighContrastChanged;

        return highContrastIsActive;
    }
#endif
}
