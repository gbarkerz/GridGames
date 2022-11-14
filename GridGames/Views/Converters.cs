using GridGames.ResX;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GridGames.Views
{
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
                    gridRowHeightMultiplier = 0.4;
                    break;
                case 3:
                    gridRowHeightMultiplier = 0.5;
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
