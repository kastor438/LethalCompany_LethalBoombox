using System;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using static TerminalApi.TerminalApi;
using TerminalApi.Events;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Security.Policy;


namespace LethalCompany_LethalBoombox.AddedScripts
{
    internal class SpotifyAPI : MonoBehaviour
    {
        public static SpotifyAPI Instance { get; private set; }
        public TerminalKeyword SongInputKeyword;
        public string SongInput;
        public TerminalKeyword ArtistInputKeyword;
        public string ArtistInput;
        public AudioClip[] spotifyAudioClips;
        public bool firstFetch = true;
        public bool searchCriteriaChanged = false;
        public bool invalidInput = false;

        private readonly string clientID = "17dfce76674e4117896a74272baa27a7";
        private readonly string clientSecret = "b71348dcd069404bba15f209537af959";
        private static SpotifyAuthResult spotifyAuthResult;
        private static SpotifyResults spotifyResults;
        private static Item[] artistFilteredSpotifyTracks = new Item[10];

        public class SpotifyAuthResult
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
        }


        public class SpotifyResults
        {
            public Tracks tracks { get; set; }
        }

        public class Tracks
        {
            public string href { get; set; }
            public Item[] items { get; set; }
            public int limit { get; set; }
            public string next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
        }

        public class Item
        {
            public Album album { get; set; }
            public Artist[] artists { get; set; }
            public string[] available_markets { get; set; }
            public int disc_number { get; set; }
            public int duration_ms { get; set; }
            public bool _explicit { get; set; }
            public External_Ids external_ids { get; set; }
            public External_Urls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public bool is_local { get; set; }
            public string name { get; set; }
            public int popularity { get; set; }
            public string preview_url { get; set; }
            public int track_number { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }

        public class Album
        {
            public string album_type { get; set; }
            public Artist[] artists { get; set; }
            public string[] available_markets { get; set; }
            public External_Urls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public Image[] images { get; set; }
            public string name { get; set; }
            public string release_date { get; set; }
            public string release_date_precision { get; set; }
            public int total_tracks { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }

        public class External_Urls
        {
            public string spotify { get; set; }
        }

        public class Artist
        {
            public External_Urls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }

        public class Image
        {
            public int height { get; set; }
            public string url { get; set; }
            public int width { get; set; }
        }

        public class External_Ids
        {
            public string isrc { get; set; }
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            StartCoroutine(InitializeSpotify());
            TerminalKeyword songKeyword = CreateTerminalKeyword("song", true);
            TerminalKeyword artistKeyword = CreateTerminalKeyword("artist", true);
            TerminalKeyword trackKeyword = CreateTerminalKeyword("track", true);
            AddTerminalKeyword(trackKeyword);

            AddTerminalKeyword(songKeyword);
            AddTerminalKeyword(artistKeyword);
            AddCommand("fetch", "No song information set.\n", null, true);

            SongInputKeyword = null;
            ArtistInputKeyword = null;
            SongInput = "";
            ArtistInput = "";

            spotifyAudioClips = new AudioClip[0];
        }

        private IEnumerator InitializeSpotify()
        {
            WWWForm form = new WWWForm();

            using (UnityWebRequest webRequest = UnityWebRequest.Post("https://accounts.spotify.com/api/token", string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}", clientID, clientSecret), "application/x-www-form-urlencoded"))
            {
                yield return webRequest.SendWebRequest();
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Connection Error: {0}", webRequest.error));
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Data Processing Error: {0}", webRequest.error));
                        break;
                    case UnityWebRequest.Result.Success:
                        LethalBoomboxBase.Instance.mls.LogInfo(webRequest.downloadHandler.text);
                        spotifyAuthResult = JsonConvert.DeserializeObject<SpotifyAuthResult>(webRequest.downloadHandler.text);
                        SetInitialSpotifySongs();
                        break;
                    default:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("API Error: {0}", webRequest.error));
                        break;
                }
            }
        }

        public Item[] GetArtistFilteredSpotifyTracks()
        {
            return artistFilteredSpotifyTracks;
        }

        private void SetInitialSpotifySongs()
        {
            string[] audioClipURLs = { "https://p.scdn.co/mp3-preview/3c07483a9f0401cffb46d104b5cb1b30853e7767?cid=17dfce76674e4117896a74272baa27a7" };

            for (int i = 0; i < audioClipURLs.Length; i++)
            {
                GetTrackAsAudioClip(audioClipURLs[i]);
            }
        }

        public void SearchSpotify()
        {
            LethalBoomboxBase.Instance.mls.LogInfo(string.Format("SearchSpotify, SongInput: {0}", SongInput));
            string uri = string.Format("https://api.spotify.com/v1/search?q={0}&type=track&limit=50", SongInput);
            StartCoroutine(FetchSpotifySongs(uri));
            searchCriteriaChanged = false;
        }

        private IEnumerator FetchSpotifySongs(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", string.Format("Bearer {0}", spotifyAuthResult.access_token));

                yield return webRequest.SendWebRequest();
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Connection Error: {0}", webRequest.error));
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Data Processing Error: {0}", webRequest.error));
                        break;
                    case UnityWebRequest.Result.Success:
                        LethalBoomboxBase.Instance.mls.LogInfo(webRequest.downloadHandler.text);
                        spotifyResults = JsonConvert.DeserializeObject<SpotifyResults>(webRequest.downloadHandler.text);

                        string terminalOutput = FormatSpotifyTracksTerminalOutput(spotifyResults);
                        terminalOutput = UnescapeString(terminalOutput);
                        if (terminalOutput == "")
                            terminalOutput = "No tracks found with search.\n";
                        AddCommand("fetch", string.Format("Fetching songs...\n{0}", terminalOutput), null, true);
                        break;
                    default:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("API Error: {0}", webRequest.error));
                        break;
                }
            }
        }

        public void GetTrackAsAudioClip(int trackNumber)
        {
            StartCoroutine(GetTrackAsAudioClip_WebRequest(trackNumber));
        }

        public void GetTrackAsAudioClip(string trackURL)
        {
            StartCoroutine(GetTrackAsAudioClip_WebRequest(trackURL));
        }

        private IEnumerator GetTrackAsAudioClip_WebRequest(int trackNumber)
        {
            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(artistFilteredSpotifyTracks[trackNumber].preview_url, AudioType.MPEG))
            {
                //webRequest.SetRequestHeader("Content-Type", "application/json");
                //webRequest.SetRequestHeader("Authorization", string.Format("Bearer {0}", spotifyAuthResult.access_token));
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("GetTrackAsAudioClip Connection Error: {0}", webRequest.error));
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("GetTrackAsAudioClip Data Processing Error: {0}", webRequest.error));
                        break;
                    case UnityWebRequest.Result.Success:
                        AudioClip newSpotifyAudioClip = DownloadHandlerAudioClip.GetContent(webRequest);
                        AddAudioClip(newSpotifyAudioClip);
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Adding new track: {0}", spotifyResults.tracks.items[trackNumber].name));
                        break;
                    default:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("GetTrackAsAudioClip API Error: {0}", webRequest.error));
                        break;
                }
            }
        }

        private IEnumerator GetTrackAsAudioClip_WebRequest(string trackURL)
        {
            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(trackURL, AudioType.MPEG))
            {
                //webRequest.SetRequestHeader("Content-Type", "application/json");
                //webRequest.SetRequestHeader("Authorization", string.Format("Bearer {0}", spotifyAuthResult.access_token));
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("GetTrackAsAudioClip Connection Error: {0}", webRequest.error));
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("GetTrackAsAudioClip Data Processing Error: {0}", webRequest.error));
                        break;
                    case UnityWebRequest.Result.Success:
                        AudioClip newSpotifyAudioClip = DownloadHandlerAudioClip.GetContent(webRequest);
                        AddAudioClip(newSpotifyAudioClip);
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Adding initial track url: {0}", trackURL));
                        break;
                    default:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("GetTrackAsAudioClip API Error: {0}", webRequest.error));
                        break;
                }
            }
        }

        public void AddAudioClip(AudioClip newSpotifyAudioClip)
        {
            if (spotifyAudioClips.Length < 10)
            {
                Array.Resize(ref spotifyAudioClips, spotifyAudioClips.Length + 1);
                spotifyAudioClips[spotifyAudioClips.Length - 1] = newSpotifyAudioClip;
            }
            else
            {
                for (int i = 1; i < spotifyAudioClips.Length; i++)
                {
                    spotifyAudioClips[i - 1] = spotifyAudioClips[i];
                }
                spotifyAudioClips[9] = newSpotifyAudioClip;
            }
        }

        public string FormatSpotifyTracksTerminalOutput(SpotifyResults spotifyResults)
        {
            string terminalOutput = "";
            int trackCount = 0;
            artistFilteredSpotifyTracks = new Item[10];
            for (int i = 0; i < spotifyResults.tracks.items.Length; i++)
            {
                if (spotifyResults.tracks.items[i] != null)
                {
                    string trackArtistNames = spotifyResults.tracks.items[i].artists[0].name;
                    for (int j = 1; j < spotifyResults.tracks.items[i].artists.Length; j++)
                    {
                        trackArtistNames = string.Format("{0}, {1}", trackArtistNames, spotifyResults.tracks.items[i].artists[j].name);
                    }
                    if (trackArtistNames.ToLower().Contains(ArtistInput.ToLower()))
                    {
                        terminalOutput = string.Format("{0}Track {1}: {2}, By: {3}\n", terminalOutput, (trackCount + 1).ToString(),
                            spotifyResults.tracks.items[i].name, trackArtistNames);
                        artistFilteredSpotifyTracks[trackCount] = spotifyResults.tracks.items[i];
                        trackCount++;
                    }
                }
                if (trackCount >= 10)
                    break;
            }

            if (firstFetch == true)
            {
                TerminalKeyword trackKeyword = GetKeyword("track");
                for (int i = 0; i < 10; i++)
                {
                    DeleteKeyword((i+1).ToString());
                    if (i < trackCount)
                    {
                        TerminalKeyword trackNumberKeyword = CreateTerminalKeyword(string.Format("{0}", (i + 1).ToString()));
                        AddTerminalKeyword(trackNumberKeyword);
                        TerminalNode trackTriggerNode = CreateTerminalNode(string.Format("Track {0} added to boombox.\n", (i + 1).ToString()), true);
                        AddCompatibleNoun(trackKeyword, trackNumberKeyword.word, trackTriggerNode);
                    }
                    else
                    {
                        TerminalKeyword trackNumberKeyword = CreateTerminalKeyword(string.Format("{0}", (i + 1).ToString()));
                        AddTerminalKeyword(trackNumberKeyword);
                        TerminalNode trackTriggerNode = CreateTerminalNode(string.Format("Track {0} is an invalid track.\n", (i + 1).ToString()), true);
                        AddCompatibleNoun(trackKeyword, trackNumberKeyword.word, trackTriggerNode);
                    }
                }
                firstFetch = false;
            }

            return terminalOutput;
        }
        public string UnescapeString(string inputString)
        {
            string stringWithUnicodeSymbols = @inputString;
            var splitted = Regex.Split(stringWithUnicodeSymbols, @"\\u([a-fA-F\d]{4})");
            string outString = "";
            foreach (var s in splitted)
            {
                try
                {
                    if (s.Length == 4)
                    {
                        var decoded = ((char)Convert.ToUInt16(s, 16)).ToString();
                        outString += decoded;
                    }
                    else
                    {
                        outString += s;
                    }
                }
                catch (Exception e)
                {
                    outString += s;
                }
            }
            return outString;
        }
    }
}