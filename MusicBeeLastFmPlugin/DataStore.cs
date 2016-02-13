using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBeePlugin
{
    public class DataStore
    {
        public SortedDictionary<string, Dictionary<string, Dictionary<string, Song>>> OrderedSongs { get; private set; }

        public DataStore()
        {
            this.OrderedSongs = new SortedDictionary<string, Dictionary<string, Dictionary<string, Song>>>();
        }

        public void AddSongs(List<Song> songs)
        {
            List<Song> unidentifiedSongs = new List<Song>();
            foreach (Song song in songs)
            {
                bool identified = false;
                Dictionary<string, Dictionary<string, Song>> songByTitleByAlbum;
                if (this.OrderedSongs.TryGetValue(song.Artist, out songByTitleByAlbum))
                {
                    Dictionary<string, Song> songByTitle;
                    if (songByTitleByAlbum.TryGetValue(song.Album, out songByTitle))
                    {
                        Song previousSong;
                        if (songByTitle.TryGetValue(song.SongTitle, out previousSong))
                        {
                            identified = true;
                            previousSong.PlayCount += song.PlayCount;
                        }
                    }
                }

                if (!identified)
                {
                    unidentifiedSongs.Add(song);
                }
            }

            foreach (Song unidentifiedSong in unidentifiedSongs)
            {
                this.OrderedSongs.TryGetAddValue(unidentifiedSong.Artist)
                                 .TryGetAddValue(unidentifiedSong.Album)
                                 .Add(unidentifiedSong.SongTitle, unidentifiedSong);
            }
        }

        public int getNumberOfSongsInAlbum(string artist, string album)
        {
            int numberOfSongs = 0;

            Dictionary<string, Dictionary<string, Song>> songByTitleByAlbum;
            if (this.OrderedSongs.TryGetValue(artist, out songByTitleByAlbum))
            {
                Dictionary<string, Song> songByTitle;
                if (songByTitleByAlbum.TryGetValue(album, out songByTitle))
                {
                    numberOfSongs = songByTitle.Count;
                }
            }

            return numberOfSongs;
        }

        public bool TryGetPlayCount(string artist, string album, string title, out int playCount)
        {
            playCount = 0;
            bool isAvailable = false;

            Dictionary<string, Dictionary<string, Song>> songByTitleByAlbum;
            if (this.OrderedSongs.TryGetValue(artist, out songByTitleByAlbum))
            {
                Dictionary<string, Song> songByTitle;
                if (songByTitleByAlbum.TryGetValue(album, out songByTitle) || songByTitleByAlbum.TryGetValue("", out songByTitle))
                {
                    Song previousSong;
                    if (songByTitle.TryGetValue(title, out previousSong))
                    {
                        isAvailable = true;
                        playCount = previousSong.PlayCount;
                    }
                }
            }

            return isAvailable;
        }
    }
}
