using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicBeePlugin.ReaderWriters
{
    public class LastFmWebScraper
    {
        // Example: http://www.last.fm/user/PrescottGagnon/library/music/alt-J/This+Is+All+Yours?date_preset=ALL

        string url;

        public LastFmWebScraper(string lastFmUserName)
        {
            this.url = "http://www.last.fm/user" + lastFmUserName + "library/music/";
        }

        public void GetPlayCounts(string artist, string album, List<string> tracks)
        {
            // get the playcounts for those tracks!

            StringBuilder sb = new StringBuilder();
            sb.Append(this.url);
            sb.Append(artist); // We would probably need to do something about spaces in names, special characters, etc.
            sb.Append("/");
            sb.Append(album); // We would probably need to do something about spaces in names, special characters, etc.
            sb.Append("?date_preset=All");

            sb.ToString(); 
        }
    }
}
