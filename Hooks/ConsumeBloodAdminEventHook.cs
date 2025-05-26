using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using RevealForge.Utils;
using System;

namespace RevealForge.Hooks
{
    [HarmonyPatch(typeof(ProjectM.VariousMigratedDebugEventsSystem), nameof(ProjectM.VariousMigratedDebugEventsSystem.HandleConsumeBloodEvent))]
    public static class ConsumeBloodAdminEventHook
    {
        public static void Initialize() { }

        static void Prefix(VariousMigratedDebugEventsSystem __instance,
                               [HarmonyArgument(0)] ConsumeBloodAdminEvent clientEvent,
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

                ProjectM.Network.ConsumeBloodAdminEvent eventData = clientEvent;

                string bloodTypeName = VWorld.GetItemName(eventData.PrimaryType, entityManager);
                if (bloodTypeName.StartsWith("ItemGUID("))
                {
                    bloodTypeName = $"Blood ({eventData.PrimaryType.GuidHash})";
                }

                string action = $"consumed {bloodTypeName}";
                string details = $"(Quality: {eventData.PrimaryQuality:F0}%, Amount: {eventData.Amount})";

                string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";
                FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref announcementMessage);
            }
            catch (Exception) {  }
        }
    }
}