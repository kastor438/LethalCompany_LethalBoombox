using HarmonyLib;
using LethalCompany_LethalBoombox.AddedScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TerminalApi.TerminalApi;

namespace LethalCompany_LethalBoombox.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch("ParsePlayerSentence")]
        [HarmonyPrefix]
        static async void ParsePlayerSentencePatch_Prefix()
        {
            char[] delimiters = { ' ' };
            string terminalInput = GetTerminalInput();
            string[] terminalInputArray = terminalInput.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Terminal Input: {0}", terminalInput));

            if (terminalInput != null && terminalInputArray.Length >= 2 && terminalInputArray[0].ToLower() == "song")
            {
                if (terminalInput.Substring(5).Length >= 2)
                {
                    DeleteKeyword(SpotifyAPI.Instance.SongInput);
                    SpotifyAPI.Instance.SongInput = terminalInput.Substring(5);

                    TerminalKeyword songKeyword = GetKeyword("song");
                    SpotifyAPI.Instance.SongInputKeyword = CreateTerminalKeyword(SpotifyAPI.Instance.SongInput);
                    TerminalNode songTriggerNode = CreateTerminalNode(string.Format("Song Set: {0}\n", SpotifyAPI.Instance.SongInputKeyword.word), true);

                    AddTerminalKeyword(SpotifyAPI.Instance.SongInputKeyword);
                    AddCompatibleNoun(songKeyword, SpotifyAPI.Instance.SongInputKeyword.word, songTriggerNode);

                    LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Song name changed: {0}", SpotifyAPI.Instance.SongInput));
                    SpotifyAPI.Instance.searchCriteriaChanged = true;
                    SpotifyAPI.Instance.invalidInput = false;
                }
                else
                {
                    DeleteKeyword(SpotifyAPI.Instance.SongInput);
                    SpotifyAPI.Instance.SongInput = terminalInput.Substring(5);

                    TerminalKeyword songKeyword = GetKeyword("song");
                    SpotifyAPI.Instance.SongInputKeyword = CreateTerminalKeyword(SpotifyAPI.Instance.SongInput);
                    TerminalNode songTriggerNode = CreateTerminalNode("Invalid Input. Provide Atleast 2 characters.\n", true);

                    AddTerminalKeyword(SpotifyAPI.Instance.SongInputKeyword);
                    AddCompatibleNoun(songKeyword, SpotifyAPI.Instance.SongInputKeyword.word, songTriggerNode);
                    SpotifyAPI.Instance.invalidInput = true;
                }
            }
            else if (terminalInput != null && terminalInputArray.Length >= 2 && terminalInputArray[0].ToLower() == "artist")
            {
                if(terminalInput.Substring(7).Length >= 2)
                {
                    DeleteKeyword(SpotifyAPI.Instance.ArtistInput);
                    SpotifyAPI.Instance.ArtistInput = terminalInput.Substring(7);

                    TerminalKeyword artistKeyword = GetKeyword("artist");
                    SpotifyAPI.Instance.ArtistInputKeyword = CreateTerminalKeyword(SpotifyAPI.Instance.ArtistInput);
                    TerminalNode artistTriggerNode = CreateTerminalNode(string.Format("Artist Set: {0}\n", SpotifyAPI.Instance.ArtistInputKeyword.word), true);

                    AddTerminalKeyword(SpotifyAPI.Instance.ArtistInputKeyword);
                    AddCompatibleNoun(artistKeyword, SpotifyAPI.Instance.ArtistInputKeyword.word, artistTriggerNode);

                    LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Artist name changed: {0}", SpotifyAPI.Instance.ArtistInput));
                    SpotifyAPI.Instance.searchCriteriaChanged = true;
                    SpotifyAPI.Instance.invalidInput = false;
                }
                else
                {
                    DeleteKeyword(SpotifyAPI.Instance.ArtistInput);
                    SpotifyAPI.Instance.ArtistInput = terminalInput.Substring(7);

                    TerminalKeyword artistKeyword = GetKeyword("artist");
                    SpotifyAPI.Instance.ArtistInputKeyword = CreateTerminalKeyword(SpotifyAPI.Instance.ArtistInput);
                    TerminalNode artistTriggerNode = CreateTerminalNode("Invalid Input. Provide Atleast 2 characters.\n", true);

                    AddTerminalKeyword(SpotifyAPI.Instance.ArtistInputKeyword);
                    AddCompatibleNoun(artistKeyword, SpotifyAPI.Instance.ArtistInputKeyword.word, artistTriggerNode);
                    SpotifyAPI.Instance.invalidInput = true;
                }
            }
            else if (terminalInput != null && terminalInputArray.Length == 1 && terminalInputArray[0].ToLower() == "fetch")
            {
                if (SpotifyAPI.Instance.searchCriteriaChanged && !SpotifyAPI.Instance.invalidInput)
                {
                    LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Fetching Songs...."));
                    DeleteKeyword("fetch");
                    SpotifyAPI.Instance.SearchSpotify();
                }
            }
        }
        
        [HarmonyPatch("ParsePlayerSentence")]
        [HarmonyPostfix]
        public static void ParsePlayerSentencePatch_Postfix()
        {
            char[] delimiters = { ' ' };
            string terminalInput = GetTerminalInput();
            string[] terminalInputArray = terminalInput.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (terminalInput != null && terminalInputArray.Length == 2 && terminalInputArray[0].ToLower() == "track")
            {
                try
                {
                    int trackNumber = int.Parse(terminalInputArray[1]) - 1;
                    if (trackNumber > 0 && trackNumber <= SpotifyAPI.Instance.GetArtistFilteredSpotifyTracks().Length)
                    {
                        SpotifyAPI.Instance.GetTrackAsAudioClip(trackNumber);
                    }
                }
                catch (Exception exception)
                {
                    LethalBoomboxBase.Instance.mls.LogInfo(exception.Message);
                }
            }
        }
    }
}
