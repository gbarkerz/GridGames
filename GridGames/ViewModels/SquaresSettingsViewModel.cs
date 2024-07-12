using GridGames.ResX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames.ViewModels
{
    public class SquareSettingsViewModel : BaseViewModel
    {
        public SquareSettingsViewModel()
        {
            Title = AppResources.ResourceManager.GetString("SquaresSettings");
        }

        private bool showNumbers;
        public bool ShowNumbers
        {
            get
            {
                return showNumbers;
            }
            set
            {
                if (showNumbers != value)
                {
                    SetProperty(ref showNumbers, value);

                    Preferences.Set("ShowNumbers", value);
                }
            }
        }

        private int numberSizeIndex;
        public int NumberSizeIndex
        {
            get
            {
                return numberSizeIndex;
            }
            set
            {
                if (numberSizeIndex != value)
                {
                    SetProperty(ref numberSizeIndex, value);

                    Preferences.Set("NumberSizeIndex", value);
                }
            }
        }

        private bool showPicture;
        public bool ShowPicture
        {
            get
            {
                return showPicture;
            }
            set
            {
                if (showPicture != value)
                {
                    SetProperty(ref showPicture, value);

                    Preferences.Set("ShowPicture", value);
                }
            }
        }

        private string picturePathSquares;
        public string PicturePathSquares
        {
            get
            {
                return picturePathSquares;
            }
            set
            {
                if (picturePathSquares != value)
                {
                    SetProperty(ref picturePathSquares, value);

                    Preferences.Set("PicturePathSquares", value);
                }
            }
        }

        private string pictureName;
        public string PictureName
        {
            get
            {
                return pictureName;
            }
            set
            {
                if (pictureName != value)
                {
                    SetProperty(ref pictureName, value);

                    Preferences.Set("PictureName", value);
                }
            }
        }

        private int gridSizeScale = 100;
        public int GridSizeScale
        {
            get
            {
                return gridSizeScale;
            }
            set
            {
                if (gridSizeScale != value)
                {
                    SetProperty(ref gridSizeScale, value);

                    Preferences.Set("GridSizeScale", (int)value);
                }
            }
        }
    }
}
