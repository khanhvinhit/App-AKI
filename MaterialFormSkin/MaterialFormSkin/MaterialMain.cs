using MaterialFormSkin.Properties;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using MaterialFormSkin.DAO;
using YoutubeExtractor;

namespace MaterialFormSkin
{
    public partial class MaterialMain : MaterialForm
    {
        #region Properties

        private const double timerUpdate = 1000;
        private NetworkInterface[] nicArr;
        private System.Windows.Forms.Timer timer;
        decimal downTime = 0;
        double gio = 0;
        double i = 0;
        private readonly MaterialSkinManager skinManager;
        private int colorSchemeIndex;
        List<string> list = new List<string>();
        Boolean b = false, play = false, mute = false;
        int song = 0;
        string link, linkphu;
        Stopwatch Stopwatch = new Stopwatch();

        private DAO.ChessBoardManager chessBoard;

        #endregion
        #region Initialize
        public MaterialMain()
        {
            InitializeComponent();
            skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            string a = Settings.Default["Background"].ToString();
            skinManager.Theme = a == "1" ? skinManager.Theme = MaterialSkinManager.Themes.DARK : skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            SetColor(Settings.Default["Color"].GetHashCode());
            //skinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            InitializeTimer();
            //Vẽ khung
            chessBoard = new ChessBoardManager(pn, txtNameClient, picMark);
            chessBoard.EndedGame += ChessBoard_EndedGame;
            chessBoard.PlayerMarked += ChessBoard_PlayerMarked;
            prbCoolDown.Step = Cons.COOL_DOWN_STEP;
            prbCoolDown.Maximum = Cons.COOL_DOWN_TIME;
            prbCoolDown.Value = 0;
            timerCoolDown.Interval = Cons.COOL_DOWN_INTERVAL;

            chessBoard.DrawChessBoard();

        }

        #endregion
        #region method

        void EndGame()
        {
            timerCoolDown.Stop();
            pn.Enabled = false;
            MessageBox.Show("Kết thúc");
        }
        /// <summary>
        /// List Nhac
        /// </summary>
        /// <param name="t">Hiển thị list nhạc</param>
        public void displayList(String[] t)
        {
            String[] s = t;
            if (!b)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    FileInfo file = new FileInfo(s[i]);
                    list_FileNhac.Items.Add(file.Name);
                    list.Add(s[i]);
                }
                axWindowsMediaPlayer1.URL = list[0].ToString();
                lbStop.Enabled = true;
                axWindowsMediaPlayer1.settings.volume = 80;
                list_FileNhac.SelectedIndex = song;
                btnPlay.Icon = Properties.Resources.pause;
                play = true;
                b = true;
            }
            else
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (!list.Contains(s[i].ToString()))
                    {
                        FileInfo file = new FileInfo(s[i]);
                        if (s[i].Contains(".mp3") || s[i].Contains(".mp4"))
                        {
                            list.Add(s[i]);
                            list_FileNhac.Items.Add(file.Name);
                        }
                    }
                }
            }
            if (list_FileNhac.Items.Count > 1)
            {
                list_FileNhac.Enabled = true;
            }
        }

        /// <summary>
        /// Load mạng vào cbb
        /// </summary>
        private void InitializeNetworkInterface()
        {
            nicArr = NetworkInterface.GetAllNetworkInterfaces();
            List<string> goodAdapters = new List<string>();

            foreach (NetworkInterface nicnac in nicArr)
            {
                if (nicnac.SupportsMulticast && nicnac.GetIPv4Statistics().UnicastPacketsReceived >= 1 && nicnac.OperationalStatus.ToString() == "Up")//
                {
                    goodAdapters.Add(nicnac.Name);
                }

            }

            if (goodAdapters.Count != cbInterface.Items.Count && goodAdapters.Count != 0)
            {
                cbInterface.Items.Clear();
                foreach (string gadpt in goodAdapters)
                {
                    cbInterface.Items.Add(gadpt);

                }
                cbInterface.SelectedIndex = 0;

            }
            if (goodAdapters.Count == 0) cbInterface.Items.Clear();
        }
        /// <summary>
        /// Hàm chạy thời gian
        /// </summary>
        private void InitializeTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = (int)timerUpdate;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            lbThongBao.Text = "";
            lbTime.Text = "";
            sttBad.Value = 0;
            lbKT.Text = "";
            cobResolution.SelectedIndex = 0;
        }
        /// <summary>
        /// Cập nhật thông tin mạng
        /// </summary>
        private void UpdateNetworkInterface()
        {

            if (cbInterface.Items.Count >= 1)
            {
                NetworkInterface nic = nicArr[cbInterface.SelectedIndex];

                IPv4InterfaceStatistics interfaceStats = nic.GetIPv4Statistics();

                long bytesSentSpeed = (long)(interfaceStats.BytesSent - double.Parse(lblBytesSent.Text)) / 1024;
                long bytesReceivedSpeed = (long)(interfaceStats.BytesReceived - double.Parse(lblBytesReceived.Text)) / 1024;

                // Update the labels
                lbname.Text = nic.Description.ToString();
                lblInterfaceType.Text = nic.NetworkInterfaceType.ToString();
                lblSpeed.Text = nic.Speed.ToString();
                lblSpeed.Text = (nic.Speed).ToString("N0");
                lblBytesReceived.Text = interfaceStats.BytesReceived.ToString("N0");
                lblBytesSent.Text = interfaceStats.BytesSent.ToString("N0");
                lblUpload.Text = bytesSentSpeed.ToString() + " KB/s";
                lblDownload.Text = bytesReceivedSpeed.ToString() + " KB/s";

                UnicastIPAddressInformationCollection ipInfo = nic.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation item in ipInfo)
                {
                    if (item.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        labelIPAddress.Text = item.Address.ToString();
                        //uniCastIPInfo = item;
                        break;
                    }
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            InitializeNetworkInterface();
            UpdateNetworkInterface();
        }

        /// <summary>
        /// Hàm tắt máy
        /// </summary>
        /// <param name="cmd"></param>
        void Shutdown_Restart(string cmd)
        {
            Process.Start("shutdown", cmd);
        }
        /// <summary>
        /// Hàm tính thời gian
        /// </summary>
        void Calculator()
        {
            decimal giochay = 0;
            decimal phutchay = 0;
            decimal giaychay = 0;
            try
            {
                if (txtGio.Text != "00")
                {
                    giochay = decimal.Parse(txtGio.Text);

                }
                if (txtPhut.Text != "00")
                {
                    phutchay = decimal.Parse(txtPhut.Text);

                }
                else
                {
                    phutchay = 0;
                }
                if (txtGiay.Text != "00")
                {
                    giaychay = decimal.Parse(txtGiay.Text);
                }
                else
                {
                    giaychay = 0;
                }
                downTime = giochay * 60 * 60 + phutchay * 60 + giaychay;
                gio = int.Parse((giochay * 60 * 60 + phutchay * 60 + giaychay).ToString()) * 100;
            }
            catch (Exception)
            {
                downTime = 0;
            }
        }
        /// <summary>
        /// Set Color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SetColor(int b)
        {
            if (b > 7) b = 0;

            //These are just example color schemes
            switch (b)
            {
                case 0:
                    skinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
                    sttBad.ForeColor = Color.Gray;
                    cPBar.ProgressColor = Color.Gray;
                    break;
                case 1:
                    skinManager.ColorScheme = new ColorScheme(Primary.Indigo500, Primary.Indigo700, Primary.Indigo100, Accent.Pink200, TextShade.WHITE);
                    sttBad.ForeColor = Color.Indigo;
                    cPBar.ProgressColor = Color.Indigo;
                    break;
                case 2:
                    skinManager.ColorScheme = new ColorScheme(Primary.Green600, Primary.Green700, Primary.Green200, Accent.Red100, TextShade.WHITE);
                    sttBad.ForeColor = Color.Green;
                    cPBar.ProgressColor = Color.Green;
                    break;
                case 3:
                    skinManager.ColorScheme = new ColorScheme(Primary.Red600, Primary.Red700, Primary.Red200, Accent.Green100, TextShade.WHITE);
                    sttBad.BackColor = Color.Red;
                    cPBar.ProgressColor = Color.Red;
                    break;
                case 4:
                    skinManager.ColorScheme = new ColorScheme(Primary.Pink600, Primary.Pink700, Primary.Pink200, Accent.Green100, TextShade.WHITE);
                    sttBad.ForeColor = Color.Pink;
                    cPBar.ProgressColor = Color.Pink;
                    break;
                case 5:
                    skinManager.ColorScheme = new ColorScheme(Primary.Purple600, Primary.Purple700, Primary.Purple200, Accent.Red100, TextShade.WHITE);
                    sttBad.ForeColor = Color.Purple;
                    cPBar.ProgressColor = Color.Purple;
                    break;
                case 6:
                    skinManager.ColorScheme = new ColorScheme(Primary.Yellow600, Primary.Yellow700, Primary.Yellow200, Accent.Blue100, TextShade.WHITE);
                    sttBad.ForeColor = Color.Yellow;
                    cPBar.ProgressColor = Color.Yellow;
                    break;
                case 7:
                    skinManager.ColorScheme = new ColorScheme(Primary.Blue600, Primary.Blue700, Primary.Blue200, Accent.Yellow100, TextShade.WHITE);
                    sttBad.ForeColor = Color.Blue;
                    cPBar.ProgressColor = Color.Blue;
                    break;
            }
        }

        #endregion
        #region Event

        void ChessBoard_PlayerMarked(object sender, EventArgs e)
        {
            timerCoolDown.Start();
            prbCoolDown.Value = 0;
        }
        void ChessBoard_EndedGame(object sender, EventArgs e)
        {
            EndGame();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            cPBar.Invoke((MethodInvoker)delegate
            {
                cPBar.Text = DateTime.Now.ToString("hh:mm:ss");
                cPBar.SubscriptText = DateTime.Now.ToString("tt");
                lbDate.Text = "Hôm nay: " + DateTime.Now.ToString("dd/MM/yyy");
            });
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(10);
            this.Hide();
            this.ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            //Hủy lệnh bằng Cmd
            Shutdown_Restart("-a");
            timer1.Stop();
            lbTime.Text = "";
            lbThongBao.Text = "Hủy lệnh thành công";
            sttBad.Value = 0;

        }

        private void btnRetart_Click(object sender, EventArgs e)
        {
            Calculator();
            Shutdown_Restart("-r");
            lbThongBao.Text = "Restart máy tính thành công";
        }

        private void btnShutdown_Click(object sender, EventArgs e)
        {
            Calculator();
            if (downTime == 0)
            {
                Shutdown_Restart("-s");
                lbThongBao.Text = "Tắt máy thành công";
            }
            else
            {
                string a = "-s -t " + downTime.ToString();
                Shutdown_Restart(a);
                lbThongBao.Text = "Hẹn giờ tắt thành công, số giờ còn lại: ";
                timer1.Enabled = true;
                timer1.Start();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lbTime.Text = gio.ToString() + " ms";
            gio--;
            int a = (int)((i / gio) * 100);
            if (a <= 100)
            {
                i++;
                //lbThu.Text = a.ToString();
                sttBad.Value = Math.Min(a, 100);
            }

            if (gio == 0)
            {
                timer1.Stop();
                lbThongBao.Text = "Hết giờ";
                Application.Exit();
            }
        }

        private void btnBackground_Click(object sender, EventArgs e)
        {
            skinManager.Theme = skinManager.Theme == MaterialSkinManager.Themes.DARK ? MaterialSkinManager.Themes.LIGHT : MaterialSkinManager.Themes.DARK;
            cPBar.InnerColor = cPBar.InnerColor == Color.FromArgb(64, 64, 64) ? Color.White : Color.FromArgb(64, 64, 64);
            cPBar.ForeColor = cPBar.ForeColor == Color.White ? Color.Gray : Color.White;
            cPBar.SubscriptColor = cPBar.SubscriptColor == Color.White ? Color.Gray : Color.White;
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            colorSchemeIndex++;
            SetColor(colorSchemeIndex);
        }

        private void cmsExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (play)
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();
                play = false;
                btnPlay.Icon = Properties.Resources.play;
                itemPlay.Image = Properties.Resources.play_arrow_black;
            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
                play = true;
                btnPlay.Icon = Properties.Resources.pause;
                itemPlay.Image = Properties.Resources.pause_black;
            }
        }

        private void lbStop_Click(object sender, EventArgs e)
        {
            btnPlay.Icon = Properties.Resources.play;
            itemPlay.Image = Properties.Resources.play_arrow_black;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
        }

        private void btnMute_Click(object sender, EventArgs e)
        {
            if (!mute)
            {
                axWindowsMediaPlayer1.settings.mute = true;
                mute = true;
                btnMute.Icon = Properties.Resources.mute;
                itemMute.Image = Properties.Resources.volume_off;
            }
            else
            {
                axWindowsMediaPlayer1.settings.mute = false;
                mute = false;
                btnMute.Icon = Properties.Resources.volup;
                itemMute.Image = Properties.Resources.volume_up;
            }
        }

        private void btnPrevMusic_Click(object sender, EventArgs e)
        {
            if (song > 0)
            {
                song--;
                list_FileNhac.SelectedIndex = song;
                axWindowsMediaPlayer1.URL = list[song].ToString();
            }
        }

        private void btnNextMusic_Click(object sender, EventArgs e)
        {
            if (song < list_FileNhac.Items.Count - 1)
            {
                list_FileNhac.SelectedIndex = ++song;
                axWindowsMediaPlayer1.URL = list[list_FileNhac.SelectedIndex].ToString();
            }
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            if (list_FileNhac.SelectedIndex < list.Count - 1)
            {
                list_FileNhac.SelectedIndex = list_FileNhac.SelectedIndex + 1;
                axWindowsMediaPlayer1.URL = list[list_FileNhac.SelectedIndex].ToString();
                timer2.Enabled = false;
            }
            else
            {
                list_FileNhac.SelectedIndex = 0;
                timer2.Enabled = false;
            }
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                timer2.Interval = 100;
                timer2.Enabled = true;
            }
        }

        private void list_FileNhac_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            if (list_FileNhac.SelectedIndex > 0)
            {
                axWindowsMediaPlayer1.URL = list[list_FileNhac.SelectedIndex].ToString();
            }

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            InitializeTimer();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                notifyIcon.Visible = false;
                WindowState = FormWindowState.Normal;
                this.Show();
                this.ShowInTaskbar = true;

            }
        }

        private void btnClearList_Click(object sender, EventArgs e)
        {
            btnPlay.Icon = Properties.Resources.play;
            itemPlay.Image = Properties.Resources.play_arrow_black;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            for (int i = 0; i < list_FileNhac.Items.Count; i++)
            {
                list.Remove(list[0]);
            }
            list_FileNhac.Items.Clear();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (txtLink.Text == "")
            {
                lbKT.Text = "Link trống!";
            }
            else
            {
                if (txtPath.Text == "")
                {
                    lbKT.Text = "Thư mục lưu trống!";
                }
                else
                {
                    lbKT.Text = "";
                    btnWatch.Enabled = false;
                    btnWatch.Visible = false;
                    prbDownload.Minimum = 0;
                    prbDownload.Maximum = 100;
                    IEnumerable<VideoInfo> videos = DownloadUrlResolver.GetDownloadUrls(txtLink.Text);
                    var kt = videos.FirstOrDefault(p => p.Resolution == Convert.ToInt32(cobResolution.Text));
                    if (kt != null)
                    {
                        VideoInfo video = videos.First(p => p.VideoType == VideoType.Mp4 && p.Resolution == Convert.ToInt32(cobResolution.Text));
                        if (video.RequiresDecryption)
                            DownloadUrlResolver.DecryptDownloadUrl(video);


                        link = video.Title + video.VideoExtension;
                        link = link.Replace("|", "_").Replace(" ", "_");
                        link = txtPath.Text + "\\" + link;
                        if (!System.IO.File.Exists(link))
                        {
                            VideoDownloader downloader = new VideoDownloader(video, link);
                            downloader.DownloadProgressChanged += DownloadProgressChanged;
                            Thread thread = new Thread(() => { downloader.Execute(); }) { IsBackground = true };
                            thread.Start();
                        }
                        else
                        {
                            lbKT.Text = "Video đã tồn tại!";
                        }
                    }
                    else
                    {
                        lbKT.Text = "Độ phân giải không phù hợp!";
                    }
                }
            }
        }


        private void btnPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.ShowNewFolderButton = true;
            f.Description = "Lưu video";
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtPath.Text = f.SelectedPath;
            }
        }

        private void MaterialMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default["Background"] = skinManager.Theme == MaterialSkinManager.Themes.DARK ? "1" : "0";
            Settings.Default["Color"] = colorSchemeIndex;
            Settings.Default.Save();
        }

        private void btnWatch_Click(object sender, EventArgs e)
        {
            Process.Start(linkphu);
        }

        private void timerCoolDown_Tick(object sender, EventArgs e)
        {
            prbCoolDown.PerformStep();
            if (prbCoolDown.Value >= prbCoolDown.Maximum)
            {
                EndGame();
                
            }
        }

        private void list_FileNhac_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                list.RemoveAt(list_FileNhac.SelectedIndex);
                list_FileNhac.Items.RemoveAt(list_FileNhac.SelectedIndex);
            }
        }

        private void btnLoadMedia_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                displayList(openFileDialog1.FileNames);

            }
        }
        private void DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            Invoke(new MethodInvoker(delegate ()
            {
                prbDownload.Value = (int)e.ProgressPercentage;
                lblPercent.Text = string.Format("{0:0,##}", e.ProgressPercentage) + "%";
                prbDownload.Update();
                if (e.ProgressPercentage == 100)
                {
                    btnWatch.Enabled = true;
                    btnWatch.Visible = true;
                    txtLink.Text = "";
                    txtPath.Text = "";
                    linkphu = link;
                    link = "";
                    lbKT.Text = "Tải xong!";
                    prbDownload.Value = 0;
                }
            }));
        }
        #endregion


    }
}
