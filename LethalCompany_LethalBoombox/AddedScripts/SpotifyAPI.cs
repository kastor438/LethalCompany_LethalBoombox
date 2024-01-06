using System;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text.RegularExpressions;


namespace LethalCompany_LethalBoombox.AddedScripts
{
    internal class SpotifyAPI : MonoBehaviour
    {
        public static SpotifyAPI Instance { get; private set; }
        public bool firstFetch = true;
        public bool searchCriteriaChanged = false;
        public bool invalidInput = false;

        private readonly string clientID = "17dfce76674e4117896a74272baa27a7";
        private readonly string clientSecret = "b71348dcd069404bba15f209537af959";
        private static AudioClip[] spotifyAudioClips;
        private static SpotifyAuthResult spotifyAuthResult;
        private static SpotifyResults spotifyResults;
        private static Item[] artistFilteredSpotifyTracks = new Item[10];
        private string SongInput;
        private string ArtistInput;
        private bool isFetchingSongs;

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
            gameObject.AddComponent<SpotifyAPI_NetworkHandler>();
            StartCoroutine(InitializeSpotify());
            
            SongInput = "";
            ArtistInput = "";
            spotifyAudioClips = new AudioClip[0];
            isFetchingSongs = false;
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

        public SpotifyResults GetSpotifyResults()
        {
            return spotifyResults;
        }

        public Item[] GetArtistFilteredSpotifyTracks()
        {
            return artistFilteredSpotifyTracks;
        }

        public AudioClip[] GetSpotifyAudioClips()
        {
            return spotifyAudioClips;
        }

        public bool GetIsFetchingSongs()
        {
            return isFetchingSongs;
        }

        public string GetSongInput()
        {
            return SongInput;
        }

        public void SetSongInput(string SongInput)
        {
            searchCriteriaChanged = true;
            this.SongInput = SongInput;
            LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Song Changed: {0}", SongInput));
        }

        public void SetArtistInput(string ArtistInput)
        {
            searchCriteriaChanged = true;
            this.ArtistInput = ArtistInput;
            LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Artist Changed: {0}", ArtistInput));
        }

        public void SetInitialSpotifySongs()
        {
            string[] audioClipURLs = { "https://p.scdn.co/mp3-preview/3c07483a9f0401cffb46d104b5cb1b30853e7767?cid=17dfce76674e4117896a74272baa27a7" };

            for (int i = 0; i < audioClipURLs.Length; i++)
            {
                GetTrackAsAudioClip(audioClipURLs[i]);
            }
            LethalBoomboxBase.Instance.mls.LogInfo("Set Initial Tracks.");
        }

        public void SearchSpotify()
        {
            searchCriteriaChanged = false;
            string uri = string.Format("https://api.spotify.com/v1/search?q={0}&type=track&limit=50", SongInput);
            //string uri = string.Format("https://catfact.ninja/fact");
            UnityWebRequest songRequest = UnityWebRequest.Get(uri);
            isFetchingSongs = true;
            StartCoroutine(FetchSpotifySongs(songRequest, uri));
        }

        private IEnumerator FetchSpotifySongs(UnityWebRequest songRequest, string uri)
        {
            using (songRequest)
            {
                songRequest.SetRequestHeader("Content-Type", "application/json");
                songRequest.SetRequestHeader("Authorization", string.Format("Bearer {0}", spotifyAuthResult.access_token));

                yield return songRequest.SendWebRequest();

                switch (songRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Connection Error: {0}", songRequest.error));
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Data Processing Error: {0}", songRequest.error));
                        break;
                    case UnityWebRequest.Result.Success:
                        LethalBoomboxBase.Instance.mls.LogInfo(songRequest.downloadHandler.text);
                        spotifyResults = JsonConvert.DeserializeObject<SpotifyResults>(songRequest.downloadHandler.text);
                        SetTrackOptions();
                        isFetchingSongs = false;
                        break;
                    default:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("API Error: {0}", songRequest.error));
                        break;
                }
                //return "Error fetching songs...";
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
                        yield return new WaitUntil(() => GetComponent<SpotifyAPI_NetworkHandler>() != null);
                        GetComponent<SpotifyAPI_NetworkHandler>().Call_UpdateSpotifyAudioClipsClientRpc(ref spotifyAudioClips, newSpotifyAudioClip);
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Adding new track: {0}", artistFilteredSpotifyTracks[trackNumber].name));
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
                        yield return new WaitUntil(() => GetComponent<SpotifyAPI_NetworkHandler>() != null);
                        GetComponent<SpotifyAPI_NetworkHandler>().Call_UpdateSpotifyAudioClipsClientRpc(ref spotifyAudioClips, newSpotifyAudioClip);
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Adding initial track url: {0}", trackURL));
                        break;
                    default:
                        LethalBoomboxBase.Instance.mls.LogInfo(string.Format("GetTrackAsAudioClip API Error: {0}", webRequest.error));
                        break;
                }
            }
        }

        public void SetTrackOptions()
        {
            int trackCount = 0;
            artistFilteredSpotifyTracks = new Item[10];
            for (int i = 0; i < spotifyResults.tracks.items.Length; i++)
            {
                if (spotifyResults.tracks.items[i] != null && spotifyResults.tracks.items[i].preview_url != null)
                {
                    string trackArtistNames = spotifyResults.tracks.items[i].artists[0].name;
                    for (int j = 1; j<spotifyResults.tracks.items[i].artists.Length; j++)
                    {
                        trackArtistNames = string.Format("{0}, {1}", trackArtistNames, spotifyResults.tracks.items[i].artists[j].name);
                    }
                    if (ArtistInput == string.Empty || trackArtistNames.ToLower().Contains(ArtistInput.ToLower()))
                    {
                        artistFilteredSpotifyTracks[trackCount] = spotifyResults.tracks.items[i];
                        trackCount++;
                    }
                }
                if (trackCount >= 10)
                    break;
            }
        }

        public string FormatSpotifyTracksTerminalOutput()
        {
            string terminalOutput = "";
            for (int i = 0; i < artistFilteredSpotifyTracks.Length; i++)
            {
                if (artistFilteredSpotifyTracks[i] != null)
                {
                    string trackArtistNames = artistFilteredSpotifyTracks[i].artists[0].name;
                    for (int j = 1; j < artistFilteredSpotifyTracks[i].artists.Length; j++)
                    {
                        trackArtistNames = string.Format("{0}, {1}", trackArtistNames, artistFilteredSpotifyTracks[i].artists[j].name);
                    }
                    terminalOutput = string.Format("{0}Track {1}: {2}, By: {3}\n", terminalOutput, (i + 1).ToString(), artistFilteredSpotifyTracks[i].name, trackArtistNames);
                }
            }
            if (terminalOutput == "")
                terminalOutput = "No tracks found with search.\n";
            else
                terminalOutput = UnescapeString(terminalOutput);
            
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
                catch
                {
                    outString += s;
                }
            }
            return outString;
        }
    }
}