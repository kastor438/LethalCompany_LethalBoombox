using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using RuntimeNetcodeRPCValidator;
using System;

namespace LethalCompany_LethalBoombox.AddedScripts
{
    public class BoomboxNetworkHandler : NetworkBehaviour
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
            RunBoomboxSetupClientRpc(gameObject.GetComponent<AudioSource>());
        }

        [ClientRpc]
        public void RunBoomboxSetupClientRpc(AudioSource ___boomboxAudio)
        {
            RunBoomboxSetupServerRpc(___boomboxAudio);
        }
        [ServerRpc]
        public void RunBoomboxSetupServerRpc(AudioSource ___boomboxAudio)
        {
            SetBoomboxClientRpc(___boomboxAudio);
        }
        [ClientRpc]
        public void SetBoomboxClientRpc(AudioSource ___boomboxAudio)
        {
            ___boomboxAudio.volume = 0.4f;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeBoomboxVolumeServerRpc(AudioSource ___boomboxAudio, float newVolume)
        {
            ChangeBoomboxVolumeClientRpc(___boomboxAudio, newVolume);
        }

        [ClientRpc]
        public void ChangeBoomboxVolumeClientRpc(AudioSource ___boomboxAudio, float newVolume)
        {
            LethalBoomboxBase.Instance.mls.LogInfo("Adjusted volume to " + newVolume.ToString() + "f.");
            ___boomboxAudio.volume = newVolume;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeBoomboxSongServerRpc(AudioSource ___boomboxAudio, AudioClip[] ___musicAudios, int songIndex)
        {
            ChangeBoomboxSongClientRpc(___boomboxAudio, ___musicAudios, songIndex);
        }

        [ClientRpc]
        public void ChangeBoomboxSongClientRpc(AudioSource ___boomboxAudio, AudioClip[] ___musicAudios, int songIndex)
        {
            LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Current Audio Clip Index: {0}\nNext Audio Clip Index: {1}", Array.IndexOf(___musicAudios, ___boomboxAudio.clip).ToString(), songIndex.ToString()));
            ___boomboxAudio.clip = ___musicAudios[songIndex];
            ___boomboxAudio.Play();
        }
    }
}