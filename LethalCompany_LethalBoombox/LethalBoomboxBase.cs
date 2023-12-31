using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCAPI.TerminalCommands.Models;
using LethalCompany_LethalBoombox.Patches;
using System.Reflection;
using UnityEngine;

namespace LethalCompany_LethalBoombox
{
    [BepInDependency("LCAPI.TerminalCommands", BepInDependency.DependencyFlags.HardDependency)]
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

            NetcodeWeaver();

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
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

        private static void NetcodeWeaver()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}
