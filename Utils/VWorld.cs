using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using Unity.Collections;
using System;
using System.Collections.Generic;
using RevealForge.Data; 

namespace RevealForge.Utils
{
    public static class VWorld
    {
        private static World _serverWorld;
        private static EntityManager _serverEntityManager;
        private static PrefabCollectionSystem _prefabCollectionSystem;

        public static void Initialize()
        {
            TryGetServerWorldAndSystems();
        }

        private static void TryGetServerWorldAndSystems()
        {
            if (_serverWorld == null || !_serverWorld.IsCreated)
            {
                _serverWorld = GetWorld("Server");
            }

            if (_serverWorld != null && _serverWorld.IsCreated)
            {
                if (_serverEntityManager.Equals(default(EntityManager)) || !_serverEntityManager.World.IsCreated)
                {
                    _serverEntityManager = _serverWorld.EntityManager;
                }

                if (_prefabCollectionSystem == null || !_prefabCollectionSystem.World.IsCreated)
                {
                    try
                    {
                        if (_serverWorld.IsCreated)
                        { 
                            _prefabCollectionSystem = _serverWorld.GetExistingSystemManaged<PrefabCollectionSystem>();
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        private static World GetWorld(string name)
        {
            if (World.All == null) return null;
            foreach (var world in World.All)
            {
                if (world.Name == name) return world;
            }
            return null;
        }

        public static World Server
        {
            get
            {
                if (_serverWorld == null || !_serverWorld.IsCreated) TryGetServerWorldAndSystems();
                return _serverWorld;
            }
        }

        public static EntityManager ServerEntityManager
        {
            get
            {
                var currentServerWorld = Server;
                if (currentServerWorld == null || !currentServerWorld.IsCreated)
                {
                    return default;
                }
                if (_serverEntityManager.Equals(default(EntityManager)) || !_serverEntityManager.World.IsCreated)
                {
                    _serverEntityManager = currentServerWorld.EntityManager;
                }
                return _serverEntityManager;
            }
        }

        public static bool IsServer
        {
            get
            {
                var currentServerWorld = Server;
                return currentServerWorld != null && currentServerWorld.IsCreated &&
                       (currentServerWorld.Flags & WorldFlags.GameServer) == WorldFlags.GameServer;
            }
        }

        private static bool IsValidNameString(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsControl(name[i]) && !char.IsWhiteSpace(name[i])) return false;
            }
            return true;
        }

        public static string GetItemName(PrefabGUID prefabGuid, EntityManager entityManager)
        {
            
            if (InternalPrefabs.NameByGuidHash != null && InternalPrefabs.NameByGuidHash.TryGetValue(prefabGuid.GuidHash, out string mappedName))
            {
                return mappedName;
            }

            if (_prefabCollectionSystem == null || !_prefabCollectionSystem.World.IsCreated)
            {
                TryGetServerWorldAndSystems();
                if (_prefabCollectionSystem == null || !_prefabCollectionSystem.World.IsCreated)
                {
                    return $"ItemGUID({prefabGuid.GuidHash})";
                }
            }

            if (entityManager.Equals(default(EntityManager)) || !entityManager.World.IsCreated)
            {
                entityManager = ServerEntityManager;
                if (entityManager.Equals(default(EntityManager)) || !entityManager.World.IsCreated)
                    return $"ItemGUID({prefabGuid.GuidHash})";
            }

            try
            {
                if (_prefabCollectionSystem.World != null && _prefabCollectionSystem.World.IsCreated)
                {
                    PrefabLookupMap prefabLookupMap = _prefabCollectionSystem.PrefabLookupMap;
                    if (prefabLookupMap.TryGetName(prefabGuid, out string directStringName) && IsValidNameString(directStringName))
                    {
                        return directStringName;
                    }
                    if (prefabLookupMap.TryGetFixedName(prefabGuid, out FixedString128Bytes fixedNameFs))
                    {
                        string fsName = fixedNameFs.ToString();
                        if (IsValidNameString(fsName))
                        {
                            return fsName;
                        }
                    }
                }
            }
            catch (Exception) { }

            return $"ItemGUID({prefabGuid.GuidHash})";
        }
    }
}