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
        public BoomboxItem boomboxInstance;

        public void Awake()
        {
            boomboxInstance = GetComponent<BoomboxItem>();
            // Are we a server instance?
            if (IsHost)
                StartCoroutine(WaitForSomeTime());
        }

        private IEnumerator WaitForSomeTime()
        {
            // Wait for network object to spawn, GameNetworkManager to exist, and have component SpotifyAPI attached.
            yield return new WaitUntil(() => NetworkObject.IsSpawned);

            // Tell all clients to run this method.
            RunBoomboxSetupClientRpc();
        }

        [ClientRpc]
        public void RunBoomboxSetupClientRpc()
        {
            RunBoomboxSetupServerRpc();
        }
        [ServerRpc]
        public void RunBoomboxSetupServerRpc()
        {
            SetBoomboxClientRpc();
        }
        [ClientRpc]
        public void SetBoomboxClientRpc()
        {
            boomboxInstance.GetComponent<AudioSource>().volume = 0.4f;
            LethalBoomboxBase.Instance.mls.LogInfo("Clearly spawned......");
        }

        // Changing boombox volume
        [ClientRpc]
        public void Call_ChangeBoomboxVolumeClientRpc(float newVolume)
        {
            ChangeBoomboxVolumeServerRpc(newVolume);
        }

        [ServerRpc]
        public void ChangeBoomboxVolumeServerRpc(float newVolume)
        {
            ChangeBoomboxVolumeClientRpc(newVolume);
        }

        [ClientRpc]
        public void ChangeBoomboxVolumeClientRpc(float newVolume)
        {
            LethalBoomboxBase.Instance.mls.LogInfo("Adjusted volume to " + newVolume.ToString() + "f.");
            boomboxInstance.GetComponent<AudioSource>().volume = newVolume;
        }
        
        // Changing boombox song
        [ClientRpc]
        public void Call_ChangeBoomboxSongClientRpc(AudioClip[] ___musicAudios, int songIndex)
        {
            ChangeBoomboxSongServerRpc(___musicAudios, songIndex);
        }

        [ServerRpc]
        public void ChangeBoomboxSongServerRpc(AudioClip[] ___musicAudios, int songIndex)
        {
            ChangeBoomboxSongClientRpc(___musicAudios, songIndex);
        }

        [ClientRpc]
        public void ChangeBoomboxSongClientRpc(AudioClip[] ___musicAudios, int songIndex)
        {
            LethalBoomboxBase.Instance.mls.LogInfo(string.Format("Current Audio Clip Index: {0}\nNext Audio Clip Index: {1}", Array.IndexOf(___musicAudios, boomboxInstance.GetComponent<AudioSource>().clip).ToString(), songIndex.ToString()));
            boomboxInstance.GetComponent<AudioSource>().clip = ___musicAudios[songIndex];
            boomboxInstance.GetComponent<AudioSource>().Play();
        }
    }
}