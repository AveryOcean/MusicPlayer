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
    /// <summary>
    /// This class, as you can tell by the name, contains data for songs.
    /// </summary>
    class SongData
    {
        public string fileName;

        public string songName;
        public string artist;

        public SongData(string file, string name, string artist)
        {
            this.fileName = file;
            this.songName = name;
            this.artist = artist;
        }
    }

    public partial class MainPage : ContentPage
    {
        private static PickOptions PickOption_Music = new PickOptions
        {
            PickerTitle = "Music Files",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/x-wav", "audio/ogg" } }, //Add more platforms later
            })
        };

        public MainPage()
        {
            InitializeComponent();

            CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.Off;
            loopBtn.Text = TextBasedOnLoopStatus();
        }

        private List<FileResult> songQueue = new List<FileResult>();
        private List<SongData> songData = new List<SongData>();

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
            CrossMediaManager.Current.ToggleRepeat();

            loopBtn.Text = TextBasedOnLoopStatus();
        }

        private async void PlaySong()
        {
            ShowSongInfo(songName, artistName);

            playing = true;
            progressBar.IsEnabled = true;
            await CrossMediaManager.Current.Play(songQueue[songIndex].FullPath);

            CrossMediaManager.Current.PositionChanged += Current_PositionChanged;
            CrossMediaManager.Current.StateChanged += Current_StateChanged;
        }

        private void Current_StateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
        {
            if (e.State != MediaManager.Player.MediaPlayerState.Playing)
                return;

            try
            {
                var songLengthSeconds = CrossMediaManager.Current.Duration.TotalSeconds;
                progressBar.Maximum = songLengthSeconds;
            }
            catch { }
        }

        private void Current_PositionChanged(object sender, MediaManager.Playback.PositionChangedEventArgs e)
        {
            if (CrossMediaManager.Current.State != MediaManager.Player.MediaPlayerState.Playing)
                return;

            var progress = CrossMediaManager.Current.Position.TotalSeconds;
            progressBar.Value = progress;
        }

        private void IncreaseSongIndex()
        {
            songIndex++;
            if (songIndex >= songQueue.Count) songIndex = 0;
        }

        private async void PlayNext(object sender, EventArgs e)
        {
            if (!playing)
                return;

            IncreaseSongIndex();
            PlaySong();
        }

        private void DecreaseSongIndex()
        {
            songIndex--;
            if (songIndex < 0) songIndex = songQueue.Count - 1;
        }

        private async void PlayPrevious(object sender, EventArgs e)
        {
            if (!playing)
                return;

            DecreaseSongIndex();
            PlaySong();
        }

        private void progressBar_DragCompleted(object sender, EventArgs e)
        {
            if (!playing) return;
            var seekTo = progressBar.Value;

            CrossMediaManager.Current.SeekTo(TimeSpan.FromSeconds(seekTo));
        }

        private void InfoCreator_Next(object sender, EventArgs e)
        {
            IncreaseSongIndex();
            ShowSongInfo(info_currentSong, info_currentArtist);
        }

        private void ShowSongInfo(Label currentSong, Label currentArtist)
        {
            if (songQueue.Count < 1)
                return;

            var song = songQueue[songIndex];
            var dataEnum = songData.Where(x => x.fileName == song.FileName);

            if(dataEnum.Count() < 1)
            {
                currentSong.Text = song.FileName;
                currentArtist.Text = string.Empty;
                return;
            }

            var data = dataEnum.FirstOrDefault();
            currentSong.Text = data.songName;
            currentArtist.Text = data.artist;
        }

        private void InfoCreator_AddSongInfo(object sender, EventArgs e)
        {
            //Get song data, if it exists
            var song = songQueue[songIndex];
            var dataEnum = songData.Where(x => x.fileName == song.FileName);

            var name = info_songName.Text;
            var artist = info_songArtist.Text;

            if (dataEnum.Count() < 1)
            {
                //Add new
                var newData = new SongData(song.FileName, name, artist);
                songData.Add(newData);

                ShowSongInfo(info_currentSong, info_currentArtist);
                ShowSongInfo(songName, artistName);
                return;
            }

            //Modify current
            var data = dataEnum.FirstOrDefault();
            data.songName = name;
            data.artist = artist;

            ShowSongInfo(info_currentSong, info_currentArtist);
            ShowSongInfo(songName, artistName);
        }

        private void InfoCreator_Prev(object sender, EventArgs e)
        {
            DecreaseSongIndex();
            ShowSongInfo(info_currentSong, info_currentArtist);
        }
    }
}
