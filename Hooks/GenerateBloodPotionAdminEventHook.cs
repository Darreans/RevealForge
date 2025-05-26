using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using RevealForge.Utils; 
using System;
using System.Runtime.InteropServices;

namespace RevealForge.Hooks
{
    [HarmonyPatch(typeof(ProjectM.VariousMigratedDebugEventsSystem), nameof(ProjectM.VariousMigratedDebugEventsSystem.HandleGenerateBloodPotionEvent))]
    public static class GenerateBloodPotionAdminEventHook
    {
        public static void Initialize() { }

        static void Prefix(VariousMigratedDebugEventsSystem __instance,
                               [HarmonyArgument(0)] ref GenerateBloodPotionAdminEvent clientEvent,
                               [HarmonyArgument(1)] FromCharacter fromCharacter)
        {
            if (__instance == null) return;

            EntityManager entityManager = __instance.EntityManager;
            if (entityManager.Equals(default(EntityManager)) || !entityManager.World.IsCreated)
            {
                entityManager = VWorld.ServerEntityManager;
                if (entityManager.Equals(default(EntityManager)) || !entityManager.World.IsCreated) return;
            }

            try
            {
                if (!entityManager.HasComponent<User>(fromCharacter.User)) return;

                User adminUserData = entityManager.GetComponentData<User>(fromCharacter.User);
                string adminName = adminUserData.CharacterName.ToString();
                if (string.IsNullOrEmpty(adminName)) adminName = $"User ({adminUserData.PlatformId})";

                string primaryBloodName = VWorld.GetItemName(clientEvent.PrimaryBloodTypePrefab, entityManager);
                if (primaryBloodName.StartsWith("ItemGUID("))
                {
                    primaryBloodName = $"BloodType ({clientEvent.PrimaryBloodTypePrefab.GuidHash})";
                }

                string action = $"generated a {primaryBloodName} Potion";
                string details = $"(Quality: {clientEvent.PrimaryQuality:F0}%)";
                string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";

                FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref announcementMessage);
            }
            catch (Exception) { }
        }
    }
}