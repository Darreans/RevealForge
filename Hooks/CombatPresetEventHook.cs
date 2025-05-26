using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using RevealForge.Utils;
using System;

namespace RevealForge.Hooks
{
    [HarmonyPatch(typeof(ProjectM.Gameplay.Systems.GiveCombatPresetSystem), nameof(ProjectM.Gameplay.Systems.GiveCombatPresetSystem.OnUpdate))]
    public static class CombatPresetEventHook
    {
        private static EntityManager _entityManager;

        public static void Initialize() { }

        private static void EnsureEntityManager(SystemBase systemInstance)
        {
            if ((_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated))
            {
                if (systemInstance != null && systemInstance.World != null && systemInstance.World.IsCreated)
                {
                    _entityManager = systemInstance.EntityManager;
                }
                else
                {
                    _entityManager = VWorld.ServerEntityManager;
                }
                if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) { }
            }
        }

        static void Prefix(ProjectM.Gameplay.Systems.GiveCombatPresetSystem __instance)
        {
            EnsureEntityManager(__instance);
            if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) return;

            EntityQuery eventQuery = default;
            try
            {
                eventQuery = _entityManager.CreateEntityQuery(
                    ComponentType.ReadOnly<GiveCombatPresetEvent>(), 
                    ComponentType.ReadOnly<FromCharacter>()
                );

                if (eventQuery.IsEmpty) return;

                NativeArray<Entity> entities = eventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    if (!_entityManager.HasComponent<FromCharacter>(entity)) continue;

                    FromCharacter fromCharacter = _entityManager.GetComponentData<FromCharacter>(entity);
                    if (!_entityManager.HasComponent<User>(fromCharacter.User)) continue;

                    User adminUserData = _entityManager.GetComponentData<User>(fromCharacter.User);
                    string adminName = adminUserData.CharacterName.ToString();
                    if (string.IsNullOrEmpty(adminName)) adminName = $"User_{adminUserData.PlatformId}";

                    string action = "spawned a Combat Preset";
                   

                    string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)}{ChatColors.FormatText(".")}";

                    FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                    ServerChatUtils.SendSystemMessageToAllClients(_entityManager, ref announcementMessage);
                }
                entities.Dispose();
            }
            catch (Exception) {  }
            finally
            {
                if (!eventQuery.Equals(default(EntityQuery))) 
                {
                    eventQuery.Dispose();
                }
            }
        }
    }
}