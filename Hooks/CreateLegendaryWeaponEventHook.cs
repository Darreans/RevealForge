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
    public static class CreateLegendaryWeaponEventHook
    {
        private static EntityManager _entityManager;
        private static EntityQuery _eventQuery;
        private static bool _isQueryInitialized = false;

        public static void Initialize() { }

        private static void EnsureInitialized(SystemBase systemInstance)
        {
            if (_entityManager.Equals(default(EntityManager)) || (_entityManager.World != null && !_entityManager.World.IsCreated))
            {
                if (systemInstance != null && systemInstance.World != null && systemInstance.World.IsCreated)
                {
                    _entityManager = systemInstance.EntityManager;
                }
                else
                {
                    _entityManager = VWorld.ServerEntityManager;
                }
                if (_entityManager.Equals(default(EntityManager)) || (_entityManager.World == null || !_entityManager.World.IsCreated))
                {
                    _isQueryInitialized = false;
                    return;
                }
            }

            if (!_isQueryInitialized && !_entityManager.Equals(default(EntityManager)) && _entityManager.World != null && _entityManager.World.IsCreated)
            {
                try
                {
                    _eventQuery = _entityManager.CreateEntityQuery(new EntityQueryDesc
                    {
                        All = new ComponentType[] {
                            ComponentType.ReadOnly<ProjectM.Network.CreateLegendaryWeaponDebugEvent>(),
                            ComponentType.ReadOnly<FromCharacter>(),
                            ComponentType.ReadOnly<HandleClientDebugEvent>()
                        }
                    });
                    _isQueryInitialized = true;
                }
                catch (Exception) { _isQueryInitialized = false; }
            }
        }

        static void Prefix(DebugEventsSystem __instance)
        {
            EnsureInitialized(__instance);
            if (!_isQueryInitialized || _entityManager.Equals(default(EntityManager)) || (_entityManager.World == null || !_entityManager.World.IsCreated)) return;
            if (_eventQuery.Equals(default(EntityQuery)) || _eventQuery.IsEmpty) return;

            try
            {
                NativeArray<Entity> entities = _eventQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    if (!_entityManager.Exists(entity) ||
                        !_entityManager.HasComponent<FromCharacter>(entity) ||
                        !_entityManager.HasComponent<ProjectM.Network.CreateLegendaryWeaponDebugEvent>(entity))
                    {
                        continue;
                    }

                    FromCharacter fromCharacter = _entityManager.GetComponentData<FromCharacter>(entity);
                    if (fromCharacter.User == Entity.Null || !_entityManager.HasComponent<User>(fromCharacter.User)) continue;

                    User adminUserData = _entityManager.GetComponentData<User>(fromCharacter.User);
                    string adminName = adminUserData.CharacterName.ToString();
                    if (string.IsNullOrEmpty(adminName)) adminName = $"User ({adminUserData.PlatformId})";

                    ProjectM.Network.CreateLegendaryWeaponDebugEvent eventData = _entityManager.GetComponentData<ProjectM.Network.CreateLegendaryWeaponDebugEvent>(entity);

                    PrefabGUID weaponGuid = eventData.WeaponPrefabGuid;
                    int internalTier = eventData.Tier;
                    float power = eventData.StatMod1Power;

                    string weaponName = VWorld.GetItemName(weaponGuid, _entityManager);
                    if (weaponName.StartsWith("ItemGUID("))
                    {
                        weaponName = $"Weapon ({weaponGuid.GuidHash})";
                    }

                    int displayTier = internalTier + 1;
                    string statsString = (power >= 0.999f) ? "Max Stats" : $"Power {power:F2}";

                    string action = $"created legendary Tier {displayTier} {weaponName}";
                    string details = $"(Stats: {statsString})";
                    string announcementString = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatCommand(action)} {ChatColors.FormatText($"{details}.")}";

                    FixedString512Bytes announcementMessage = new FixedString512Bytes(announcementString);
                    ServerChatUtils.SendSystemMessageToAllClients(_entityManager, ref announcementMessage);
                }
                entities.Dispose();
            }
            catch (Exception) { }
        }
    }
}