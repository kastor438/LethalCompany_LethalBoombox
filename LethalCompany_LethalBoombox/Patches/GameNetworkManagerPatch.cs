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
            if (GameNetworkManager.Instance.gameObject.GetComponent<BoomboxNetworkHandler>() == null)
            {
                GameNetworkManager.Instance.gameObject.AddComponent<BoomboxNetworkHandler>();
                LethalBoomboxBase.Instance.mls.LogInfo("Component BoomboxNetworkHandler mounted on GameNetworkManager.");
            }
        }

        [HarmonyPatch(nameof(GameNetworkManager.JoinLobby))]
        [HarmonyPostfix]
        static void JoinLobbyPatch()
        {
            LethalBoomboxBase.Instance.mls.LogInfo("JoinLobbyPatch");

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
            if (GameNetworkManager.Instance.gameObject.GetComponent<BoomboxNetworkHandler>() == null)
            {
                GameNetworkManager.Instance.gameObject.AddComponent<BoomboxNetworkHandler>();
                LethalBoomboxBase.Instance.mls.LogInfo("Component BoomboxNetworkHandler mounted on GameNetworkManager.");
            }
        }

        [HarmonyPatch("SteamMatchmaking_OnLobbyMemberJoined")]
        [HarmonyPostfix]
        static void SteamMatchmaking_OnLobbyMemberJoinedPatch()
        {
            LethalBoomboxBase.Instance.mls.LogInfo("SteamMatchmaking_OnLobbyMemberJoinedPatch");

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
            if (GameNetworkManager.Instance.gameObject.GetComponent<BoomboxNetworkHandler>() == null)
            {
                GameNetworkManager.Instance.gameObject.AddComponent<BoomboxNetworkHandler>();
                LethalBoomboxBase.Instance.mls.LogInfo("Component BoomboxNetworkHandler mounted on GameNetworkManager.");
            }
        }
    }
}
