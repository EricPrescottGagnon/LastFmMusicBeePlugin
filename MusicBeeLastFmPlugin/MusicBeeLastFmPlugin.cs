﻿using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using MusicBeePlugin.ReaderWriters;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();

        public static char[] filesSeparators = { '\0' };
        public static String PluginOutputDirectory = "LastFmPlugin";
        public static String OutputFile = "lastFmPlugin.txt";

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "LastFmPlayCount";
            about.Description = "Retrieve LastFm PlayCounts";
            about.Author = "Eric Prescott-Gagnon";
            about.TargetApplication = "";   // current only applies to artwork, lyrics or instant messenger name that appears in the provider drop down selector or target Instant Messenger
            about.Type = PluginType.General;
            about.VersionMajor = 1;  // your plugin version
            about.VersionMinor = 0;
            about.Revision = 1;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 0;   // height in pixels that musicbee should reserve in a panel for config settings. When set, a handle to an empty panel will be passed to the Configure function

            string directoryPath = mbApiInterface.Setting_GetPersistentStoragePath() + "\\" + Plugin.PluginOutputDirectory;
            Directory.CreateDirectory(directoryPath);

            string path = mbApiInterface.Setting_GetPersistentStoragePath() + "\\" + Plugin.PluginOutputDirectory + "\\" + Plugin.OutputFile;
            FileStream stream = new FileStream(path, FileMode.Create);
            
            return about;
        }

        public bool Configure(IntPtr panelHandle)
        {
            // save any persistent settings in a sub-folder of this path
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            // panelHandle will only be set if you set about.ConfigurationPanelHeight to a non-zero value
            // keep in mind the panel width is scaled according to the font the user has selected
            // if about.ConfigurationPanelHeight is set to 0, you can display your own popup window
            if (panelHandle != IntPtr.Zero)
            {
                Panel configPanel = (Panel)Panel.FromHandle(panelHandle);
                Label prompt = new Label();
                prompt.AutoSize = true;
                prompt.Location = new Point(0, 0);
                prompt.Text = "prompt:";
                TextBox textBox = new TextBox();
                textBox.Bounds = new Rectangle(60, 0, 100, textBox.Height);
                configPanel.Controls.AddRange(new Control[] { prompt, textBox });
            }
            return false;
        }
       
        // called by MusicBee when the user clicks Apply or Save in the MusicBee Preferences screen.
        // its up to you to figure out whether anything has changed and needs updating
        public void SaveSettings()
        {
            // save any persistent settings in a sub-folder of this path
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
        }

        // MusicBee is closing the plugin (plugin is being disabled by user or MusicBee is shutting down)
        public void Close(PluginCloseReason reason)
        {
        }

        // uninstall this plugin - clean up any persisted files
        public void Uninstall()
        {
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath() + "\\" + Plugin.PluginOutputDirectory;
            Directory.Delete(dataPath, true);            
        }

        public void Log(String message)
        {
            string path = mbApiInterface.Setting_GetPersistentStoragePath() + "\\" + Plugin.PluginOutputDirectory + "\\" + Plugin.OutputFile;

            FileStream stream = new FileStream(path, FileMode.Append);
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                streamWriter.WriteLine(DateTime.Now + " " + message);
            }
        }

        // receive event notifications from MusicBee
        // you need to set about.ReceiveNotificationFlags = PlayerEvents to receive all notifications, and not just the startup event
        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            // perform some action depending on the notification type
            switch (type)
            {
                case NotificationType.PluginStartup:

                    try
                    {
                        string lastFmUser = mbApiInterface.Setting_GetLastFmUserId();
                        mbApiInterface.MB_Trace("Last.fm user: " + lastFmUser);

                        DataStore dataStore = new DataStore();
                        LastFmReader lastFmReader = new LastFmReader(dataStore, lastFmUser);

                        MusicBeeReaderWriter mbReaderWriter = new MusicBeeReaderWriter(dataStore, mbApiInterface);
                        //mbReaderWriter.Read();

                        lastFmReader.ReadFromTopTracks();

                        string output = mbReaderWriter.Write();
                        this.Log(output);
                    }
                    catch (Exception e)
                    {
                        this.Log(e.Message);
                    }
                    break;
                case NotificationType.TrackChanged:
                    string artist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);
                    // ...
                    break;
            }
        }

        // return an array of lyric or artwork provider names this plugin supports
        // the providers will be iterated through one by one and passed to the RetrieveLyrics/ RetrieveArtwork function in order set by the user in the MusicBee Tags(2) preferences screen until a match is found
        public string[] GetProviders()
        {
            return null;
        }

        // return lyrics for the requested artist/title from the requested provider
        // only required if PluginType = LyricsRetrieval
        // return null if no lyrics are found
        public string RetrieveLyrics(string sourceFileUrl, string artist, string trackTitle, string album, bool synchronisedPreferred, string provider)
        {
            return null;
        }

        // return Base64 string representation of the artwork binary data from the requested provider
        // only required if PluginType = ArtworkRetrieval
        // return null if no artwork is found
        public string RetrieveArtwork(string sourceFileUrl, string albumArtist, string album, string provider)
        {
            //Return Convert.ToBase64String(artworkBinaryData)
            return null;
        }
   }
}