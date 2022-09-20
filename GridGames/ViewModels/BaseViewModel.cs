using GridGames.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GridGames.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private bool gameIsLoading = true;
        public bool GameIsLoading
        {
            get { return gameIsLoading; }
            set
            {
                SetProperty(ref gameIsLoading, value);
            }
        }

        private bool showDarkTheme = false;
        public bool ShowDarkTheme
        {
            get
            {
                return showDarkTheme;
            }
            set
            {
                if (showDarkTheme != value)
                {
                    SetProperty(ref showDarkTheme, value);
                }
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaiseNotificationEvent(string notification)
        {
            Debug.WriteLine("GridGames: Announcing \"" + notification + "\"");

            var reader = SemanticScreenReader.Default;
            if (reader != null)
            {
                try
                {
                    reader.Announce(notification);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("GridGames: ALERT! " + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("GridGames: ALERT! SemanticScreenReader.Default is null");
            }
        }

        public void RaiseDelayedNotificationEvent(string notification)
        {
            timer = new Timer(new TimerCallback((s) => NowAnnounce(notification)),
                               null, 
                               TimeSpan.FromMilliseconds(2000),
                               TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        private Timer timer;

        private void NowAnnounce(string notification)
        {
            timer.Dispose();

            var newThread = new System.Threading.Thread(() =>
            {
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    Debug.WriteLine("Perform delayed announcement. \"" + 
                        notification + "\"");

                    SemanticScreenReader.Default.Announce(notification);
                });
            });

            newThread.Start();
        }
    }
}
