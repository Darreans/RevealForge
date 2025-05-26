using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using RevealForge.Utils; 
using System;

namespace RevealForge.Hooks
{
    [HarmonyPatch(typeof(ProjectM.VariousMigratedDebugEventsSystem), nameof(ProjectM.VariousMigratedDebugEventsSystem.HandleCompleteCurrentAchievementEvent))]
    public static class CompleteCurrentAchievementEventHook
    {
        public static void Initialize() { }

        static void Prefix(VariousMigratedDebugEventsSystem __instance,
                               [HarmonyArgument(0)] ref CompleteCurrentAchievementAdminEvent eventData,
                               [HarmonyArgument(1)] ref FromCharacter fromCharacter)
        {
            if (__instance == null) return;

            EntityManager entityManager = __instance.EntityManager;
            if (entityManager.World == null || !entityManager.World.IsCreated)
            {
                entityManager = VWorld.ServerEntityManager;
                if (entityManager.World == null || !entityManager.World.IsCreated) return;
            }

            try
            {
                if (fromCharacter.User == Entity.Null) return;
                if (!entityManager.HasComponent<User>(fromCharacter.User)) return;

                User adminUserData = entityManager.GetComponentData<User>(fromCharacter.User);
                string adminName = adminUserData.CharacterName.ToString();
                if (string.IsNullOrEmpty(adminName)) adminName = $"Admin (ID: {adminUserData.PlatformId})";

                string targetCharacterName = eventData.CharacterName.ToString();
                if (string.IsNullOrEmpty(targetCharacterName))
                {
                    targetCharacterName = "player specified in command";
                }

                int amount = eventData.Amount;
                string achievementText = amount == 1 ? "current achievement" : $"{amount} current achievements";
                string action = $"completed {achievementText}";
                string details = $"for {targetCharacterName}";

                string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";
                FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref announcementMessage);
            }
            catch (Exception) { }
        }
    }
}