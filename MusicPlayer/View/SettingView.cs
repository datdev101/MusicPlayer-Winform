﻿using MusicPlayer.DataRepo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayer.View
{
    public partial class SettingView : UserControl
    {
        string basePath = Environment.SpecialFolder.MyMusic.ToString() + "/dplayer";

        public SettingView()
        {
            InitializeComponent();
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            var f = new FolderBrowserDialog();
            if (f.ShowDialog() == DialogResult.OK)
            {
                cbLocal.Items.Add(f.SelectedPath);
                cbLocal.SelectedIndex = cbLocal.Items.Count - 1;
                Data.localPath.Add(f.SelectedPath);
                var view = MainForm.Instance.GetContainerView("LocalView");
                if (view != null)
                    (view as LocalView).LocalView_Load(null, null);
            }
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            if (cbLocal.Items.Count > 0)
            {
                Data.localPath.RemoveAt(cbLocal.SelectedIndex);
                cbLocal.Items.RemoveAt(cbLocal.SelectedIndex);
                var view = MainForm.Instance.GetContainerView("LocalView");
                if (view != null)
                    (view as LocalView).LocalView_Load(null, null);
            }
        }
    }
}