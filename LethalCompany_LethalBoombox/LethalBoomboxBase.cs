using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalCompany_LethalBoombox.Patches;

namespace LethalCompany_LethalBoombox
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LethalBoomboxBase : BaseUnityPlugin
    {
        private const string modGUID = "Kastor.LethalBoombox";
        private const string modName = "LethalBoombox";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
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

            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(BoomboxItemPatch));
            harmony.PatchAll(typeof(TerminalPatch));
        }
    }
}
