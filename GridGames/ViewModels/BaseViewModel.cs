using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

        private bool gameIsLoading = false;
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
                    // Always delay the announcement, just in case the screen reader wants to
                    // immediately announce the focused item around the time of this announcement.
                    // (Otherwise on iOS the custom announcements don't get announced.)s
                    RaiseDelayedNotificationEvent(notification, 200);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GridGames: ALERT! " + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("GridGames: ALERT! SemanticScreenReader.Default is null");
            }
        }

        private Timer timedDelayedAnnouncement;
        private string mostRecentDelayedAnnouncement;

        public void RaiseDelayedNotificationEvent(string notification, int msDelay)
        {
            Debug.WriteLine("Delay notification: \"" +
                notification + "\"");

            // If multiple custom announcements are attempted in succession,
            // only announce the most recent.
            mostRecentDelayedAnnouncement = notification;

            if (timedDelayedAnnouncement == null)
            {
                timedDelayedAnnouncement = new Timer(new TimerCallback((s) => NowAnnounce()),
                                   null,
                                   TimeSpan.FromMilliseconds(msDelay),
                                   TimeSpan.FromMilliseconds(Timeout.Infinite));
            }
        }

        private void NowAnnounce()
        {
            timedDelayedAnnouncement.Dispose();
            timedDelayedAnnouncement = null;

            Debug.WriteLine("Now announce: \"" +
                mostRecentDelayedAnnouncement + "\"");

            // Always run this on the UI thread. (On Windows an exception is thrown otherwise.)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Debug.WriteLine("Now announce on UI thread: \"" +
                    mostRecentDelayedAnnouncement + "\"");

                SemanticScreenReader.Default.Announce(mostRecentDelayedAnnouncement);
            });
        }
    }
}
