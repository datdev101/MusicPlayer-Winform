using MusicPlayer.Model;
using MusicPlayer.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MusicPlayer.View
{
    public partial class MediaList2 : UserControl
    {
        public Song song;
        public int idx;
        public CheckBox Box { get => checkBox1; }
        public MediaList2()
        {
            InitializeComponent();
        }

        public MediaList2(Song song, int idx)
        {
            InitializeComponent();
            this.song = song;
            this.idx = idx;
            if (MainForm.Instance.playingOrder != null)
            {
                if (song == MainForm.Instance.isPlaying.files[MainForm.Instance.playingOrder[MainForm.Instance.counter]])
                    ActiveSongUI();
            }


        }
        private async void MediaList2_Load(object sender, EventArgs e)
        {
            lbName.Text = song.GetTitle();
            lbArtist.Text = song.GetArtistNameJoined();
            lbAlbum.Text = song.GetAlbumName();
            lbDuration.Text = song.GetFormatedDuration();
            if (song.GetThumbImg() is Image)
            {
                pbImg.BackgroundImage = song.GetThumbImg() as Image;
                pbImg.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else if (song.GetThumbImg() is string)
            {
                pbImg.BackgroundImage = await Helper.LoadImagefromURL(song.GetThumbImg().ToString());
                pbImg.BackgroundImageLayout = ImageLayout.Stretch;
            }
            if (song.GetTitle() == "" || song.GetTitle() == null)
            {
                lbName.Text = Path.GetFileNameWithoutExtension(song.GetSrc());
            }
            //if (song.isLiked)
            //    pbTim.BackgroundImage = Properties.Resources.icons8_heart_24px;
            //else
            //    pbTim.BackgroundImage = Properties.Resources.icons8_love_24px;
        }

        public void ActiveSongUI()
        {
            foreach (Control c in tableLayoutPanel1.Controls)
                c.BackColor = Color.FromArgb(15, 15, 16);
            ChangeColorLabel(Color.FromArgb(1, 241, 250));
        }

        public void DeActiveSongUI()
        {

            foreach (Control c in tableLayoutPanel1.Controls)
                c.BackColor = Color.FromArgb(37, 38, 44);
            ChangeColorLabel(Color.White);
        }

        private void ChangeColorLabel(Color color)
        {
            lbName.ForeColor = color;
            lbArtist.ForeColor = color;
            lbDuration.ForeColor = color;
            lbAlbum.ForeColor = color;
        }

        private void pbImg_Click(object sender, EventArgs e)
        {
            if (MainForm.Instance.isPlaying != null)
            {
                int old = MainForm.Instance.playingOrder[MainForm.Instance.counter];
                var ctls = MainForm.Instance.GetPanelPlaying().Controls;
                (ctls[ctls.Count - 1 - old] as MediaList2).DeActiveSongUI();
            }
            MainForm.Instance.counter = idx;
            MainForm.Instance.PlayMedia();


            ActiveSongUI();
        }

        private void pbImg_MouseEnter(object sender, EventArgs e)
        {
            pbImg.Image = Properties.Resources.icons8_play_32px;
        }

        private void pbImg_MouseLeave(object sender, EventArgs e)
        {
            pbImg.Image = null;
        }

        //private void pbTim_Click(object sender, EventArgs e)
        //{
        //    song.isLiked = !song.isLiked;
        //    UpdateLikedUI();
        //    if (MainForm.Instance.playingOrder != null)
        //    {
        //        MainForm.Instance.UpdateLikedSong2(MainForm.Instance.playingOrder[idx]);
        //    }
        //    if (FavoriteView.Instance != null)
        //    {
        //        FavoriteView.Instance.ReloadView();
        //    }
        //}
        //public void UpdateLikedUI()
        //{
        //    if (song.isLiked)
        //    {
        //        MainForm.Instance.favorite.files.Add(song);
        //        pbTim.BackgroundImage = Properties.Resources.icons8_heart_24px;
        //    }
        //    else
        //    {
        //        MainForm.Instance.favorite.files.Remove(song);
        //        pbTim.BackgroundImage = Properties.Resources.icons8_love_24px;
        //    }
        //}
    }
}
