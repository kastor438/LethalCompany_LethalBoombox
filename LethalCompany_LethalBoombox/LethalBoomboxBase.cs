using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCAPI.TerminalCommands.Attributes;
using LCAPI.TerminalCommands.Models;
using LethalCompany_LethalBoombox.AddedScripts;
using LethalCompany_LethalBoombox.Patches;
using System;
using System.Security.Policy;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static LethalCompany_LethalBoombox.AddedScripts.SpotifyAPI;

namespace LethalCompany_LethalBoombox
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LethalBoomboxBase : BaseUnityPlugin
    {
        private const string modGUID = "Kastor.LethalBoombox";
        private const string modName = "LethalBoombox";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private ModCommands modCommands;

        public static LethalBoomboxBase Instance;

        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("LethalBoombox has loaded.");

            modCommands = CommandRegistry.CreateModRegistry();
            modCommands.RegisterFrom(this); // Register commands from the plugin class

            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(BoomboxItemPatch));
        }

        public void RegisterCommands<T>(T instance) where T : class
        {
            modCommands.RegisterFrom(instance); // Register commands from the plugin class
        }
    }
}
