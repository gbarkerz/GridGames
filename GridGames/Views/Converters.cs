using GridGames.ResX;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.UI.Composition.Interactions;

namespace GridGames.Views
{
    public class SweeperItemGameOverToAccessibleName : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 4))
            {
                return "";
            }

            if ((values[0] == null) || (values[1] == null) ||
                (values[2] == null) || (values[3] == null))
            {
                return "";
            }

            var accessibleName = (string)values[0];
            var hasFrog = (bool)values[1];
            var gameWon = (bool)values[2];
            var gameLost = (bool)values[3];

            var fullAccessibleName = "";

            if (hasFrog && (gameWon || gameLost))
            {
                fullAccessibleName = (gameWon ? "Peaceful resting frog" : "Disturbed frog");
            }
            else
            {
                fullAccessibleName = accessibleName;
            }

            return fullAccessibleName;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class NearbyFrogCountToLabel : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 4))
            {
                return "";
            }

            if ((values[0] == null) || (values[1] == null) ||
                (values[2] == null) || (values[3] == null))
            {
                return "";
            }

            bool turnedUp = (bool)values[0];
            int nearbyFrogCount = (int)values[1];
            bool showsQueryFrog = (bool)values[2];
            bool hasFrog = (bool)values[3];

            if (!turnedUp || hasFrog || showsQueryFrog || (nearbyFrogCount <= 0))
            {
                return "";
            }

            return nearbyFrogCount.ToString();
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TurnedUpToBackgroundColour : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 6))
            {
                return "";
            }

            if ((values[0] == null) || (values[1] == null) || (values[2] == null) ||
                (values[3] == null) || (values[4] == null) || (values[5] == null))
            {
                return "";
            }

            var turnedUp = (bool)values[0];
            var hasFrog = (bool)values[1];
            var showsQueryFrog = (bool)values[2];
            bool gameWon = (bool)values[3];
            bool gameLost = (bool)values[4];
            bool showDarkTheme = (bool)values[5];

            Color veryDarkGrey = Color.FromArgb("FF404040");
            Color veryDarkRed = Color.FromArgb("FF400000");
            Color veryDarkGreen = Color.FromArgb("FF004000");

            Color col = showDarkTheme ? veryDarkGreen : Colors.LightGreen;

            if (showsQueryFrog || (gameWon && hasFrog))
            {
                col = showDarkTheme ? veryDarkGreen : Colors.LightGreen;
            }
            else if (gameLost && hasFrog)
            {
                col = showDarkTheme ? veryDarkRed : Colors.Pink;
            }
            else if (turnedUp)
            {
                col = showDarkTheme ? veryDarkGrey : Colors.LightGray;
            }

            return col;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LeafToVisible : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 4))
            {
                return false;
            }

            if ((values[0] == null) || (values[1] == null) ||
                (values[2] == null) || (values[3] == null))
            {
                return false;
            }

            var turnedUp = (bool)values[0];
            var hasFrog = (bool)values[1];
            var showsQueryFrog = (bool)values[2];
            var gameOver = (bool)values[3];

            return (!turnedUp || showsQueryFrog || (hasFrog && gameOver));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LeafToLabel : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 5))
            {
                return "";
            }

            if ((values[0] == null) || (values[1] == null) ||
                (values[2] == null) || (values[3] == null) ||
                (values[4] == null))
            {
                return "";
            }

            var turnedUp = (bool)values[0];
            var hasFrog = (bool)values[1];
            var showsQueryFrog = (bool)values[2];
            var gameWon = (bool)values[3];
            var gameLost = (bool)values[4];

            string text = "\uf06C";

            if (showsQueryFrog)
            {
                text = "\uf52e" + "?";
            }
            else if (gameWon && hasFrog)
            {
                text = "\uf52e" + " " + "\uf118";
            }
            else if (gameLost && hasFrog)
            {
                text = "\uf52e" + " " + "\uf119";
            }

            return text;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomPictureToItemName : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 3))
            {
                return "";
            }

            if ((values[0] == null) || (values[1] == null) || (values[2] == null))
            {
                return "";
            }
            int index = (int)values[0];
            string fileName = (string)values[1];
            string accessibleName = (string)values[2];

            return index + ", " + fileName + ", " + accessibleName;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QAToItemName : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 2))
            {
                return "";
            }

            string question = (string)values[0];
            string answerSet = (string)values[1];

            return question + ", " + answerSet;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DarkThemeToSquareLabelColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var showDarkTheme = (bool)value;

            var color = Colors.Black;

            // Is this the text colour?
            if ((string)parameter == "0")
            {
                color = (showDarkTheme ? Colors.White : Colors.Black);
            }
            else
            {
                color = (showDarkTheme ? Colors.Black : Colors.White);
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QuestionToQuestionString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var question = value as string;

            return "Q: " + question;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AnswerSetToAnswersString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var answers = value as Collection<string>;

            string fullAnswers = "A: ";

            for (int i = 0; i < answers.Count; ++i)
            {
                fullAnswers += answers[i];

                if (i < answers.Count - 1)
                {
                    fullAnswers += ", ";
                }
            }

            return fullAnswers;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FilePathToIsVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var filePath = (string)value;

            return !String.IsNullOrWhiteSpace(filePath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SideLengthToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sideLength = (int)value;

            return sideLength - 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;

            return intValue + 4;
        }
    }

    public class FrogCountToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var frogCount = (int)value;

            return frogCount - 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;

            return intValue + 2;
        }
    }

    public class PictureAspectToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var aspect = (Aspect)value;

            return (int)aspect;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;

            return (Aspect)intValue;
        }
    }

    public class SweeperLabelContainerHeightToFontSize : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var containerHeightPixels = (double)value;

            int scaler = int.Parse(parameter as string);

            //return (containerHeightPixels * 0.25) / scaler;
            return 0.67 * (containerHeightPixels * 0.25) / scaler;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;

            return (Aspect)intValue;
        }
    }

    public class WheresLabelContainerHeightToFontSize : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var containerHeightPixels = (double)value;

            // Future: Properly account for line height etc. For now, just shrink the value.
            // Also this reduces the size to account for tall cells in portrait orientation.

            // Note: The container here is the main CollectionView, and we have 4 rows.
            return (containerHeightPixels * 0.3) * 0.25;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;

            return (Aspect)intValue;
        }
    }

    public class CardFaceUpToImageWidth : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length < 2) || (values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var faceUp = (bool)values[0];
            var containerWidthPixels = (double)values[1];

            return faceUp ? containerWidthPixels : 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WheresAnsweredToTextColor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 2))
            {
                return "";
            }

            if ((values[0] == null) || (values[1] == null))
            {
                return "";
            }

            bool isFound = (bool)values[0];
            bool showDarkTheme = (bool)values[1];

            var colorName = "WheresTextColor";

            if (showDarkTheme)
            {
                colorName = isFound ? "WheresAnsweredTextColorDark" : "WheresTextColorDark";
            }
            else
            {
                if (isFound)
                {
                    colorName = "WheresAnsweredTextColor";
                }
            }

            return App.Current.Resources[colorName];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WheresAnsweredToBackgroundColor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 2))
            {
                return "";
            }

            if ((values[0] == null) || (values[1] == null))
            {
                return "";
            }

            bool isFound = (bool)values[0];
            bool showDarkTheme = (bool)values[1];

            var colorName = "WheresBackgroundColor";

            if (showDarkTheme)
            {
                colorName = isFound ? "WheresAnsweredBackgroundColorDark" : "WheresBackgroundColorDark";
            }
            else
            {
                if (isFound)
                {
                    colorName = "WheresAnsweredBackgroundColor";
                }
            }

            return App.Current.Resources[colorName];

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FirstRunToGridOpacity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var firstRun = (bool)value;

            return firstRun ? 0.0 : 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WCAGTitleToQuestion : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "Where's \"" + (string)value + "\"?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FirstRunLoadingSquaresToGridOpacity : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length < 2) || (values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var firstRunSquares = (bool)values[0];
            var gameIsLoading = (bool)values[1];

            double opacity = 1.0;

            if (firstRunSquares)
            {
                opacity = 0.0;
            }
            else if (gameIsLoading)
            {
                opacity = 0.0;
            }

            return opacity;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SweeperCollectionViewHeightToRowHeight : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length < 2) || (values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var gridHeight = (double)values[0];
            var sideLength = (int)values[1];

            if ((gridHeight <= 0) || (sideLength == 0))
            {
                return 0;
            }

            return ((double)gridHeight / sideLength) - 2;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionViewHeightToRowHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value / 4) - 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LabelContainerHeightToFontSize : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length < 2) || 
                (values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var collectionViewHeight = (double)values[0];
            var numberSizeIndex = (int)values[1];

            if (collectionViewHeight <= 0)
            {
                return 1;
            }

            double gridRowHeightMultiplier = 0.3;

            switch (numberSizeIndex)
            {
                case 0:
                    gridRowHeightMultiplier = 0.2;
                    break;
                case 2:
#if IOS
                    gridRowHeightMultiplier = 0.3;
#else
                    gridRowHeightMultiplier = 0.4;
#endif
                    break;
                case 3:
#if IOS
                    gridRowHeightMultiplier = 0.4;
#else
                    gridRowHeightMultiplier = 0.5;
#endif
                    break;
                default:
                    break;
            }

            Debug.WriteLine("Squares label data: gridHeight " + collectionViewHeight +
                ", gridRowHeightMultiplier " + gridRowHeightMultiplier);

            // Future: Properly account for line height etc. For now, just shrink the value.
            // Also this reduces the size to account for tall cells in portrait orientation.
            return collectionViewHeight * 0.25 * gridRowHeightMultiplier;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SettingsPicturePathToPicturePathLabelIsVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var picturePath = (string)value;

            bool picturePathLabelIsVisible = !String.IsNullOrWhiteSpace(picturePath);

            return picturePathLabelIsVisible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CheckBoxStateToAccessibleName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var checkedState = (bool)value;

            var resourceParam = (string)parameter;

            var checkBoxName = AppResources.ResourceManager.GetString(resourceParam);

            return (checkedState ? "Checked, " : "Unchecked, ") + checkBoxName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;

            return (Aspect)intValue;
        }
    }

    public class PathToFileName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = (string)value;

            return Path.GetFileName(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;

            return (Aspect)intValue;
        }
    }
}
