using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using RevealForge.Utils; 
using System;

namespace RevealForge.Hooks
{
    [HarmonyPatch(typeof(VariousMigratedDebugEventsSystem), nameof(VariousMigratedDebugEventsSystem.HandleRenamePlayerEvent))]
    public static class RenameEventHook
    {
        public static void Initialize() { }

        static void Prefix(VariousMigratedDebugEventsSystem __instance,
                               RenamePlayerEvent clientEvent,
                               FromCharacter fromCharacter)
        {
            try
            {
                EntityManager entityManager = __instance.EntityManager;
                string adminName;

                if (entityManager.HasComponent<User>(fromCharacter.User))
                {
                    User adminUserData = entityManager.GetComponentData<User>(fromCharacter.User);
                    adminName = adminUserData.CharacterName.ToString();
                    if (string.IsNullOrEmpty(adminName))
                    {
                        adminName = $"User ({adminUserData.PlatformId})";
                    }
                }
                else
                {
                    adminName = "UnknownAdmin";
                }

                ProjectM.Network.NetworkId eventTargetNetworkId = clientEvent.TargetNetworkId;
                string newPlayerName = clientEvent.NewName.ToString();
                string oldPlayerName = "UnknownPlayer";
                bool targetFound = false;

                if (eventTargetNetworkId.Type == NetworkIdType.Normal)
                {
                    int targetIndexFromEvent = eventTargetNetworkId.Normal_Index;
                    byte targetGenerationFromEvent = eventTargetNetworkId.Normal_Generation;

                    EntityQuery userQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<User>());
                    NativeArray<User> usersDataArray = userQuery.ToComponentDataArray<User>(Allocator.Temp);

                    foreach (User currentUser in usersDataArray)
                    {
                        if (currentUser.Index == targetIndexFromEvent &&
                            currentUser.Generation == (int)targetGenerationFromEvent)
                        {
                            oldPlayerName = currentUser.CharacterName.ToString();
                            if (string.IsNullOrEmpty(oldPlayerName)) oldPlayerName = $"User (ID: {currentUser.PlatformId})"; // Fallback if name empty
                            targetFound = true;
                            break;
                        }
                    }
                    usersDataArray.Dispose();
                    userQuery.Dispose();
                }

                if (!targetFound && eventTargetNetworkId.Type == NetworkIdType.Normal)
                {
                    oldPlayerName = $"User (ID: {eventTargetNetworkId.Normal_Index}-{eventTargetNetworkId.Normal_Generation})";
                }
                else if (!targetFound)
                {
                    oldPlayerName = $"User (Type:{eventTargetNetworkId.Type})"; 
                }
                if (string.IsNullOrEmpty(oldPlayerName)) oldPlayerName = "UnknownPlayer";

                string action = "renamed";
                string details = $"{oldPlayerName} to {newPlayerName}";
                string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";

                FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref announcementMessage);
            }
            catch (Exception) {  }
        }
    }
}