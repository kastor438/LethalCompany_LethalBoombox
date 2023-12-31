using System;
using UnityEngine;
using UnityEngine.InputSystem;
using HarmonyLib;
using LethalCompany_LethalBoombox.AddedScripts;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace LethalCompany_LethalBoombox.Patches
{
    [HarmonyPatch(typeof(BoomboxItem))]
    internal class BoomboxItemPatch
    {
        public static int CurrentAudioClipIndex { get; private set; } = 0;
        public static bool FirstSpotifyAudioPlay = true;

        [HarmonyPatch(nameof(BoomboxItem.Update))]
        [HarmonyPostfix]
        static void BoomboxControlsPatch(BoomboxItem __instance, ref AudioSource ___boomboxAudio, ref bool ___isPlayingMusic, AudioClip[] ___musicAudios)
        {
            float currVolume = ___boomboxAudio.volume;

            if (___isPlayingMusic && __instance.isHeld && __instance.playerHeldBy != null && __instance.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    float newVolume = Mathf.Clamp(currVolume + 0.1f, 0.0f, 1.0f);
                    ChangeBoomboxVolume_ServerRpc(___boomboxAudio, newVolume);
                    LethalBoomboxBase.Instance.mls.LogInfo("Increasing boombox volume.");
                    LethalBoomboxBase.Instance.mls.LogInfo("Adjusted volume to " + newVolume.ToString() + "f.");
                }
                else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                {
                    float newVolume = Mathf.Clamp(currVolume - 0.1f, 0, 1.0f);
                    ChangeBoomboxVolume_ServerRpc(___boomboxAudio, newVolume);
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
                    ChangeBoomboxSong_ServerRpc(___boomboxAudio, ___musicAudios, newAudioClipIndex);

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
                    ChangeBoomboxSong_ServerRpc(___boomboxAudio, ___musicAudios, newAudioClipIndex);

                    LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Current Audio Clip Index: {0}\nNext Audio Clip Index: {1}", currentAudioClipIndex.ToString(), newAudioClipIndex.ToString()));
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        static void ChangeBoomboxVolume_ServerRpc(AudioSource ___boomboxAudio, float newVolume)
        {
            ChangeBoomboxVolume_ClientRpc(___boomboxAudio, newVolume);
        }

        [ClientRpc]
        static void ChangeBoomboxVolume_ClientRpc(AudioSource ___boomboxAudio, float newVolume)
        {
            ___boomboxAudio.volume = newVolume;
        }

        [ServerRpc(RequireOwnership = false)]
        static void ChangeBoomboxSong_ServerRpc(AudioSource ___boomboxAudio, AudioClip[] ___musicAudios, int songIndex)
        {
            ChangeBoomboxSong_ClientRpc(___boomboxAudio, ___musicAudios, songIndex);
        }

        [ClientRpc]
        static void ChangeBoomboxSong_ClientRpc(AudioSource ___boomboxAudio, AudioClip[] ___musicAudios, int songIndex)
        {
            ___boomboxAudio.clip = ___musicAudios[songIndex];
            ___boomboxAudio.Play();
        }

        [HarmonyPatch("StartMusic")]
        [HarmonyPrefix]
        static void GetClipIndex_StartMusicPatch(ref AudioSource ___boomboxAudio, ref AudioClip[] ___musicAudios, bool startMusic)
        {
            if (startMusic)
            {
                ___musicAudios = SpotifyAPI.Instance.GetSpotifyAudioClips();
                if (SpotifyAPI.Instance != null && SpotifyAPI.Instance.GetSpotifyAudioClips().Length > 0 && FirstSpotifyAudioPlay)
                {
                    FirstSpotifyAudioPlay = false;
                    CurrentAudioClipIndex = 0;
                    ___boomboxAudio.volume = 0.5f;
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
