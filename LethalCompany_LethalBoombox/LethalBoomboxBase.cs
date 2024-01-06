using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCAPI.TerminalCommands.Models;
using LethalCompany_LethalBoombox.Patches;
using System.Reflection;
using UnityEngine;
using RuntimeNetcodeRPCValidator;
namespace LethalCompany_LethalBoombox
{
    [BepInPlugin(BoomboxPluginInfo.PLUGIN_GUID, BoomboxPluginInfo.PLUGIN_NAME, BoomboxPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("LCAPI.TerminalCommands", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RuntimeNetcodeRPCValidator.MyPluginInfo.PLUGIN_GUID, RuntimeNetcodeRPCValidator.MyPluginInfo.PLUGIN_VERSION)]
    public class LethalBoomboxBase : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(BoomboxPluginInfo.PLUGIN_GUID);
        private NetcodeValidator netcodeValidator;

        private ModCommands modCommands;

        public static LethalBoomboxBase Instance;

        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            netcodeValidator = new NetcodeValidator(BoomboxPluginInfo.PLUGIN_GUID);
            netcodeValidator.PatchAll();

            mls = BepInEx.Logging.Logger.CreateLogSource(BoomboxPluginInfo.PLUGIN_GUID);
            mls.LogInfo("LethalBoombox has loaded.");

            modCommands = CommandRegistry.CreateModRegistry();
            modCommands.RegisterFrom(this); // Register commands from the plugin class

            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(BoomboxItemPatch));
        }

        public void RegisterCommands<T>(T instance) where T : class
        {
            modCommands.RegisterFrom(instance); // Register commands from the plugin class
        }

        private void OnDestroy()
        {
            netcodeValidator.Dispose();
        }
    }
}
