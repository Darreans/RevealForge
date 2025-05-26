using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using RevealForge.Utils;
using System;

namespace RevealForge.Hooks
{
    [HarmonyPatch(typeof(ProjectM.VariousMigratedDebugEventsSystem), nameof(ProjectM.VariousMigratedDebugEventsSystem.HandleChangeHealthOfClosestToPositionEvent))]
    public static class ChangeHealthOfClosestToPositionEventHook
    {
        public static void Initialize() { }

        static void Prefix(VariousMigratedDebugEventsSystem __instance,
                               [HarmonyArgument(0)] ref ChangeHealthOfClosestToPositionDebugEvent eventData,
                               [HarmonyArgument(1)] ref FromCharacter fromCharacter,
                               [HarmonyArgument(2)] double serverTime)
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
                if (string.IsNullOrEmpty(adminName)) adminName = $"User (ID: {adminUserData.PlatformId})";

                int healthChangeAmount = eventData.Amount;
                string actionVerb = healthChangeAmount >= 0 ? "increased" : "decreased";
                string action = $"{actionVerb} health";
                string details = $"by {Math.Abs(healthChangeAmount)} using a console command";

                string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";
                FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref announcementMessage);
            }
            catch (Exception) { }
        }
    }
}