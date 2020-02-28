/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace MULTIPLAYER_GAME.Systems
{
    public class ObjectDatabase : NetworkBehaviour
    {
        public static ObjectDatabase Instance;

        [SyncVar]
        public int indexID;

        [HideInInspector] public bool haveEntitiesChanged = true;
        private Dictionary<int, Entity> allEntities = new Dictionary<int, Entity>();
        private List<EntitySpawnPosition> entitySpawnPositions = new List<EntitySpawnPosition>();

        [SerializeField] private Entity[] entityPrefabs;

        [Space]
        private string weaponResoucePath = "/Weapons";
        private Dictionary<int, Weapon> allWeapons = new Dictionary<int, Weapon>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);

            foreach (var entity in FindObjectsOfType<Entity>())
            {
                //AddEntity(entity);
            }

            foreach (var spawn in FindObjectsOfType<EntitySpawnPosition>())
            {
                entitySpawnPositions.Add(spawn);
            }
        }

        public static Entity GetEntity(int ID)
        {
            Entity entity;
            Instance.allEntities.TryGetValue(ID, out entity);
            return entity;
        }

        public static Player GetPlayerEntity(int ID)
        {
            Entity entity;
            Instance.allEntities.TryGetValue(ID, out entity);
            if (entity.GetType() == typeof(Player))
                return (Player)entity;
            return null;
        }

        public static Entity[] GetAllEntities()
        {
            List<Entity> list = new List<Entity>(Instance.allEntities.Values);
            return list.ToArray();
        }

        public static T[] GetAllEntities<T>() where T : Entity
        {
            List<T> result = new List<T>();
            foreach (var entity in GetAllEntities())
            {
                if (entity.GetType() == typeof(T))
                    result.Add((T)entity);
            }
            return result.ToArray();
        }

        public static void AddEntity(Entity entity)
        {
            if (Instance.isServer)
                entity.ID = Instance.indexID++;
            Instance.allEntities.Add(entity.ID, entity);

            Instance.haveEntitiesChanged = true;
        }

        public static int NextEntityID()
        {
            return Instance.indexID++;
        }

        public static void RemoveEntity(Entity entity)
        {
            Instance.allEntities.Remove(entity.ID);

            Instance.haveEntitiesChanged = true;
        }
        public static void RemoveEntity(int entityID)
        {
            if (Instance.allEntities.ContainsKey(entityID))
            {
                Instance.allEntities.Remove(entityID);

                Instance.haveEntitiesChanged = true;
            }
        }

        public static EntitySpawnPosition GetRandomSpawnpoint()
        {
            return Instance.entitySpawnPositions[Random.Range(0, Instance.entitySpawnPositions.Count)];
        }

        public static Entity GetEntityPrefabByID(int ID)
        {
            if (Instance.entityPrefabs.Length > ID)
                return Instance.entityPrefabs[ID];

            return null;
        }

        public static void GetPosition(out int[] IDs, out Vector3[] positions)
        {
            IDs = new int[Instance.allEntities.Count];
            positions = new Vector3[Instance.allEntities.Count];

            int i = 0;
            foreach (var key in Instance.allEntities.Keys)
            {
                IDs[i] = key;
                positions[i] = Instance.allEntities[key].agent.nextPosition;
                i++;
            }
        }

        public static void SetPosition(int[] IDs, Vector3[] positions)
        {
            int i = 0;
            foreach (int ID in IDs)
            {
                Entity entity = GetEntity(ID);
                Vector3 destination = entity.agent.destination;
                entity.agent.Warp(positions[i]);
                entity.agent.SetDestination(destination);
                i++;
            }
        }
    }
}
