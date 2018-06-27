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
using System.ComponentModel;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;


namespace WpfApp6
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        class finfo
        {
            public finfo(string name, string path) { Filename = name; Filepath = path; }
            public string Filename { set; get; }
            public string Filepath { set; get; }
            
        }
        
        //定義綁定用的List 
        BindingList<finfo> list = new BindingList<finfo>();
        //定義Timer  
        DispatcherTimer v_timer = new DispatcherTimer();
        //全域變數  暫停跟歌曲排序  
        bool v_IsPause = true;
        int v_index = -1;
        public MainWindow()
        {
            InitializeComponent();
            //綁定列表 
            lb_list.ItemsSource = list;
            //绑定更新進度條的Timer 
            v_timer.Interval = new System.TimeSpan(10);
            v_timer.Tick += new EventHandler(Progress_timer_Tick);
           
        }

        private void MoveLyric()
        {
            
        }
        
        // 讀取歌詞
        private void ReadLyric(ref finfo f, string filelyric)
        {
            // 簡繁字體GB2312
            string lrc = File.ReadAllText(filelyric, System.Text.Encoding.GetEncoding("GB2312"));
            Regex rx = new Regex(@"(?<=^\[)(\d+:\d+\.\d+).(.+)(?=$)", RegexOptions.Multiline);
            //
            foreach (Match x in rx.Matches(lrc))
            {
                try
                {
                    
                    TimeSpan ti = new TimeSpan(0, int.Parse(x.Value.Substring(0, 2)),
                                            int.Parse(x.Value.Substring(3, 2)));//int.Parse(x.Value.Substring(6, 2))  
                    
                }
                catch
                {
                    //continue;  
                }
            }
        }

        
        private void SetUI(bool start)
        {
            
            v_timer.IsEnabled = start;
            btn_play.Content = start ? "Pause" : "Play";   
            v_IsPause = !start; 
        }
        
        // 播放文件
        private void SetSource()
        {
            // 取檔案資訊
            var obj = lb_list.SelectedItem as finfo;
            if (obj == null) return;

              
            mediaElement.Source = new Uri(obj.Filepath);
            
            
            
            // 設標題
            txt_title.Text = obj.Filename;
            // 歌曲順序
            v_index = lb_list.SelectedIndex;
            // PLLLAY  
            mediaElement.Play();
            SetUI(true);

        }
        #region 開始暫停重播
        /// <summary>  
        /// PLAYBUTTON
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            // 沒檔案的時候poen_Click
            if (list.Count <= 0) { btn_open_Click(this, null); return; }
            // 開始或暫停
            if (v_IsPause) { mediaElement.Play(); }
            else { mediaElement.Pause(); }
            SetUI(v_IsPause);
        }
        /// <summary>  
        /// 重播按鈕
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            if (!mediaElement.HasAudio) return;
            mediaElement.Stop();
            
            // Play  
            SetUI(true);
            mediaElement.Play();
        }
        #endregion
        #region 上一曲、下一曲、自动下一曲  
        /// <summary>  
        /// 播放上一首
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void btn_prev_Click(object sender, RoutedEventArgs e)
        {
            int index = v_index - 1;            if (index >= 0 && index < list.Count)
            {
                lb_list.SelectedIndex = index;
                SetSource();
            }
        }
        /// <summary>  
        /// 播放下一曲  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            int index = v_index + 1;
            if (index >= 0 && index < list.Count)
            {
                lb_list.SelectedIndex = index;
                SetSource();
            }
        }
        /// <summary>  
        /// AUTONEXT  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
       
        #endregion
        #region 音量设置  
        /// <summary>  
        /// 修改音量  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void sd_vol_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //音量
            mediaElement.Volume = e.NewValue / 10;
        }
        /// <summary>  
        /// MUTE
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void ck_vol_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.IsMuted = (bool)ck_vol.IsChecked;
        }
        #endregion
        #region 添加、删除、播放、加载歌词  
        /// <summary>  
        /// 添加檔案  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void btn_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "音檔格式(*.mp3,*.wav)|*.mp3;*.wav";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() != true) return;

            for (int i = 0; i < dlg.FileNames.Length; ++i)
            {
                finfo newfile = new finfo(dlg.SafeFileNames[i], dlg.FileNames[i]);
                string filelyric = dlg.FileNames[i].Remove(dlg.FileNames[i].Length - 4) + ".lrc";
                if (File.Exists(filelyric)) { ReadLyric(ref newfile, filelyric); }
                list.Add(newfile);
            }

            lb_list.SelectedIndex = (list.Count - 1);

            if (v_IsPause)
            {
                // 設置播放檔案  
                SetSource();
            }
        }
        /// <summary>  
        /// DELETE
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void btn_del_Click(object sender, RoutedEventArgs e)
        {
            int index = lb_list.SelectedIndex;
            if (index < 0) return;
            list.RemoveAt(index);
            
            // 如果刪除的歌曲正在播 播放下一首 (好像沒用)
            if (lb_list.SelectedIndex == v_index) btn_next_Click(this, null);
        }
        /// <summary>  
        /// PLAYFILE
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (list.Count <= 0) return;
            if (mediaElement.HasAudio) mediaElement.Stop();
            SetSource();
        }
        /// <summary>  
        /// 歌詞++ 
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void btn_loadlrc_Click(object sender, RoutedEventArgs e)
        {
            if (list.Count <= 0) return;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "文字檔(*.txt)|*.txt";
            if (dlg.ShowDialog() != true) return;

            
        }


        #endregion
        #region 进度条  
        /// <summary>  
        /// TIMER
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        void Progress_timer_Tick(object sender, EventArgs e)
        {
            // 播放時間  
            if (!mediaElement.HasAudio) return;
            try
            {
                TimeSpan ts = mediaElement.Position;
                TimeSpan max = mediaElement.NaturalDuration.TimeSpan;
                
                // 播放進度%數 
                double pos = ts.TotalSeconds / max.TotalSeconds * 100;
                
                //格式化字符串  
                string timepos = string.Format("{0}/{1}", ts.ToString("hh':'mm':'ss"), max.ToString("hh':'mm':'ss"));
                
                //更新進度  
                sd_pos.Value = pos;
                txt_starttime.Text = timepos;
                
            }
            catch
            {
                //MessageBox.Show(ex.Message);  
            }
        }
        /// <summary>  
        /// 拖曳進度條
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void sd_pos_ui_MouseDown(object sender, MouseButtonEventArgs e)
        {
            v_timer.IsEnabled = false;
        }
        private void sd_pos_ui_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!mediaElement.HasAudio) return;
            
            // 計算時間

            TimeSpan max = mediaElement.NaturalDuration.TimeSpan;
            double per = sd_pos.Value / 100;
            TimeSpan pos = TimeSpan.FromSeconds(max.TotalSeconds * per);
             
            mediaElement.Position = pos;
            v_timer.IsEnabled = true;
        }

        #endregion
    }
}
