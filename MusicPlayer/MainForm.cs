
using MusicPlayer.DataRepo;
using MusicPlayer.Model;
using MusicPlayer.Service;
using MusicPlayer.Utils;
using MusicPlayer.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class MainForm : Form
    {
        private static MainForm instance;
        public static MainForm Instance { get => instance; }
        public AxWMPLib.AxWindowsMediaPlayer MediaPlayer { get => mPlayer; }

        public Playlist isPlaying;
        public List<int> playingOrder;
        public int counter = 0;
        public string playingMode = "one";

        public Playlist favorite;

        public int timeTrack = 0;
        public int curLyric = 0;

        public MainForm()
        {
            InitializeComponent();
            instance = this;
            btnLocal_Click(btnLocal, null);
            favorite = new Playlist("fav");
            Data.LoadSetting();
            Data.LoadMyPlayList();
        }
        #region Media player
        public void PlayMedia()
        {
            timeTrack = 0;
            Song song = isPlaying.files[playingOrder[counter]];
            mPlayer.URL = song.GetSrc();
            LoadCurrentLyric();
            if (song.GetThumbImg() is Image)
            {
                pbSong.Image = null;
                pbSong.BackgroundImage = song.GetThumbImg() as Image;
                pbSong.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else if (song.GetThumbImg() is string)
            {
                pbSong.BackgroundImage = null;
                pbSong.SizeMode = PictureBoxSizeMode.StretchImage;
                pbSong.LoadAsync(song.GetThumbImg().ToString());
            }
            else
            {
                pbSong.Image = null;
                pbSong.BackgroundImage = Properties.Resources.icons8_music_48px_1;
                pbSong.BackgroundImageLayout = ImageLayout.Center;
            }
            lbSongName.Text = song.GetTitle() + "\n" + song.GetArtistNameJoined();
            if (song.GetTitle() == "" || song.GetTitle() == null)
            {
                lbSongName.Text = Path.GetFileNameWithoutExtension(song.GetSrc());
            }
        }
        private void mPlayer_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (mPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                btnPlay.BackgroundImage = Properties.Resources.icons8_pause_32px;
                trackBar.MaxValue = (int)mPlayer.Ctlcontrols.currentItem.duration;
                lbMaxTime.Text = mPlayer.Ctlcontrols.currentItem.durationString;
                tTrackBar.Start();
                timerLyric.Start();
            }
            else if (mPlayer.playState == WMPLib.WMPPlayState.wmppsPaused)
            {
                btnPlay.BackgroundImage = Properties.Resources.icons8_play_32px;
                tTrackBar.Stop();
                timerLyric.Stop();
            }
            else if (mPlayer.playState == WMPLib.WMPPlayState.wmppsStopped)
            {
                btnPlay.BackgroundImage = Properties.Resources.icons8_play_32px;
                tTrackBar.Stop();
                timerLyric.Stop();
                trackBar.Value = 0;
                lbMinTime.Text = "00:00";
            }
        }
        private void tTrackBar_Tick(object sender, EventArgs e)
        {
            if (mPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                lbMinTime.Text = mPlayer.Ctlcontrols.currentPositionString;
                if (barChange)
                    trackBar.Value = (int)mPlayer.Ctlcontrols.currentPosition;
            }


        }

        private bool barChange = true;
        private void trackBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (mPlayer.currentMedia != null)
            {
                double cur = mPlayer.Ctlcontrols.currentItem.duration * e.X / (trackBar.Width - 15);
                mPlayer.Ctlcontrols.currentPosition = cur;
                lbMinTime.Text = mPlayer.Ctlcontrols.currentPositionString;
                barChange = true;

                if (isPlaying.files[playingOrder[counter]].lyric != null && !isPlaying.files[playingOrder[counter]].lyric.hasLocalLyric)
                {
                    timerLyric.Stop();
                    timeTrack = ((int)cur * 1000) + 60;

                    int f = rtbLyric.GetFirstCharIndexFromLine(curLyric);
                    int c = rtbLyric.GetLineFromCharIndex(f);
                    string curr = rtbLyric.Lines[c];
                    rtbLyric.Select(f, curr.Length);
                    rtbLyric.SelectionColor = Color.White;

                    int i = 0;
                    foreach (var item in isPlaying.files[playingOrder[counter]].lyric.sentences)
                    {
                        if (item.words[0].startTime >= timeTrack)
                        {
                            curLyric = i + 1;
                            break;
                        }
                        i++;
                    }
                    timerLyric.Start();
                }

            }
        }
        private void trackBar_MouseMove(object sender, MouseEventArgs e)
        {

            if (mPlayer.currentMedia != null && e.Button == MouseButtons.Left)
            {
                barChange = false;
            }
        }

        private void trackbarVolume_MouseUp(object sender, MouseEventArgs e)
        {
            int newVol = 100 * e.X / (trackbarVolume.Width - 15);
            mPlayer.settings.volume = newVol;
            trackbarVolume.Value = newVol;
        }

        private void trackbarVolume_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mPlayer.settings.volume = trackbarVolume.Value;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (isPlaying != null && isPlaying.files.Count > 0)
            {
                if (mPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
                    mPlayer.Ctlcontrols.pause();
                else if (mPlayer.playState == WMPLib.WMPPlayState.wmppsPaused)
                    mPlayer.Ctlcontrols.play();

            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (isPlaying != null && isPlaying.files.Count > 0)
            {
                int old = counter;
                counter++;
                if (counter >= isPlaying.files.Count)
                    counter = 0;
                PlayMedia();
                var ctls2 = panelPlaying.Controls;
                (ctls2[ctls2.Count - 1 - old] as MediaList2).DeActiveSongUI();
                (ctls2[ctls2.Count - 1 - counter] as MediaList2).ActiveSongUI();
                if (isPlaying.name == "local")
                {
                    var ctls = (panelContainer.Controls["LocalView"] as LocalView).GetPanelSong().Controls;
                    (ctls[ctls.Count - 1 - playingOrder[old]] as MediaList1).DeActiveSongUI();
                    (ctls[ctls.Count - 1 - playingOrder[counter]] as MediaList1).ActiveSongUI();
                }
            }

        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (isPlaying != null && isPlaying.files.Count > 0)
            {
                int old = counter;
                counter--;
                if (counter < 0)
                    counter = isPlaying.files.Count - 1;
                PlayMedia();
                var ctls2 = panelPlaying.Controls;
                (ctls2[ctls2.Count - 1 - old] as MediaList2).DeActiveSongUI();
                (ctls2[ctls2.Count - 1 - counter] as MediaList2).ActiveSongUI();
                if (isPlaying.name == "local")
                {
                    var ctls = (panelContainer.Controls["LocalView"] as LocalView).GetPanelSong().Controls;
                    (ctls[ctls.Count - 1 - playingOrder[old]] as MediaList1).DeActiveSongUI();
                    (ctls[ctls.Count - 1 - playingOrder[counter]] as MediaList1).ActiveSongUI();
                }
            }
        }
        private void btnShuffer_Click(object sender, EventArgs e)
        {
            if (isPlaying != null && isPlaying.files.Count > 0)
            {

                if (btnShuffer.Tag.ToString() == "f")
                {
                    int old = playingOrder[counter];
                    ShufferPlaying(playingOrder[counter]);
                    counter = 0;
                    LoadViewPlaying();
                    var ctls2 = panelPlaying.Controls;
                    (ctls2[ctls2.Count - 1] as MediaList2).ActiveSongUI();
                    if (old != 0)
                        (ctls2[ctls2.Count - 1 - old] as MediaList2).DeActiveSongUI();
                    //PlayMedia();
                    btnShuffer.BackColor = Color.FromArgb(27, 28, 34);
                    btnShuffer.BorderSize = 1;
                    btnShuffer.Tag = "t";
                }
                else
                {
                    NormalPlaying(counter);
                    counter = 0;
                    LoadViewPlaying();
                    //PlayMedia();
                    var view = panelPlaying.Controls[panelPlaying.Controls.Count - 1] as MediaList2;
                    view.ActiveSongUI();

                    btnShuffer.BackColor = Color.FromArgb(15, 15, 16);
                    btnShuffer.BorderSize = 0;
                    btnShuffer.Tag = "f";
                }
            }
        }

        private void btnRepeat_Click(object sender, EventArgs e)
        {
            if (isPlaying != null && isPlaying.files.Count > 0)
            {
                if (btnRepeat.Tag.ToString() == "off") // tat
                {
                    btnRepeat.BackColor = Color.FromArgb(27, 28, 34);
                    btnRepeat.BorderSize = 1;
                    btnRepeat.BackgroundImage = Properties.Resources.icons8_repeat_24px;
                    btnRepeat.Tag = "all";
                }
                else if (btnRepeat.Tag.ToString() == "all") // tat ca
                {
                    btnRepeat.BackgroundImage = Properties.Resources.repeat_one;
                    btnRepeat.Tag = "one";
                }
                else // chi 1
                {
                    btnRepeat.BackgroundImage = Properties.Resources.icons8_repeat_24px;
                    btnRepeat.BorderSize = 0;
                    btnRepeat.Tag = "off";
                }
            }
        }
        ToolTip ToolTip1 = new ToolTip();
        private void btnRepeat_MouseHover(object sender, EventArgs e)
        {
            if (btnRepeat.Tag.ToString() == "off")
            {
                ToolTip1.SetToolTip(btnRepeat, "Bật phát lại tất cả");
            }
            else if (btnRepeat.Tag.ToString() == "all") // tat ca
            {
                ToolTip1.SetToolTip(btnRepeat, "Bật phát lại một bài");
            }
            else // chi 1
            {
                ToolTip1.SetToolTip(btnRepeat, "Tắt phát lại");
            }

        }
        private void UpdateIsPlayingSongUI(int oldVal, int newVal)
        {
            var ctls2 = panelPlaying.Controls;
            (ctls2[ctls2.Count - 1 - playingOrder[oldVal]] as MediaList2).DeActiveSongUI();
            (ctls2[ctls2.Count - 1 - playingOrder[newVal]] as MediaList2).ActiveSongUI();
            if (isPlaying.name == "local")
            {
                var ctls = (panelContainer.Controls["LocalView"] as LocalView).GetPanelSong().Controls;
                (ctls[ctls.Count - 1 - playingOrder[oldVal]] as MediaList1).DeActiveSongUI();
                (ctls[ctls.Count - 1 - playingOrder[newVal]] as MediaList1).ActiveSongUI();
            }
        }

        #endregion

        #region Menu
        private void ClearMenu()
        {
            var view = GetContainerView("PlaylistView");
            //var view2 = GetContainerView("LocalView");
            if (view != null)
                view.Dispose();
            //if (view2 != null)
            //    view2.Dispose();

            if (GetContainerView("LocalView") != null)
            {
                var local = (GetContainerView("LocalView") as LocalView).GetPanelSong();
                if (local != null && isPlaying != null)
                {
                    var song = isPlaying.files[playingOrder[counter]];
                    foreach (MediaList1 item in local.Controls)
                    {
                        if (item.song != song) item.DeActiveSongUI();
                        else item.ActiveSongUI();
                    }
                }
            }

            foreach (Button menuButton in panelMenu.Controls.OfType<Button>())
                menuButton.BackColor = Color.FromArgb(25, 26, 31);
        }
        private void ActiveMenu(Button btn, Control view)
        {
            btn.BackColor = Color.FromArgb(255, 128, 0);
            view.Dock = DockStyle.Fill;
            view.BringToFront();
        }

        private void btnLocal_Click(object sender, EventArgs e)
        {
            ClearMenu();
            LocalView view;
            if (!panelContainer.Controls.ContainsKey("LocalView"))
            {
                view = new LocalView();
                panelContainer.Controls.Add(view);
            }
            else
                view = panelContainer.Controls["LocalView"] as LocalView;
            ActiveMenu(sender as Button, view);
        }


        private void btnSearch_Click(object sender, EventArgs e)
        {
            ClearMenu();
            SearchView view;
            if (!panelContainer.Controls.ContainsKey("SearchView"))
            {
                view = new SearchView();
                panelContainer.Controls.Add(view);
            }
            else
                view = panelContainer.Controls["SearchView"] as SearchView;
            ActiveMenu(sender as Button, view);
        }

        private void btnFav_Click(object sender, EventArgs e)
        {
            ClearMenu();
            FavoriteView view;
            if (!panelContainer.Controls.ContainsKey("FavoriteView"))
            {
                view = new FavoriteView();
                panelContainer.Controls.Add(view);
            }
            else
                view = panelContainer.Controls["FavoriteView"] as FavoriteView;
            ActiveMenu(sender as Button, view);
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            ClearMenu();
            SettingView view;
            if (!panelContainer.Controls.ContainsKey("SettingView"))
            {
                view = new SettingView();
                panelContainer.Controls.Add(view);
            }
            else
                view = panelContainer.Controls["SettingView"] as SettingView;
            ActiveMenu(sender as Button, view);
        }

        public void btnKaraoke_Click(object sender, EventArgs e)
        {
            ClearMenu();
            KaraokeView view;
            if (!panelContainer.Controls.ContainsKey("KaraokeView"))
            {
                view = new KaraokeView();
                panelContainer.Controls.Add(view);
            }
            else
                view = panelContainer.Controls["KaraokeView"] as KaraokeView;
            ActiveMenu(btnKaraoke, view);
        }

        public void btnPlaylist_Click(object sender, EventArgs e)
        {
            ClearMenu();
            PlaylistView view;
            if (!panelContainer.Controls.ContainsKey("PlaylistView"))
            {
                view = new PlaylistView();
                panelContainer.Controls.Add(view);
            }
            else
                view = panelContainer.Controls["PlaylistView"] as PlaylistView;
            ActiveMenu(btnPlaylist, view);
        }
        #endregion

        #region List playing
        private void panelSongInfo_Click(object sender, EventArgs e)
        {
            tabPageMedia.Visible = !tabPageMedia.Visible;
            if (tabPageMedia.Visible)
                tabPageMedia.BringToFront();
        }

        public void NormalPlaying(int st = -1)
        {
            if (st != -1)
            {
                int old = playingOrder[st];
                playingOrder = Enumerable.Range(0, isPlaying.files.Count).Where(i => i != old).ToList();
                playingOrder.Insert(0, old);
            }
            else
                playingOrder = Enumerable.Range(0, isPlaying.files.Count).ToList();
        }
        public void ShufferPlaying(int st = -1)
        {
            if (st != -1)
            {
                int old = playingOrder[st];
                playingOrder = Enumerable.Range(0, isPlaying.files.Count).Where(i => i != old).ToList();
                Helper.Shuffle(playingOrder);
                playingOrder.Insert(0, old);
            }
            else
            {
                NormalPlaying();
                Helper.Shuffle(playingOrder);
            }
        }
        public Panel GetPanelPlaying()
        {
            return panelPlaying;
        }
        //public void UpdateLikedSong(int id)
        //{
        //    if (panelPlaying.Controls.Count > 0)
        //    {
        //        var mediaView = panelPlaying.Controls[id] as MediaList2;
        //        mediaView.UpdateLikedUI();
        //    }
        //}

        //public void UpdateLikedSong2(int id)
        //{
        //    var view = panelContainer.Controls["LocalView"] as LocalView;
        //    var ctls = view.GetPanelSong().Controls;
        //    if (ctls.Count > 0)
        //    {
        //        var mediaView = ctls[ctls.Count - 1 - id] as MediaList1;
        //        mediaView.UpdateLikedUI();
        //    }

        //}

        public void LoadViewPlaying()
        {
            panelPlaying.Controls.Clear();
            for (int i = playingOrder.Count - 1; i > -1; i--)
            {
                Song song = isPlaying.files[playingOrder[i]];
                var view = new MediaList2(song, playingOrder[i]);
                view.Dock = DockStyle.Top;
                panelPlaying.Controls.Add(view);
            }


        }
        #endregion

        #region Borderless form
        private int borderSize = 2;
        private Size formSize; //Keep form size when it is minimized and restored.Since the form is resized because it takes into account the size of the title bar and borders.
                               //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
        //Overridden methods
        protected override void WndProc(ref Message m)
        {
            const int WM_NCCALCSIZE = 0x0083;//Standar Title Bar - Snap Window
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020; //Minimize form (Before)
            const int SC_RESTORE = 0xF120; //Restore form (Before)
            const int WM_NCHITTEST = 0x0084;//Win32, Mouse Input Notification: Determine what part of the window corresponds to a point, allows to resize the form.
            const int resizeAreaSize = 10;


            // Resize/WM_NCHITTEST values
            const int HTCLIENT = 1; //Represents the client area of the window
            const int HTLEFT = 10;  //Left border of a window, allows resize horizontally to the left
            const int HTRIGHT = 11; //Right border of a window, allows resize horizontally to the right
            const int HTTOP = 12;   //Upper-horizontal border of a window, allows resize vertically up
            const int HTTOPLEFT = 13;//Upper-left corner of a window border, allows resize diagonally to the left
            const int HTTOPRIGHT = 14;//Upper-right corner of a window border, allows resize diagonally to the right
            const int HTBOTTOM = 15; //Lower-horizontal border of a window, allows resize vertically down
            const int HTBOTTOMLEFT = 16;//Lower-left corner of a window border, allows resize diagonally to the left
            const int HTBOTTOMRIGHT = 17;//Lower-right corner of a window border, allows resize diagonally to the right

            ///<Doc> More Information: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest </Doc>

            if (m.Msg == WM_NCHITTEST)
            { //If the windows m is WM_NCHITTEST
                base.WndProc(ref m);
                if (this.WindowState == FormWindowState.Normal)//Resize the form if it is in normal state
                {
                    if ((int)m.Result == HTCLIENT)//If the result of the m (mouse pointer) is in the client area of the window
                    {
                        Point screenPoint = new Point(m.LParam.ToInt32()); //Gets screen point coordinates(X and Y coordinate of the pointer)                           
                        Point clientPoint = this.PointToClient(screenPoint); //Computes the location of the screen point into client coordinates                          

                        if (clientPoint.Y <= resizeAreaSize)//If the pointer is at the top of the form (within the resize area- X coordinate)
                        {
                            if (clientPoint.X <= resizeAreaSize) //If the pointer is at the coordinate X=0 or less than the resizing area(X=10) in 
                                m.Result = (IntPtr)HTTOPLEFT; //Resize diagonally to the left
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize))//If the pointer is at the coordinate X=11 or less than the width of the form(X=Form.Width-resizeArea)
                                m.Result = (IntPtr)HTTOP; //Resize vertically up
                            else //Resize diagonally to the right
                                m.Result = (IntPtr)HTTOPRIGHT;
                        }
                        else if (clientPoint.Y <= (this.Size.Height - resizeAreaSize)) //If the pointer is inside the form at the Y coordinate(discounting the resize area size)
                        {
                            if (clientPoint.X <= resizeAreaSize)//Resize horizontally to the left
                                m.Result = (IntPtr)HTLEFT;
                            else if (clientPoint.X > (this.Width - resizeAreaSize))//Resize horizontally to the right
                                m.Result = (IntPtr)HTRIGHT;
                        }
                        else
                        {
                            if (clientPoint.X <= resizeAreaSize)//Resize diagonally to the left
                                m.Result = (IntPtr)HTBOTTOMLEFT;
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize)) //Resize vertically down
                                m.Result = (IntPtr)HTBOTTOM;
                            else //Resize diagonally to the right
                                m.Result = (IntPtr)HTBOTTOMRIGHT;
                        }
                    }
                }
                return;
            }


            //Remove border and keep snap window
            if (m.Msg == WM_NCCALCSIZE && m.WParam.ToInt32() == 1)
            {
                return;
            }

            //Keep form size when it is minimized and restored. Since the form is resized because it takes into account the size of the title bar and borders.
            if (m.Msg == WM_SYSCOMMAND)
            {
                /// <see cref="https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand"/>
                /// Quote:
                /// In WM_SYSCOMMAND messages, the four low - order bits of the wParam parameter 
                /// are used internally by the system.To obtain the correct result when testing 
                /// the value of wParam, an application must combine the value 0xFFF0 with the 
                /// wParam value by using the bitwise AND operator.
                int wParam = (m.WParam.ToInt32() & 0xFFF0);

                if (wParam == SC_MINIMIZE)  //Before
                    formSize = this.ClientSize;
                if (wParam == SC_RESTORE)// Restored form(Before)
                    this.Size = formSize;
            }
            base.WndProc(ref m);
        }
        private void AdjustForm()
        {
            switch (this.WindowState)
            {
                case FormWindowState.Maximized: //Maximized form (After)
                    this.Padding = new Padding(8, 8, 8, 8);
                    break;
                case FormWindowState.Normal: //Restored form (After)
                    if (this.Padding.Top != borderSize)
                        this.Padding = new Padding(borderSize);
                    break;
            }
        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            formSize = Size;
            this.WindowState = FormWindowState.Minimized;
        }
        private void btnMax_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                formSize = this.ClientSize;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.Size = formSize;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Data.SaveSetting();
            Data.SaveMyPlayList();
            Application.Exit();
        }
        private void MainForm_Resize(object sender, EventArgs e)
        {
            AdjustForm();
        }

        #endregion

        #region lyric
        private void HighlightLine(int idxLine)
        {
            if (idxLine > 0)
            {
                int f = rtbLyric.GetFirstCharIndexFromLine(idxLine - 1);
                int c = rtbLyric.GetLineFromCharIndex(f);
                string cur = rtbLyric.Lines[c];
                rtbLyric.Select(f, cur.Length);
                rtbLyric.SelectionColor = Color.White;
            }
            int firstcharindex = rtbLyric.GetFirstCharIndexFromLine(idxLine);
            int currentline = rtbLyric.GetLineFromCharIndex(firstcharindex);
            string currentlinetext = rtbLyric.Lines[currentline];
            rtbLyric.Select(firstcharindex, currentlinetext.Length);
            rtbLyric.SelectionColor = Color.FromArgb(255, 237, 0);
            rtbLyric.ScrollToCaret();
        }
        private async void LoadCurrentLyric()
        {
            rtbLyric.Text = "";
            if (isPlaying != null)
            {
                Lyric lyric = await isPlaying.files[playingOrder[counter]].GetLyric();
                if (lyric != null)
                {

                    if (lyric.hasLocalLyric)
                    {
                        rtbLyric.AppendText(lyric.localLyric);
                        return;
                    }
                    if (lyric.sentences != null)
                    {
                        foreach (var item in lyric.sentences)
                            rtbLyric.AppendText(item.fullSentence() + "\n");
                        rtbLyric.SelectAll();
                        rtbLyric.SelectionAlignment = HorizontalAlignment.Center;
                    }

                }
            }

        }
        private void timerLyric_Tick(object sender, EventArgs e)
        {
            if (isPlaying != null)
            {
                Lyric lyric = isPlaying.files[playingOrder[counter]].lyric;
                if (lyric != null && !lyric.hasLocalLyric)
                {
                    if (lyric.sentences.Count > curLyric)
                    {
                        var words = lyric.sentences[curLyric].words;
                        if (timeTrack >= words[0].startTime && timeTrack < words.Last().endTime)
                        {
                            HighlightLine(curLyric);
                        }
                        if (timeTrack >= words.Last().endTime)
                            curLyric++;
                    }

                }
            }
            timeTrack += timerLyric.Interval;
        }

        #endregion

        private void tabPageMedia_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabPageMedia.SelectedTab == tabPageMedia.TabPages["tabLyric"])
            {


            }
        }

        public Control GetContainerView(string name)
        {
            return panelContainer.Controls[name];
        }


    }
}
