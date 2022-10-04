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

    public class WheresLabelContainerHeightToFontSize : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length < 2) || (values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var showNumbers = (bool)values[0];
            var containerHeightPixels = (double)values[1];

            // Future: Properly account for line height etc. For now, just shrink the value.
            // Also this reduces the size to account for tall cells in portrait orientation.
            double fontHeightPoints = 0;

            if (showNumbers)
            {
                fontHeightPoints = containerHeightPixels * 0.3;
            }

            return fontHeightPoints;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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

    public class FirstRunToGridVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var firstRun = (bool)value;

            return firstRun ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

    public class GameIsLoadingToGridOpacity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var gameIsLoading = (bool)value;

            double squareListOpacity = (gameIsLoading ? 0.3 : 1.0);

            return squareListOpacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GameIsLoadingToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isGameLoading = (bool)value;

            return (isGameLoading ? false : true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

    public class NumberSizeIndexToGridRowHeightMultiplier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var numberSizeIndex = (int)value;

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

            return gridRowHeightMultiplier;
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
            var gridHeightMultipler = (double)values[1];

            Debug.WriteLine("Squares label data: gridHeight " + collectionViewHeight +
                ", gridHeightMultiplier " + gridHeightMultipler);

            if (collectionViewHeight <= 0)
            {
                return 0;
            }

            // Future: Properly account for line height etc. For now, just shrink the value.
            // Also this reduces the size to account for tall cells in portrait orientation.
            return collectionViewHeight * 0.25 * gridHeightMultipler;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SquareTargetIndexToIsVisible : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length < 2) || (values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var targetIndex = (int)values[0];
            var picturesVisible = (bool)values[1];

            // Only show a picture if pictures are to be shown and this is not the empty square.
            return picturesVisible && (targetIndex != 15);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowNumbersSquareTargetIndexToLabelVisibility : IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var targetIndex = (int)value;

            // The Border on the empty square is not visible.
            return (targetIndex != 15);
        }

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

            var showNumbers = (bool)values[0];
            var targetIndex = (int)values[1];

            return showNumbers && (targetIndex < 15);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SquareTargetIndexToBackgroundColor : IMultiValueConverter
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

            var targetIndex = (int)values[0];
            bool showDarkTheme = (bool)values[1];

            var colorName = "MidGray";

            if (targetIndex < 15)
            {
                colorName = showDarkTheme ? "Black" : "White";
            }

            return App.Current.Resources[colorName];
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
