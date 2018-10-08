using Microsoft.Win32;
using NAudio.CoreAudioApi;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Music_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaPlayer MediaPlayer = new MediaPlayer();
        bool IsPlayingSong = false;
        string SongLocation = "";
        int CircleNumber = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        async Task TimerAndProgressBar()
        {
            UpdateProgressBar();
            while (IsPlayingSong)
            {
                await Task.Delay(1000);
                SongTimer.Text = $"{MediaPlayer.Position.Minutes.ToString("00")}:{MediaPlayer.Position.Seconds.ToString("00")}";
            }
        }

        async Task UpdateProgressBar()
        {
            await Task.Delay(250);
            while (IsPlayingSong)
            {
                try { SliderButton.Value = (MediaPlayer.Position.TotalMilliseconds / MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds) * 100; } catch { }
                await Task.Delay(1);
            }
        }

        async Task MakeLines()
        {
            RotateTransform rotateTransform = new RotateTransform(45);
            Thickness thickness = new Thickness(0, -2, 10, -100);
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Black);

            double amountOfLinesToMake = (SystemParameters.PrimaryScreenWidth - 200) / 12;
            if (amountOfLinesToMake - ((int)amountOfLinesToMake) > 0)
                amountOfLinesToMake = amountOfLinesToMake + (1 - (amountOfLinesToMake - (int)amountOfLinesToMake));

            amountOfLinesToMake += 5;
            while (amountOfLinesToMake != 0)
            {
                Rectangle rectangle = new Rectangle();
                rectangle.Margin = thickness;
                rectangle.Fill = solidColorBrush;
                rectangle.RenderTransform = rotateTransform;
                rectangle.Width = 2;
                VenetianBlindsHolder.Children.Add(rectangle);
                amountOfLinesToMake--;
            }
        }

        async Task MakeRadioWaves()
        {
            while (IsPlayingSong)
            {
                Rectangle circle = new Rectangle();
                circle.Name = $"circle{CircleNumber}";
                CircleNumber++;
                circle.Fill = new SolidColorBrush(Colors.Transparent);
                circle.Stroke = (SolidColorBrush)Application.Current.Resources["MainSolidColour"];
                circle.StrokeThickness = 5;
                circle.RadiusX = 1000;
                circle.RadiusY = 1000;
                RegisterName(circle.Name, circle);
                RadioWaves.Children.Add(circle);
                CircleAmimation(circle);
                await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            }
        }

        private async Task CircleAmimation(object sender)
        {
            var enumerator = new MMDeviceEnumerator();
            var storyboard = new Storyboard();
            var scale = (int)Math.Floor(enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console).AudioMeterInformation.MasterPeakValue * 79);
            
            DoubleAnimation WidthAnimation = new DoubleAnimation
            {
                From = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(scale)),
                To = 50
            };
            DoubleAnimation HeightAnimation = WidthAnimation.Clone();
            DoubleAnimation OpacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(scale)),
            };

            Storyboard.SetTargetProperty(WidthAnimation, new PropertyPath(Rectangle.WidthProperty));
            Storyboard.SetTargetProperty(HeightAnimation, new PropertyPath(Rectangle.HeightProperty));
            Storyboard.SetTargetProperty(OpacityAnimation, new PropertyPath(Rectangle.OpacityProperty));

            Storyboard.SetTargetName(WidthAnimation, ((Rectangle)sender).Name);
            Storyboard.SetTargetName(HeightAnimation, ((Rectangle)sender).Name);
            Storyboard.SetTargetName(OpacityAnimation, ((Rectangle)sender).Name);

            storyboard.Children.Add(HeightAnimation);
            storyboard.Children.Add(WidthAnimation);
            storyboard.Children.Add(OpacityAnimation);
            storyboard.Begin((Rectangle)sender);
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MakeLines().ConfigureAwait(true).GetAwaiter().GetResult();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Supported Files (*.asf; *.aif; *.aifc; *.aiff; *.au; *.avi; *.mid; *.mpe; *.mpeg; *.mpg; *.mpv2; *.mp2; *.mp3; *.m1v; *.snd; *.wav; *.wm; *.wma; *.wmv)|*.asf; *.aif; *.aifc; *.aiff; *.au; *.avi; *.mid; *.mpe; *.mpeg; *.mpg; *.mpv2; *.mp2; *.mp3; *.m1v; *.snd; *.wav; *.wm; *.wma; *.wmv";
            openFileDialog.ShowDialog();
            if (!string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                SongLocation = openFileDialog.FileName;
                var file = File.Open(SongLocation, FileMode.Open);
                SongName.Text = $"Playing: {file.Name}";
                file.Dispose();
                MediaPlayer.Stop();
                MediaPlayer.Open(new Uri(SongLocation, UriKind.Absolute));
                MediaPlayer.Play();
                IsPlayingSong = true;
                PlayPauseButton.IsEnabled = true;
                StopButton.IsEnabled = true;
                PlayPauseButton.Content = "Pause Song";
                TimerAndProgressBar();
                MakeRadioWaves();
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlayingSong)
            {
                MediaPlayer.Pause();
                PlayPauseButton.Content = "Play Song";
            }
            else
            {
                MediaPlayer.Play();
                PlayPauseButton.Content = "Pause Song";
                StopButton.IsEnabled = true;
            }
            IsPlayingSong = !IsPlayingSong;
            TimerAndProgressBar();
            MakeRadioWaves();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            IsPlayingSong = false;
            PlayPauseButton.Content = "Play Song";
            SliderButton.Value = 0;
            StopButton.IsEnabled = false;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Progress.Value = e.NewValue;
        }
    }
}
