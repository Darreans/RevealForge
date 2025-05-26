using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Entities;
using RevealForge.Utils; 
using System;
using Unity.Collections;
using System.Reflection;
using ProjectM.Shared;

namespace RevealForge.Hooks
{
    [HarmonyPatch]
    public static class InventoryCommandsHook
    {
        private static EntityManager _entityManager;

        public static void Initialize() { }

        private static void EnsureEntityManager()
        {
            if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated)
            {
                _entityManager = VWorld.ServerEntityManager;
                if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) {  }
            }
        }

        private static string GetAdminNameFromSystem(object systemInstance)
        {
            if (systemInstance == null) return "Unknown Admin";
            EnsureEntityManager();
            Entity userEntity = Entity.Null;
            try
            {
                string[] possibleFieldNames = { "_LastCommandUserEntity", "LastExecutingUserEntity", "m_LastExecutingUserEntity", "m_ExecutingUser", "_currentUserEntity" };
                FieldInfo userField = null;
                foreach (var fieldName in possibleFieldNames)
                {
                    userField = AccessTools.Field(systemInstance.GetType(), fieldName);
                    if (userField != null) break;
                }

                if (userField != null)
                {
                    object userEntityObj = userField.GetValue(systemInstance);
                    if (userEntityObj is Entity foundEntity) userEntity = foundEntity;
                }

                if (userEntity != Entity.Null && _entityManager.Exists(userEntity) && _entityManager.HasComponent<User>(userEntity))
                {
                    User userData = _entityManager.GetComponentData<User>(userEntity);
                    string charName = userData.CharacterName.ToString();
                    return string.IsNullOrEmpty(charName) ? $"User ({userData.PlatformId})" : charName;
                }
            }
            catch (Exception) { }
            return "Server/Unknown";
        }

        [HarmonyPatch(typeof(GiveInventoryItemCommandSystem), "Give")]
        [HarmonyPostfix]
        static void GiveCommandPostfix(GiveInventoryItemCommandSystem __instance, string item, int giveAmount)
        {
            try
            {
                EnsureEntityManager();
                if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) return;

                string adminName = GetAdminNameFromSystem(__instance);
                string displayName = item;

                if (int.TryParse(item, out int guidHash))
                {
                    string resolvedName = VWorld.GetItemName(new PrefabGUID(guidHash), _entityManager);
                    if (!resolvedName.StartsWith("ItemGUID(")) displayName = resolvedName;
                }

                string details = $"\"{displayName}\" (Amount: {giveAmount})";
                string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatText("used command")} {ChatColors.FormatCommand("give")} {ChatColors.FormatText($"{details}.")}";

                FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                ServerChatUtils.SendSystemMessageToAllClients(_entityManager, ref announcementMessage);
            }
            catch (Exception) {  }
        }

        [HarmonyPatch(typeof(GiveInventoryItemCommandSystem), "GiveSet")]
        [HarmonyPostfix]
        static void GiveSetCommandPostfix(GiveInventoryItemCommandSystem __instance, string itemSet)
        {
            try
            {
                EnsureEntityManager();
                if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) return;

                string adminName = GetAdminNameFromSystem(__instance);
                string action = $"spawned Combat Preset: {itemSet}";
                string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)}{ChatColors.FormatText(".")}";

                FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                ServerChatUtils.SendSystemMessageToAllClients(_entityManager, ref announcementMessage);
            }
            catch (Exception) { }
        }

        [HarmonyPatch(typeof(GiveInventoryItemCommandSystem), "CreateJewel",
            new Type[] { typeof(string), typeof(int), typeof(string), typeof(float),
                         typeof(string), typeof(float), typeof(string), typeof(float),
                         typeof(string), typeof(float) })]
        [HarmonyPostfix]
        static void CreateJewelCommandPostfix(GiveInventoryItemCommandSystem __instance, string abilityName, int tier,
            string spellMod1, float power1, string spellMod2, float power2,
            string spellMod3, float power3, string spellMod4, float power4)
        {
            try
            {
                EnsureEntityManager();
                if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) return;

                string adminName = GetAdminNameFromSystem(__instance);
                string displayAbilityName = abilityName;
                if (int.TryParse(abilityName, out int abilityGuidHash))
                {
                    string resolvedName = VWorld.GetItemName(new PrefabGUID(abilityGuidHash), _entityManager);
                    if (!resolvedName.StartsWith("ItemGUID(")) displayAbilityName = resolvedName;
                }

                int displayTier = tier + 1;
                string statsString = (power1 >= 0.999f) ? "Max Stats" : "Not Max Stats";

                string action = $"created a Tier {displayTier} jewel: {displayAbilityName}";
                string details = $"(Stats: {statsString})";
                string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";

                FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                ServerChatUtils.SendSystemMessageToAllClients(_entityManager, ref announcementMessage);
            }
            catch (Exception) {  }
        }

        [HarmonyPatch(typeof(GiveInventoryItemCommandSystem), "CreateAndFullyEquipJewels", new Type[] { typeof(int) })]
        [HarmonyPostfix]
        static void CreateAndFullyEquipJewelsPostfix(GiveInventoryItemCommandSystem __instance, int inputTier)
        {
            try
            {
                EnsureEntityManager();
                if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) return;

                string adminName = GetAdminNameFromSystem(__instance);
                int displayTier = inputTier + 1;
                string statsString = "Max Stats";

                string action = $"created Tier {displayTier} jewels for all slots";
                string details = $"(Stats: {statsString})";
                string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";

                FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                ServerChatUtils.SendSystemMessageToAllClients(_entityManager, ref announcementMessage);
            }
            catch (Exception) { }
        }
    }
}