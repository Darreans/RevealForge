using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using RevealForge.Utils;
using System;

namespace RevealForge.Hooks
{
    [HarmonyPatch(typeof(DebugEventsSystem), nameof(DebugEventsSystem.OnUpdate))]
    public static class GiveDebugEventHook
    {
        private static EntityManager _entityManager;

        public static void Initialize() { }

        static void Prefix(DebugEventsSystem __instance)
        {
            if ((_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated))
            {
                if (__instance.World != null && __instance.World.IsCreated)
                {
                    _entityManager = __instance.EntityManager;
                }
                else
                {
                    _entityManager = VWorld.ServerEntityManager;
                    if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) return;
                }
            }
            if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) return;

            EntityQuery giveEventQuery = default;
            NativeArray<Entity> eventEntities = default;

            try
            {
                giveEventQuery = _entityManager.CreateEntityQuery(
                    ComponentType.ReadOnly<GiveDebugEvent>(),
                    ComponentType.ReadOnly<FromCharacter>()
                );

                if (giveEventQuery.IsEmpty) return;
                eventEntities = giveEventQuery.ToEntityArray(Allocator.Temp);

                foreach (Entity eventEntity in eventEntities)
                {
                    try
                    {
                        if (!_entityManager.Exists(eventEntity)) continue;

                        GiveDebugEvent giveEventData = _entityManager.GetComponentData<GiveDebugEvent>(eventEntity);
                        FromCharacter fromCharacterData = _entityManager.GetComponentData<FromCharacter>(eventEntity);

                        if (!_entityManager.Exists(fromCharacterData.User) || !_entityManager.HasComponent<User>(fromCharacterData.User)) continue;

                        User adminUserData = _entityManager.GetComponentData<User>(fromCharacterData.User);
                        string adminName = adminUserData.CharacterName.ToString();
                        if (string.IsNullOrEmpty(adminName)) adminName = $"User_{adminUserData.Index}";

                        string itemName = VWorld.GetItemName(giveEventData.PrefabGuid, _entityManager);
                        if (itemName.StartsWith("GUID(") || string.IsNullOrEmpty(itemName) || itemName.StartsWith("ItemGUID("))
                        {
                            itemName = $"an item (ID: {giveEventData.PrefabGuid.GuidHash})";
                        }

                        string action = "used 'give'";
                        string details = $"for {itemName} (Amount: {giveEventData.Amount})";
                        string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";

                        FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                        ServerChatUtils.SendSystemMessageToAllClients(_entityManager, ref announcementMessage);
                    }
                    catch (Exception) {  }
                }
            }
            catch (Exception) { }
            finally
            {
                if (eventEntities.IsCreated) eventEntities.Dispose(); 
                if (!giveEventQuery.Equals(default(EntityQuery))) giveEventQuery.Dispose();
            }
        }
    }
}