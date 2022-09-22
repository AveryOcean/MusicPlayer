using MediaManager;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace MusicViewer
{
    public partial class MainPage : ContentPage
    {
        private static PickOptions PickOption_Music = new PickOptions
        {
            PickerTitle = "Music Files",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/x-wav", "audio/ogg" } },
            })
        };

        public MainPage()
        {
            InitializeComponent();

            CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.Off;
            loopBtn.Text = TextBasedOnLoopStatus();
        }

        private List<FileResult> songQueue = new List<FileResult>();

        private async void AddToQueue(object sender, EventArgs e)
        {
            try
            {
                //var file = await FilePicker.PickAsync(PickOption_Music);

                //if (file == null)
                //    return;

                //CrossMediaManager.Current.Init();
                //await CrossMediaManager.Current.Play(file.FullPath);

                var files = await FilePicker.PickMultipleAsync(PickOption_Music);
                var list = files.ToList();

                if (list.Count < 1)
                    return;

#if DEBUG
                var notification = new NotificationRequest()
                {
                    Title = list.Count.ToString()
                };

                _ = notification.Show();
#endif

                songQueue.AddRange(list);
            }
            catch (Exception ex)
            {
#if DEBUG
                var notification = new NotificationRequest()
                {
                    CategoryType = NotificationCategoryType.Error,
                    Title = "An Error Occured!",
                    Description = ex.Message,
                    NotificationId = 200
                };

                _ = notification.Show();
#endif
            }
        }

        private void ClearSongQueue(object sender, EventArgs e)
        {
            songQueue.Clear();
        }

        int songIndex = 0;
        bool playing = false;
        private async void PlayCurrentSong(object sender, EventArgs e)
        {
            CrossMediaManager.Current.Dispose();
            CrossMediaManager.Current.Init();

            if (songQueue.Count < 1)
                return;

            if(playing)
            {
                await CrossMediaManager.Current.Pause();
                playing = false;
                progressBar.IsEnabled = false;
                return;
            }

            PlaySong();
        }

        private void StepBack(object sender, EventArgs e)
        {
            if (!playing)
                return;

            CrossMediaManager.Current.StepBackward();
        }

        private void StepForward(object sender, EventArgs e)
        {
            if (!playing)
                return;

            CrossMediaManager.Current.StepForward();
        }

        private string TextBasedOnLoopStatus()
        {
            string output = "";

            switch (CrossMediaManager.Current.RepeatMode)
            {
                case MediaManager.Playback.RepeatMode.Off:
                    output = "OFF";
                    break;
                case MediaManager.Playback.RepeatMode.One:
                    output = "ONE";
                    break;
                case MediaManager.Playback.RepeatMode.All:
                    output = "ALL";
                    break;
                default:
                    break;
            }

            return output;
        }

        private void ToggleLoop(object sender, EventArgs e)
        {
            if (!playing)
                return;

            CrossMediaManager.Current.ToggleRepeat();

            loopBtn.Text = TextBasedOnLoopStatus();
        }

        private async void PlaySong()
        {
            playing = true;
            progressBar.IsEnabled = true;
            await CrossMediaManager.Current.Play(songQueue[songIndex].FullPath);

            CrossMediaManager.Current.PositionChanged += Current_PositionChanged;
        }

        private void Current_PositionChanged(object sender, MediaManager.Playback.PositionChangedEventArgs e)
        {
            try
            {
                var songLengthSeconds = CrossMediaManager.Current.Duration.TotalSeconds;
                progressBar.Maximum = songLengthSeconds;
            } catch { }

            var progress = CrossMediaManager.Current.Position.TotalSeconds;
            progressBar.Value = progress;
        }

        private async void PlayNext(object sender, EventArgs e)
        {
            songIndex++;
            if (songIndex >= songQueue.Count) songIndex = 0;

            PlaySong();
        }

        private async void PlayPrevious(object sender, EventArgs e)
        {
            songIndex--;
            if (songIndex < 0) songIndex = songQueue.Count-1;

            PlaySong();
        }

        private void progressBar_DragCompleted(object sender, EventArgs e)
        {
            if (!playing) return;
            var seekTo = progressBar.Value;

            CrossMediaManager.Current.SeekTo(TimeSpan.FromSeconds(seekTo));
        }
    }
}
