using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using RevealForge.Utils;
using System;

namespace RevealForge.Hooks
{
    [HarmonyPatch(typeof(DebugEventsSystem), nameof(DebugEventsSystem.OnUpdate))]
    public static class CreateJewelDebugEventV2Hook
    {
        private static EntityManager _entityManager;
        private static EntityQuery _eventQuery;

        public static void Initialize() { }

        private static void EnsureInitialized(SystemBase systemInstance)
        {
            if ((_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated))
            {
                if (systemInstance != null && systemInstance.World != null && systemInstance.World.IsCreated) { _entityManager = systemInstance.EntityManager; }
                else { _entityManager = VWorld.ServerEntityManager; }
                if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) return;
            }
            if ((_eventQuery.Equals(default(EntityQuery))) && !_entityManager.Equals(default(EntityManager)))
            {
                try
                {
                    _eventQuery = _entityManager.CreateEntityQuery(new EntityQueryDesc
                    {
                        All = new ComponentType[] {
                            ComponentType.ReadOnly<ProjectM.Network.CreateJewelDebugEventV2>(),
                            ComponentType.ReadOnly<FromCharacter>(),
                            ComponentType.ReadOnly<HandleClientDebugEvent>()
                        }
                    });
                }
                catch (Exception) { }
            }
        }

        static void Prefix(DebugEventsSystem __instance)
        {
            EnsureInitialized(__instance);
            if (_entityManager.Equals(default(EntityManager)) || !_entityManager.World.IsCreated) return;
            if (_eventQuery.Equals(default(EntityQuery)) || _eventQuery.IsEmpty) return;

            try
            {
                NativeArray<Entity> entities = _eventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    if (!_entityManager.Exists(entity) ||
                        !_entityManager.HasComponent<FromCharacter>(entity) ||
                        !_entityManager.HasComponent<ProjectM.Network.CreateJewelDebugEventV2>(entity))
                    {
                        continue;
                    }

                    FromCharacter fromCharacter = _entityManager.GetComponentData<FromCharacter>(entity);
                    if (!_entityManager.HasComponent<User>(fromCharacter.User)) continue;

                    User adminUserData = _entityManager.GetComponentData<User>(fromCharacter.User);
                    string adminName = adminUserData.CharacterName.ToString();
                    if (string.IsNullOrEmpty(adminName)) adminName = $"User ({adminUserData.PlatformId})";

                    ProjectM.Network.CreateJewelDebugEventV2 eventData = _entityManager.GetComponentData<ProjectM.Network.CreateJewelDebugEventV2>(entity);

                    PrefabGUID abilityGuid = eventData.AbilityPrefabGuid;
                    int internalTier = eventData.Tier;
                    float power = eventData.SpellMod1Power;

                    string jewelAbilityName = VWorld.GetItemName(abilityGuid, _entityManager);
                    if (jewelAbilityName.StartsWith("ItemGUID("))
                    {
                        jewelAbilityName = $"Ability ({abilityGuid.GuidHash})";
                    }

                    int displayTier = internalTier + 1;
                    string statsString = (power >= 0.999f) ? "Max Stats" : "Not Max Stats";

                    string action = $"created a Tier {displayTier} jewel: {jewelAbilityName}";
                    string details = $"(Stats: {statsString})";
                    string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";

                    FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                    ServerChatUtils.SendSystemMessageToAllClients(_entityManager, ref announcementMessage);
                }
                entities.Dispose();
            }
            catch (Exception) {  }
        }
    }
}