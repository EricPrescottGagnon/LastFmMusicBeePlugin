using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBeePlugin;
using MusicBeePlugin.ReaderWriters;


namespace MusicBeeLastFmExe
{
    class Program
    {
        static void Main(string[] args)
        {
            string userName = "";

            DataStore dataStore = new DataStore();

            LastFmWebScraper webScraper = new LastFmWebScraper(userName);
            webScraper.GetPlayCounts("alt-J", "An Awesome Wave", new List<string>() { "Intro" });
        }
    }
}
