using HarmonyLib;
using LethalCompany_LethalBoombox.AddedScripts;
using static TerminalApi.TerminalApi;
using System.Net;
using TerminalApi;

namespace LethalCompany_LethalBoombox.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch(nameof(GameNetworkManager.StartHost))]
        [HarmonyPostfix]
        static void StartHostPatch()
        {
            GameNetworkManager.Instance.gameObject.AddComponent<SpotifyAPI>();
            LethalBoomboxBase.Instance.mls.LogInfo("Component SpotifyAPI mounted on GameNetworkManager.");
        }
    }
}
