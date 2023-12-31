using HarmonyLib;
using LethalCompany_LethalBoombox.AddedScripts;
using System.Net;

namespace LethalCompany_LethalBoombox.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch(nameof(GameNetworkManager.StartHost))]
        [HarmonyPostfix]
        static void StartHostPatch()
        {
            LethalBoomboxBase.Instance.mls.LogInfo("StartHostPatch");

            if (GameNetworkManager.Instance.gameObject.GetComponent<SpotifyAPI>() == null)
            {
                GameNetworkManager.Instance.gameObject.AddComponent<SpotifyAPI>();
                LethalBoomboxBase.Instance.mls.LogInfo("Component SpotifyAPI mounted on GameNetworkManager.");
            }
            if (GameNetworkManager.Instance.gameObject.GetComponent<BoomboxTerminalCommands>() == null)
            {
                GameNetworkManager.Instance.gameObject.AddComponent<BoomboxTerminalCommands>();
                LethalBoomboxBase.Instance.mls.LogInfo("Component BoomboxTerminalCommands mounted on GameNetworkManager.");
            }
        }
    }
}
