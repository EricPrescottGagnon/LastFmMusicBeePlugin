using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBeePlugin
{
    public class Song
    {
        public string Artist { get; private set; }
        public string Album { get; private set; }
        public string SongTitle { get; private set; }
        public int PlayCount { get; set; }

        public Song(string artist, string album, string songTitle, int PlayCount)
        {
            this.Artist = artist;
            this.Album = album;
            this.SongTitle = songTitle;
            this.PlayCount = PlayCount;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.Artist + " - ");
            sb.Append(this.SongTitle + ", ");
            sb.Append(this.Album + " ");
            sb.Append(this.PlayCount);

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            bool isEqual = false;
            if (obj is Song)
            {
                Song otherSong = obj as Song;

                if (this.SongTitle == otherSong.SongTitle &&
                    this.Artist == otherSong.Artist &&
                    this.Album == otherSong.Album)
                {
                    isEqual = true;
                }
            }

            return isEqual;
        }

        public override int GetHashCode()
        {
            return this.Artist.GetHashCode() ^ this.SongTitle.GetHashCode() ^ this.Album.GetHashCode();
        }
    }
}
