using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicBeePlugin.ReaderWriters
{
    class MusicBeeReaderWriter
    {
        public DataStore DataStore { get; private set; }
        public Plugin.MusicBeeApiInterface MbApiInterface { get; private set; }

        public MusicBeeReaderWriter(DataStore dataStore, Plugin.MusicBeeApiInterface mbApiInterface)
        {
            this.DataStore = dataStore;
            this.MbApiInterface = mbApiInterface;
        }

        public void Read()
        {

            if (MbApiInterface.Library_QueryFiles("domain=DisplayedFiles"))
            {
                List<Song> songs = new List<Song>();

                string[] files = MbApiInterface.Library_QueryGetAllFiles().Split(Plugin.filesSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string file in files)
                {
                    //string playCount = this.MbApiInterface.Library_GetFileProperty(file, Plugin.FilePropertyType.PlayCount);
                    string album = this.MbApiInterface.Library_GetFileTag(file, Plugin.MetaDataType.Album).ToLowerInvariant();
                    string trackTitle = this.MbApiInterface.Library_GetFileTag(file, Plugin.MetaDataType.TrackTitle).ToLowerInvariant();
                    string artist = this.MbApiInterface.Library_GetFileTag(file, Plugin.MetaDataType.Artist).ToLowerInvariant();

                    Song song = new Song(artist, album, trackTitle, 0);
                    songs.Add(song);
                }

                this.DataStore.AddSongs(songs);
            }
        }

        public string Write()
        {
            StringBuilder output = new StringBuilder();
 
            if (MbApiInterface.Library_QueryFiles("domain=DisplayedFiles"))
            {
                HashSet<Song> matchedSongs = new HashSet<Song>();

                string[] files = MbApiInterface.Library_QueryGetAllFiles().Split(Plugin.filesSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string file in files)
                {
                    int oldPlayCount = int.Parse(this.MbApiInterface.Library_GetFileProperty(file, Plugin.FilePropertyType.PlayCount));
                    string album = this.MbApiInterface.Library_GetFileTag(file, Plugin.MetaDataType.Album).ToLowerInvariant();
                    string trackTitle = this.MbApiInterface.Library_GetFileTag(file, Plugin.MetaDataType.TrackTitle).ToLowerInvariant();
                    string artist = this.MbApiInterface.Library_GetFileTag(file, Plugin.MetaDataType.Artist).ToLowerInvariant();

                    int newPlayCount;
                    if (this.DataStore.TryGetPlayCount(artist, album, trackTitle, out newPlayCount))
                    {
                        if (newPlayCount != oldPlayCount)
                        {
                            this.MbApiInterface.Library_SetFileTag(file, (Plugin.MetaDataType)Plugin.FilePropertyType.PlayCount, newPlayCount.ToString());
                        }

                        Song song = new Song(artist, album, trackTitle, 0);
                        matchedSongs.Add(song);
                    }
                }

                foreach (string file in files)
                {
                    this.MbApiInterface.Library_CommitTagsToFile(file);
                }


                this.MbApiInterface.MB_RefreshPanels();

                Dictionary<string, Dictionary<string, List<Song>>> unmatchedSongs = new Dictionary<string, Dictionary<string, List<Song>>>();
                foreach (Dictionary<string, Dictionary<string, Song>> songsBySomething in this.DataStore.OrderedSongs.Values)
                {
                    foreach (Dictionary<string, Song> songsBySomethingElse in songsBySomething.Values)
                    {
                        foreach (Song song in songsBySomethingElse.Values)
                        {
                            if (!matchedSongs.Contains(song))
                            {
                                unmatchedSongs.TryGetAddValue(song.Artist).TryGetAddValue(song.Album).Add(song);
                            }
                        }
                    }
                }

                foreach (KeyValuePair<string, Dictionary<string, List<Song>>> unmatchedSongsByArtist in unmatchedSongs)
                {
                    string artist = unmatchedSongsByArtist.Key;
                    foreach (KeyValuePair<string, List<Song>> unmatchedSongsByAlbum in unmatchedSongsByArtist.Value)
                    {
                        string album = unmatchedSongsByAlbum.Key;
                        int songCount = unmatchedSongsByAlbum.Value.Count;

                        if (songCount == this.DataStore.getNumberOfSongsInAlbum(artist, album))
                        {
                            output.Append(artist);
                            output.Append(" - * (" + album + ")");
                            output.Append(Environment.NewLine);
                        }
                        else
                        {
                            foreach (Song song in unmatchedSongsByAlbum.Value)
                            {
                                output.Append(song.Artist);
                                output.Append(" - ");
                                output.Append(song.SongTitle);
                                output.Append(" (" + song.Album + ")");
                                output.Append(Environment.NewLine);
                            }
                        }
                    }
                }
            }
            return output.ToString();
        }
    }
}
