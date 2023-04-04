using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();
            Volume.Value = _mediaPlayer.Volume;
        }

        List<string> _paths = new();
        MediaPlayer _mediaPlayer = new();
        DispatcherTimer dispatcherTimer = new();
        bool _isRandom = false;
        bool _isRepeat = false;

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if ((_mediaPlayer.Source != null) && (_mediaPlayer.NaturalDuration.HasTimeSpan))
            {
                TimerProgressBar.Minimum = 0;
                TimerProgressBar.Maximum = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                TimerProgressBar.Value = _mediaPlayer.Position.TotalSeconds;
                TotalTimeTB.Text = TimeSpan.FromSeconds(_mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds).ToString(@"hh\:mm\:ss");
            }

            if (_mediaPlayer.Position == _mediaPlayer.NaturalDuration && _isRepeat)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Play();
            }

            if (_mediaPlayer.Position == _mediaPlayer.NaturalDuration && trackList.SelectedIndex < trackList.Items.Count - 1 && !_isRandom)
            {
                 Next_Click(sender, null);
            }

            if (_mediaPlayer.Position == _mediaPlayer.NaturalDuration && _isRandom)
            {
                trackList.SelectedIndex = new Random().Next(trackList.Items.Count);
            }


        }


        private void Add_Click(object sender, RoutedEventArgs e)
        {

                var ofd = new OpenFileDialog();
                ofd.Multiselect = true;
                ofd.Filter = "mp3 files(*.mp3)|*.mp3|cda files(*.cda)|*cda|flac files(*.flac)|*flac|aac files(*.aac)|*aac|m4a files(*.m4a)|*m4a|wav files(*.wav)|*wav";

                if (ofd.ShowDialog() == false) return;
                foreach (string path in ofd.FileNames)
                {
                    _paths.Add(path);
                }
                foreach (string path in ofd.SafeFileNames)
                {
                    trackList.Items.Add(path);
                }
        }

        private void trackList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _mediaPlayer.Open(new Uri(_paths[trackList.SelectedIndex], UriKind.RelativeOrAbsolute));
            _mediaPlayer.Play();
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            if(trackList.SelectedItem != null) 
            {
                _paths.RemoveAt(trackList.SelectedIndex);
                trackList.Items.RemoveAt(trackList.SelectedIndex);
                _mediaPlayer.Pause();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _paths.Clear();
            trackList.Items.Clear();
            _mediaPlayer.Pause();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Playlist files(*.plst)|*.plst";
            if (sfd.ShowDialog() == false) return;
            StreamWriter writer = new StreamWriter(sfd.FileName);
            foreach( var item in _paths) 
            {
                writer.WriteLine(item);
            }
            MessageBox.Show("Playlist have been saved");
            writer.Close();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Playlist files(*.plst)|*.plst";
            if (ofd.ShowDialog() == false) return;
            Clear_Click(sender, e );
            using (StreamReader reader = new StreamReader(ofd.FileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    _paths.Add(line);
                    trackList.Items.Add(System.IO.Path.GetFileName(line));
                }
            }
        }

       private void trackList_SelectionChanged(object sender, SelectionChangedEventArgs e)
       {
            if (trackList.SelectedIndex == -1)
            {
                _mediaPlayer.Stop();
            }
            else
            {
                _mediaPlayer.Open(new Uri(_paths[trackList.SelectedIndex], UriKind.RelativeOrAbsolute));
                _mediaPlayer.Play();
            }
            //
            
       }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Play();
        }

        private void TimerProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {                            
            PassedTimeTB.Text = TimeSpan.FromSeconds(TimerProgressBar.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _mediaPlayer.Volume += (e.Delta > 0) ? 0.01 : -0.01;
           Volume.Value = _mediaPlayer.Volume;
            VolumeTB.Text = Math.Round(Volume.Value * 100) + "%";
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if(trackList.SelectedIndex <= trackList.Items.Count -1)
            {
                ++trackList.SelectedIndex;
            }
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaPlayer.Volume = Volume.Value;
            VolumeTB.Text = Math.Round(Volume.Value * 100) + "%";
        }

        private void TimerProgressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mediaPlayer.Position  = TimeSpan.FromSeconds((e.GetPosition(TimerProgressBar).X * TimerProgressBar.Maximum) / TimerProgressBar.ActualWidth);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (trackList.SelectedIndex > 0)
            {
                --trackList.SelectedIndex;
            }
        }

        private void ButtonHide_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //перенести в словарь ресурсов изменение фона кнопки рандом при помощи тригеров
        private void Random_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRandom)
            {
                _isRandom = true;
                Random.Background = new SolidColorBrush(Colors.BurlyWood);
            }
            else 
            {
                _isRandom = false;
                Random.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#0f101c");
            }
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRepeat)
            {
                _isRepeat = true;
                Repeat.Background = new SolidColorBrush(Colors.BurlyWood);
            }
            else
            {
                _isRepeat = false;
                Repeat.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#0f101c");
            }
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal) this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }
    }
}
