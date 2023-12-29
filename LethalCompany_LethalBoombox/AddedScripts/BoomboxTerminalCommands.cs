using LCAPI.TerminalCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            LethalBoomboxBase.Instance.RegisterCommands(this);
        }

        // Song Commands
        [TerminalCommand("song", true)]
        public string SongCommand(Terminal caller)
        {
            string songSetOuput = "Song Reset.\n";
            SpotifyAPI.Instance.SetSongInput("");
            songSetOuput = AppendPrefixNewLines(songSetOuput);
            return songSetOuput;
        }
        [TerminalCommand("song", true)]
        public string SongCommand(Terminal caller, [RemainingText] string songInput)
        {
            string songSetOuput = string.Format("Song Set: {0}\n", songInput);
            SpotifyAPI.Instance.SetSongInput(songInput);
            songSetOuput = AppendPrefixNewLines(songSetOuput);
            return songSetOuput;
        }

        // Artist Commands
        [TerminalCommand("artist", true)]
        public string ArtistCommand(Terminal caller)
        {
            string artistSetOuput = "Artist Reset.\n";
            SpotifyAPI.Instance.SetArtistInput("");
            artistSetOuput = AppendPrefixNewLines(artistSetOuput);
            return artistSetOuput;
        }
        [TerminalCommand("artist", true)]
        public string ArtistCommand(Terminal caller, [RemainingText] string artistInput)
        {
            string artistSetOuput = string.Format("Artist Set: {0}\n", artistInput);
            SpotifyAPI.Instance.SetArtistInput(artistInput);
            artistSetOuput = AppendPrefixNewLines(artistSetOuput);
            return artistSetOuput;
        }

        // Fetch Command
        [TerminalCommand("fetch", true)]
        public string FetchCommand(Terminal caller)
        {
            string spotifySongsOutput;
            //mls.LogInfo(string.Format("SpotifyAPI.Instance == null: {0}\nSpotifyAPI.Instance.searchCriteriaChanged: {1}\n", (SpotifyAPI.Instance == null).ToString(), SpotifyAPI.Instance.searchCriteriaChanged));
            if (SpotifyAPI.Instance != null && SpotifyAPI.Instance.searchCriteriaChanged && SpotifyAPI.Instance.GetSongInput().Length > 0)
            {
                string fetchedSongsTerminalOutput = SpotifyAPI.Instance.SearchSpotify();
                spotifySongsOutput = fetchedSongsTerminalOutput;
            }
            else if (SpotifyAPI.Instance.GetSongInput().Length == 0)
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
                if (trackNumber > 0 && trackNumber <= Array.IndexOf(SpotifyAPI.Instance.GetArtistFilteredSpotifyTracks(), null))
                {
                    LethalBoomboxBase.Instance.mls.LogInfo(string.Format("trackNumber: {0}", trackNumber));
                    LethalBoomboxBase.Instance.mls.LogInfo(string.Format("SpotifyAPI.Instance.GetArtistFilteredSpotifyTracks().Length: {0}", SpotifyAPI.Instance.GetArtistFilteredSpotifyTracks().Length));
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
