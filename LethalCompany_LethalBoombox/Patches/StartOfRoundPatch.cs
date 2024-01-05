using HarmonyLib;
using LethalCompany_LethalBoombox.AddedScripts;

namespace LethalCompany_LethalBoombox.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch("PlayerLoadedClientRpc")]
        [HarmonyPostfix]
        public static void PlayerLoadedClientRpcPatch()
        {
            LethalBoomboxBase.Instance.mls.LogInfo("PlayerLoadedClientRpcPatch");

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
