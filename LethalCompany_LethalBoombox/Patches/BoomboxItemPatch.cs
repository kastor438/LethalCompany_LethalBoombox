using System;
using UnityEngine;
using UnityEngine.InputSystem;
using HarmonyLib;
using LethalCompany_LethalBoombox.AddedScripts;

namespace LethalCompany_LethalBoombox.Patches
{
    [HarmonyPatch(typeof(BoomboxItem))]
    internal class BoomboxItemPatch
    {
        public static int CurrentAudioClipIndex { get; private set; } = 0;
        public static bool FirstSpotifyAudioPlay = true;

        [HarmonyPatch(nameof(BoomboxItem.Update))]
        [HarmonyPostfix]
        static void BoomboxControlsPatch(ref AudioSource ___boomboxAudio, ref bool ___isPlayingMusic, AudioClip[] ___musicAudios)
        {
            float currVolume = ___boomboxAudio.volume;

            if (___isPlayingMusic)
            {
                if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    float newVolume = Mathf.Clamp(currVolume + 0.1f, 0.0f, 1.0f);
                    ___boomboxAudio.volume = newVolume;
                    LethalBoomboxBase.Instance.mls.LogInfo("Increasing boombox volume.");
                    LethalBoomboxBase.Instance.mls.LogInfo("Adjusted volume to " + newVolume.ToString() + "f.");
                }
                else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                {
                    float newVolume = Mathf.Clamp(currVolume - 0.1f, 0, 1.0f);
                    ___boomboxAudio.volume = newVolume;
                    LethalBoomboxBase.Instance.mls.LogInfo("Decreasing boombox volume.");
                    LethalBoomboxBase.Instance.mls.LogInfo("Adjusted volume to " + newVolume.ToString() + "f.");
                }

                if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
                {
                    int currentAudioClipIndex = Array.IndexOf(___musicAudios, ___boomboxAudio.clip);
                    int newAudioClipIndex;
                    if (currentAudioClipIndex == -1 || (currentAudioClipIndex + 1) >= ___musicAudios.Length)
                    {
                        newAudioClipIndex = 0;
                    }
                    else
                    {
                        newAudioClipIndex = currentAudioClipIndex + 1;
                    }
                    ___boomboxAudio.clip = ___musicAudios[newAudioClipIndex];                    
                    ___boomboxAudio.Play();

                    LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Current Audio Clip Index: {0}\nNext Audio Clip Index: {1}", currentAudioClipIndex.ToString(), newAudioClipIndex.ToString()));
                }
                else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
                {

                    int currentAudioClipIndex = Array.IndexOf(___musicAudios, ___boomboxAudio.clip);
                    int newAudioClipIndex;
                    if (currentAudioClipIndex == -1 || (currentAudioClipIndex - 1) < 0)
                    {
                        newAudioClipIndex = ___musicAudios.Length - 1;
                    }
                    else
                    {
                        newAudioClipIndex = currentAudioClipIndex - 1;
                    }
                    ___boomboxAudio.clip = ___musicAudios[newAudioClipIndex];
                    ___boomboxAudio.Play();

                    LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Current Audio Clip Index: {0}\nNext Audio Clip Index: {1}", currentAudioClipIndex.ToString(), newAudioClipIndex.ToString()));
                }

                // SpotifyAPI testing
                //if (Keyboard.current.digit0Key.wasPressedThisFrame)
                //{
                //    SpotifyAPI.Instance.SearchSpotify();
                //}
            }
        }

        [HarmonyPatch("StartMusic")]
        [HarmonyPrefix]
        static void GetClipIndex_StartMusicPatch(ref AudioSource ___boomboxAudio, ref AudioClip[] ___musicAudios, bool startMusic)
        {
            if (startMusic)
            {
                LethalBoomboxBase.Instance.mls.LogInfo(string.Format("SpotifyAPI.Instance.spotifyAudioClips.Length: {0}", SpotifyAPI.Instance.spotifyAudioClips.Length));
                if (SpotifyAPI.Instance != null && SpotifyAPI.Instance.spotifyAudioClips.Length > 0 && FirstSpotifyAudioPlay)
                {
                    ___musicAudios = SpotifyAPI.Instance.spotifyAudioClips;
                    FirstSpotifyAudioPlay = false;
                    CurrentAudioClipIndex = 0;
                    LethalBoomboxBase.Instance.mls.LogInfo(string.Format("First Spotify Audio Play."));
                }
                else
                {
                    CurrentAudioClipIndex = Array.IndexOf(___musicAudios, ___boomboxAudio.clip);
                }
            }
        }

        [HarmonyPatch("StartMusic")]
        [HarmonyPostfix]
        static void StartMusicPatch(ref AudioSource ___boomboxAudio, ref AudioClip[] ___musicAudios, bool startMusic)
        {
            if (startMusic)
            {
                ___boomboxAudio.clip = ___musicAudios[CurrentAudioClipIndex];
                ___boomboxAudio.pitch = 1f;
                ___boomboxAudio.Play();
            }
        }
    }
}
