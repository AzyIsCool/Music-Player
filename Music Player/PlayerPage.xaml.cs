using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Music_Player
{
    /// <summary>
    /// Interaction logic for PlayerPage.xaml
    /// </summary>
    public partial class PlayerPage : Page
    {
        bool unloading = false;
        bool playing = false;
        MediaPlayer mediaPlayer = new MediaPlayer();

        public PlayerPage()
        {
            InitializeComponent();
            Unloaded += PlayerPage_Unloaded;
        }

        private void PlayerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            unloading = true;
        }

        async Task TimerLogic()
        {
            while (true)
            {
                if (!unloading)
                {
                    if (mediaPlayer.NaturalDuration.HasTimeSpan)
                    {
                        playPauseSong.Visibility = Visibility.Visible;
                        stopSong.Visibility = Visibility.Visible;
                        double d = (double.Parse(mediaPlayer.Position.Ticks.ToString()) / double.Parse(mediaPlayer.NaturalDuration.TimeSpan.Ticks.ToString())) * 100;
                        Time.Text = mediaPlayer.Position.ToString(@"mm\:ss");
                        LineProgressBar.Value = d;
                        CircleProgressBar.Value = LineProgressBar.Value;
                        if (d == 100)
                        {
                            playing = false;
                            playPauseSong.Content = "Play song";
                        }
                    }
                    await Task.Delay(1);
                }
            }
        }

        private async void loadSong_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            if (!string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                mediaPlayer.Play();
                playing = true;
                await TimerLogic();
            }
        }

        private void stopSong_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            playing = false;
            playPauseSong.Content = "Play song";
        }

        private void playPauseSong_Click(object sender, RoutedEventArgs e)
        {
            if (playing)
            {
                mediaPlayer.Pause();
                playing = false;
                playPauseSong.Content = "Play song";
            }
            else
            {
                if (mediaPlayer.Position == mediaPlayer.NaturalDuration.TimeSpan)
                {
                    mediaPlayer.Position = new TimeSpan(0);
                }
                mediaPlayer.Play();
                playing = true;
                playPauseSong.Content = "Pause song";
            }
        }
    }
}
