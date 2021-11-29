﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Model
{
    public class Artist
    {
        public string id;
        public string name;
        public object thumbnail;

        public Artist(string id, string name, object thumbnail)
        {
            this.id = id;
            this.name = name;
            this.thumbnail = thumbnail;
        }
    }
}
