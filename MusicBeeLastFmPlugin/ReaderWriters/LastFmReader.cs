using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MusicBeePlugin.ReaderWriters
{
    class LastFmReader
    {
        const string uri = "http://ws.audioscrobbler.com/2.0/?method=";
        const string libraryGetTracks = "library.gettracks";
        const string trackGetInfo = "track.getinfo";

        const string userTopTracks = "user.gettoptracks";

        const string apiKeyString = "&api_key=";
        const string apiKey = "5649d896a3612b8869d799019e658b5a";

        const string userString = "&user=";
        const string userNameString = "&username=";

        public string user;

        public DataStore DataStore { get; private set; }

        public LastFmReader(DataStore dataStore, string userName)
        {
            this.DataStore = dataStore;
            this.user = userName;
        }

        public void Read()
        {
            System.Diagnostics.Stopwatch totalWatch = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch queryWatch = new System.Diagnostics.Stopwatch();

            StringBuilder sb = new StringBuilder();

            sb.Append(uri);
            sb.Append(libraryGetTracks);

            sb.Append(apiKeyString);
            sb.Append(apiKey);

            sb.Append(userString);
            sb.Append(user);

            sb.Append("&limit=100");

            string query = sb.ToString();

            List<Song> songs = new List<Song>();

            WebClient client = new WebClient();

            int page = 1;
            bool lastPage = false;

            totalWatch.Start();
            do
            {
                queryWatch.Start();
                Stream stream = client.OpenRead(query+"&page="+page);
                queryWatch.Stop();

                XElement element = XElement.Load(stream);

                if (element.Attribute("status").Value != "ok")
                {
                    string errorString = element.Elements().First().Value;
                    throw new Exception(errorString);
                }

                if (page.ToString() == element.Element("tracks").Attribute("totalPages").Value)
                {
                    lastPage = true;
                }

                IEnumerable<XElement> trackElements = element.Elements().First().Elements("track");

                foreach (XElement trackElement in trackElements)
                {
                    string trackName = trackElement.Element("name").Value.ToLowerInvariant();
                    int playCount = int.Parse(trackElement.Element("playcount").Value);

                    string artist = trackElement.Element("artist").Element("name").Value.ToLowerInvariant();
                    string album = trackElement.Element("album").Element("name").Value.ToLowerInvariant();

                    Song song = new Song(artist, album, trackName, playCount);
                    songs.Add(song);
                }

                page++;

            } while (!lastPage);

            totalWatch.Stop();

            Console.Write("Total time: " + totalWatch.Elapsed + " Query time : " + queryWatch.Elapsed);

            this.DataStore.AddSongs(songs);
        }

        public void ReadFromTopTracks()
        {
            System.Diagnostics.Stopwatch totalWatch = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch queryWatch = new System.Diagnostics.Stopwatch();

            StringBuilder sb = new StringBuilder();

            sb.Append(uri);
            sb.Append(userTopTracks);

            sb.Append(apiKeyString);
            sb.Append(apiKey);

            sb.Append(userString);
            sb.Append(user);

            sb.Append("&limit=200");

            string query = sb.ToString();

            List<Song> songs = new List<Song>();

            WebClient client = new WebClient();

            int page = 1;
            bool lastPage = false;

            totalWatch.Start();
            do
            {
                queryWatch.Start();
                Stream stream = client.OpenRead(query + "&page=" + page);
                queryWatch.Stop();

                XElement element = XElement.Load(stream);

                if (element.Attribute("status").Value != "ok")
                {
                    string errorString = element.Elements().First().Value;
                    throw new Exception(errorString);
                }

                if (page.ToString() == element.Element("toptracks").Attribute("totalPages").Value)
                {
                    lastPage = true;
                }

                IEnumerable<XElement> trackElements = element.Elements().First().Elements("track");

                foreach (XElement trackElement in trackElements)
                {
                    string trackName = trackElement.Element("name").Value.ToLowerInvariant();
                    int playCount = int.Parse(trackElement.Element("playcount").Value);

                    string artist = trackElement.Element("artist").Element("name").Value.ToLowerInvariant();
                    // string album = trackElement.Element("album").Element("name").Value.ToLowerInvariant();

                    Song song = new Song(artist, "", trackName, playCount);
                    songs.Add(song);
                }

                page++;

            } while (!lastPage);

            totalWatch.Stop();

            Console.Write("Total time: " + totalWatch.Elapsed + " Query time : " + queryWatch.Elapsed);

            this.DataStore.AddSongs(songs);
        }

        public void ReadFromDatastore()
        {
            System.Diagnostics.Stopwatch totalWatch = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch queryWatch = new System.Diagnostics.Stopwatch();

            StringBuilder sb = new StringBuilder();

            sb.Append(uri);
            sb.Append(trackGetInfo);

            sb.Append(apiKeyString);
            sb.Append(apiKey);

            sb.Append(userNameString);
            sb.Append(user);

            string start = sb.ToString();
            WebClient client = new WebClient();

            totalWatch.Start();

            foreach (Dictionary<string, Dictionary<string, Song>> songByTitlesByAlbums in this.DataStore.OrderedSongs.Values)
            {
                foreach (Dictionary<string, Song> songByTitles in songByTitlesByAlbums.Values)
                {
                    foreach (Song song in songByTitles.Values)
                    {
                        StringBuilder songSb = new StringBuilder(start);

                        songSb.Append("&artist=");
                        songSb.Append(song.Artist);

                        songSb.Append("&track=");
                        songSb.Append(song.SongTitle);

                        queryWatch.Start();
                        Stream stream = client.OpenRead(songSb.ToString());
                        queryWatch.Stop();

                        XElement element = XElement.Load(stream);
                    }
                }
            }

            totalWatch.Stop();
            Console.Write("Total time: " + totalWatch.Elapsed + " Query time : " + queryWatch.Elapsed);
        }
    }
}
