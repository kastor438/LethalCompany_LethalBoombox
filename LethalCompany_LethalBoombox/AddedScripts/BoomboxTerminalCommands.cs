using LCAPI.TerminalCommands.Attributes;
using System;
using UnityEngine;

namespace LethalCompany_LethalBoombox.AddedScripts
{
    internal class BoomboxTerminalCommands : MonoBehaviour
    {
        public static BoomboxTerminalCommands Instance;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            RegisterBoomboxCommands();
        }

        public void RegisterBoomboxCommands()
        {
            LethalBoomboxBase.Instance.RegisterCommands(this);
            LethalBoomboxBase.Instance.mls.LogInfo("Registered Boombox Commands.");
        }

        // Song Commands
        [TerminalCommand("song", true)]
        public string SongCommand(Terminal caller)
        {
            SpotifyAPI.Instance.SetSongInput("");
            string songSetOuput = "Song Reset.\n";
            songSetOuput = AppendPrefixNewLines(songSetOuput);
            return songSetOuput;
        }
        [TerminalCommand("song", true)]
        public string SongCommand(Terminal caller, [RemainingText] string songInput)
        {
            SpotifyAPI.Instance.SetSongInput(songInput);
            if (SpotifyAPI.Instance != null && SpotifyAPI.Instance.searchCriteriaChanged)
            {
                SpotifyAPI.Instance.SearchSpotify();
            }

            string songSetOuput = string.Format("Song Set: {0}\n", songInput);
            songSetOuput = AppendPrefixNewLines(songSetOuput);
            return songSetOuput;
        }

        // Artist Commands
        [TerminalCommand("artist", true)]
        public string ArtistCommand(Terminal caller)
        {
            SpotifyAPI.Instance.SetArtistInput("");
            string artistSetOuput = "Artist Reset.\n";
            artistSetOuput = AppendPrefixNewLines(artistSetOuput);
            return artistSetOuput;
        }
        [TerminalCommand("artist", true)]
        public string ArtistCommand(Terminal caller, [RemainingText] string artistInput)
        {
            SpotifyAPI.Instance.SetArtistInput(artistInput);
            SpotifyAPI.Instance.SetTrackOptions();

            string artistSetOuput = string.Format("Artist Set: {0}\n", artistInput);
            artistSetOuput = AppendPrefixNewLines(artistSetOuput);
            return artistSetOuput;
        }

        // Fetch Command
        [TerminalCommand("fetch", true)]
        public string FetchCommand(Terminal caller)
        {
            string spotifySongsOutput;
            //mls.LogInfo(string.Format("SpotifyAPI.Instance == null: {0}\nSpotifyAPI.Instance.searchCriteriaChanged: {1}\n", (SpotifyAPI.Instance == null).ToString(), SpotifyAPI.Instance.searchCriteriaChanged));
            
            if (SpotifyAPI.Instance.GetSongInput().Length == 0)
            {
                spotifySongsOutput = "Set a song name to search.\n";
            }
            else
            {
                spotifySongsOutput = SpotifyAPI.Instance.FormatSpotifyTracksTerminalOutput();
            }

            spotifySongsOutput = AppendPrefixNewLines(spotifySongsOutput);
            return spotifySongsOutput;
        }

        // Track Command
        [TerminalCommand("track", true)]
        public string TrackCommand(Terminal caller, [RemainingText] string trackInput)
        {
            string spotifyTracksOutput;

            try
            {
                int trackNumber = int.Parse(trackInput);
                if (trackNumber > 0 && trackNumber <= 10 &&
                    (Array.IndexOf(SpotifyAPI.Instance.GetArtistFilteredSpotifyTracks(), null) == -1 || 
                    trackNumber <= Array.IndexOf(SpotifyAPI.Instance.GetArtistFilteredSpotifyTracks(), null)))
                {
                    SpotifyAPI.Instance.GetTrackAsAudioClip(trackNumber - 1);
                    spotifyTracksOutput = string.Format("Track {0} added to boombox.\n", trackInput);
                }
                else
                {
                    spotifyTracksOutput = "Invalid track number.\n";
                }
            }
            catch (Exception exception)
            {
                LethalBoomboxBase.Instance.mls.LogInfo(exception.Message);
                spotifyTracksOutput = "Invalid track number.\n";
            }
            spotifyTracksOutput = AppendPrefixNewLines(spotifyTracksOutput);
            return spotifyTracksOutput;
        }

        public string AppendPrefixNewLines(string terminalOutput)
        {
            return "\t\n\n\n" + terminalOutput;
        }
    }
}
