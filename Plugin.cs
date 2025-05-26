using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
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
        public static ManualLogSource LoggerInstance { get; private set; }

        public override void Load()
        {
            LoggerInstance = Log;

            LoggerInstance.LogInfo($"Loading {MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}...");

            InitializeHooks();
            RegisterVCFMiddleware();
            ApplyHarmonyPatches();

            LoggerInstance.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} successfully loaded!");
        }

        private void InitializeHooks()
        {
            LoggerInstance.LogInfo("Initializing Harmony hooks...");
            try
            {
                GiveDebugEventHook.Initialize(LoggerInstance);
                CreateJewelDebugEventV2Hook.Initialize(LoggerInstance);
                CreateLegendaryWeaponEventHook.Initialize(LoggerInstance);
                GenerateLegendaryWeaponEventHook.Initialize(LoggerInstance);
                GenerateBloodPotionAdminEventHook.Initialize(LoggerInstance);
                ConsumeBloodAdminEventHook.Initialize(LoggerInstance);
                ChangeDurabilityEventHook.Initialize(LoggerInstance);
                ChangeHealthOfClosestToPositionEventHook.Initialize(LoggerInstance);
                CompleteCurrentAchievementEventHook.Initialize(LoggerInstance);

                LoggerInstance.LogInfo("All Harmony hook initializations called.");
            }
            catch (Exception e)
            {
                LoggerInstance.LogError($"An error occurred during Harmony hook initialization: {e}");
            }
        }

        private void RegisterVCFMiddleware()
        {
            LoggerInstance.LogInfo("Attempting to register VCF middlewares...");
            try
            {
                var adminAnnounceMiddleware = new AdminAnnounceMiddleware(LoggerInstance);
                CommandRegistry.Middlewares.Add(adminAnnounceMiddleware); 
                LoggerInstance.LogInfo("AdminAnnounceMiddleware added to VCF's middleware list.");
            }
            catch (Exception e)
            {
                LoggerInstance.LogError($"Error registering VCF middleware: {e}");
            }
        }

        private void ApplyHarmonyPatches()
        {
            try
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                LoggerInstance.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} patches applied!");
            }
            catch (Exception e)
            {
                LoggerInstance.LogError($"An error occurred while applying Harmony patches: {e}");
            }
        }

        public override bool Unload()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
                LoggerInstance.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} patches are Unloaded!");
            }
            return true;
        }
    }

    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "RevealForge";
        public const string PLUGIN_NAME = "RevealForge Event Announcer";
        public const string PLUGIN_VERSION = "1.0.0"; 
    }
}