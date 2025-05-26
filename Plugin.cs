using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using RevealForge.Utils;
using RevealForge.Hooks;
using System;
using VampireCommandFramework;
using RevealForge.Middlewares;

namespace RevealForge
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin
    {
        private Harmony _harmony;

        public override void Load()
        {
            VWorld.Initialize();
            InitializeHooks();
            RegisterVCFMiddleware();
            ApplyHarmonyPatches();
        }

        private void InitializeHooks()
        {
            try
            {
                GiveDebugEventHook.Initialize();
                CreateJewelDebugEventV2Hook.Initialize();
                CreateLegendaryWeaponEventHook.Initialize();
                GenerateLegendaryWeaponEventHook.Initialize();
                GenerateBloodPotionAdminEventHook.Initialize();
                ConsumeBloodAdminEventHook.Initialize();
                ChangeHealthOfClosestToPositionEventHook.Initialize();
                CompleteCurrentAchievementEventHook.Initialize();
                CombatPresetEventHook.Initialize();
                InventoryCommandsHook.Initialize();
                RenameEventHook.Initialize();
            }
            catch (Exception) {  }
        }

        private void RegisterVCFMiddleware()
        {
            try
            {
                var adminAnnounceMiddleware = new AdminAnnounceMiddleware();
                CommandRegistry.Middlewares.Add(adminAnnounceMiddleware);
            }
            catch (Exception) { }
        }

        private void ApplyHarmonyPatches()
        {
            try
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception) { }
        }

        public override bool Unload()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
            try
            {
                CommandRegistry.Middlewares.RemoveAll(m => m is AdminAnnounceMiddleware);
            }
            catch (Exception) {  }
            return true;
        }
    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "RevealForge";
        public const string PLUGIN_NAME = "RevealForge Event Announcer";
        public const string PLUGIN_VERSION = "1.0.0";
    }
}
}