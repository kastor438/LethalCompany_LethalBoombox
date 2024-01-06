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

        [HarmonyPatch(nameof(BoomboxItem.Start))]
        [HarmonyPrefix]
        private static void AddToBoomboxObject(BoomboxItem __instance)
        {
            __instance.gameObject.AddComponent<BoomboxNetworkHandler>();
        }

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
                    __instance.GetComponent<BoomboxNetworkHandler>().Call_ChangeBoomboxVolumeClientRpc(newVolume);
                }
                else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                {
                    float newVolume = Mathf.Clamp(currVolume - 0.1f, 0, 1.0f);
                    __instance.GetComponent<BoomboxNetworkHandler>().Call_ChangeBoomboxVolumeClientRpc(newVolume);
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
                    //__instance.GetComponent<BoomboxNetworkHandler>().Call_ChangeBoomboxSongClientRpc(___musicAudios, newAudioClipIndex);
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
                    //__instance.GetComponent<BoomboxNetworkHandler>().Call_ChangeBoomboxSongClientRpc(___musicAudios, newAudioClipIndex);
                }
            }
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
                    ___boomboxAudio.volume = 0.4f;
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
