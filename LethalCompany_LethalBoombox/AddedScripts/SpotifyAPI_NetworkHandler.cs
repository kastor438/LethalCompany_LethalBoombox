using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using RuntimeNetcodeRPCValidator;
using System;

namespace LethalCompany_LethalBoombox.AddedScripts
{
    public class SpotifyAPI_NetworkHandler : NetworkBehaviour
    {
        private void Awake()
        {
            // Are we a server instance?
            if (IsHost)
                StartCoroutine(WaitForSomeTime());
        }

        private IEnumerator WaitForSomeTime()
        {
            // Wait for network object to spawn, GameNetworkManager to exist, and have component SpotifyAPI attached.
            yield return new WaitUntil(() => NetworkObject.IsSpawned);
            yield return new WaitUntil(() => GameNetworkManager.Instance != null);
            yield return new WaitUntil(() => GameNetworkManager.Instance.GetComponent<SpotifyAPI>() != null);

            // Tell all clients to run this method.
            //RunBoomboxSetupClientRpc(gameObject.GetComponent<AudioSource>());
        }

        // Update Spotify audio clips.
        [ClientRpc]
        public void Call_UpdateSpotifyAudioClipsClientRpc(ref AudioClip[] spotifyAudioClips, AudioClip newAudioClip)
        {
            UpdateSpotifyAudioClipsServerRpc(ref spotifyAudioClips, newAudioClip);
        }

        [ServerRpc]
        public void UpdateSpotifyAudioClipsServerRpc(ref AudioClip[] spotifyAudioClips, AudioClip newAudioClip)
        {
            UpdateSpotifyAudioClipsClientRpc(ref spotifyAudioClips, newAudioClip);
        }

        [ClientRpc]
        public void UpdateSpotifyAudioClipsClientRpc(ref AudioClip[] spotifyAudioClips, AudioClip newAudioClip)
        {
            if (spotifyAudioClips.Length < 10)
            {
                Array.Resize(ref spotifyAudioClips, spotifyAudioClips.Length + 1);
                spotifyAudioClips[spotifyAudioClips.Length - 1] = newAudioClip;
            }
            else
            {
                for (int i = 1; i < spotifyAudioClips.Length; i++)
                {
                    spotifyAudioClips[i - 1] = spotifyAudioClips[i];
                }
                spotifyAudioClips[9] = newAudioClip;
            }
        }
    }
}