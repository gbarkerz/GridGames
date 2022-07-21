using GridGames.ResX;
using GridGames.ViewModels;
using System.Globalization;
using System.Collections.ObjectModel;

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

    public class CardToCollectionViewIndex : IValueConverter
    {
        private static String[] numberWords = {
            AppResources.ResourceManager.GetString("One"),
            AppResources.ResourceManager.GetString("Two"),
            AppResources.ResourceManager.GetString("Three"),
            AppResources.ResourceManager.GetString("Four"),
            AppResources.ResourceManager.GetString("Five"),
            AppResources.ResourceManager.GetString("Six"),
            AppResources.ResourceManager.GetString("Seven"),
            AppResources.ResourceManager.GetString("Eight"),
            AppResources.ResourceManager.GetString("Nine"),
            AppResources.ResourceManager.GetString("Ten"),
            AppResources.ResourceManager.GetString("Eleven"),
            AppResources.ResourceManager.GetString("Twelve"),
            AppResources.ResourceManager.GetString("Thirteen"),
            AppResources.ResourceManager.GetString("Fourteen"),
            AppResources.ResourceManager.GetString("Fifteen"),
            AppResources.ResourceManager.GetString("Sixteen") };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var card = (Card)value;
            if (card == null)
            {
                return -1;
            }

            var binding = (Binding)parameter;
            var collectionView = (CollectionView)binding.Source;

            var vm = collectionView.BindingContext as MatchingViewModel;

            var collectionViewIndex = vm.SquareListCollection.IndexOf(card);

            var fullName = card.AccessibleName + " " + numberWords[collectionViewIndex];

            // Return a word here, to avoid speech of "1" being ambiguous between
            // 1, 10, 11, etc.
            return fullName;
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
                fontHeightPoints = containerHeightPixels * 0.4;
            }

            return fontHeightPoints;
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
}
