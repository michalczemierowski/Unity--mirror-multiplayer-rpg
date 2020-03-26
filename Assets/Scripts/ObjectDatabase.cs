/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory.Items;
using System.Collections.Generic;
using UnityEngine;

/*
 * Oject Database is object that contains information about all entities in game.
 * for example, when sending attack message you send victim ID, and then get Enity object by ID from Object Database
 */

namespace MULTIPLAYER_GAME.Systems
{
    public class ObjectDatabase : NetworkBehaviour
    {
        public static ObjectDatabase Instance;

        [SyncVar] public uint indexID;                                                                   // next entity ID

        // INSTANTIATED OBJECTS
        private Dictionary<long, Entity> allEntities = new Dictionary<long, Entity>();                  // all Entities on map
        private List<EntitySpawnPosition> entitySpawnPositions = new List<EntitySpawnPosition>();       // Spawn positions for entities

        [Header("Entities")]
        [SerializeField] private Entity[] entityPrefabs;                                                // Entities prefabs
        [HideInInspector] public bool haveEntitiesChanged;                                              // has all entities dictionary changed

        [Header("Objects")]
        public List<Weapon> _allWeapons;                                                                // All weapons dictionary, used becouse Unity does not support dictionary serialization
        public List<Item> _allItems;                                                                    // List is set to null after assigning values to dictionary

        private Dictionary<short, Weapon> allWeapons = new Dictionary<short, Weapon>();                     // Copy of '_allWeapons' containing all weapons
        private Dictionary<short, Item> allItems = new Dictionary<short, Item>();                           // Copy of '_allItems' containing all items

        #region //======            MONOBEHAVIOURS           ======\\

        private void Awake()
        {
            // SINGLETON
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);

            // FIND ALL SPAWNPOINTS
            foreach (var spawn in FindObjectsOfType<EntitySpawnPosition>())
            {
                entitySpawnPositions.Add(spawn);
            }

            // LOAD ALL WEAPONS
            foreach (Weapon weapon in _allWeapons)
            {
                allWeapons.Add(weapon.ID, weapon);
            }

            // LOAD ALL ITEMS
            foreach (Item item in _allItems)
            {
                allItems.Add(item.ID, item);
            }

            _allItems = null;       // set lists to null
            _allWeapons = null;
        }

        #endregion

        #region //======            NETWORKBEHAVIOURS           ======\\

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<PositionMessage>(ServerPositionMessageHandler);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkClient.RegisterHandler<PositionMessage>(ClientPositionMessageHandler);
        }

        #endregion

        #region //======            ENTITY METHODS           ======\\

        public static Entity GetEntity(uint ID)
        {
            Entity entity;
            Instance.allEntities.TryGetValue(ID, out entity);
            return entity;
        }

        public static Player GetPlayerEntity(uint ID)
        {
            Entity entity;
            Instance.allEntities.TryGetValue(ID, out entity);
            if (entity && entity.GetType() == typeof(Player))
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

        public static uint NextEntityID()
        {
            return Instance.indexID++;
        }

        public static void RemoveEntity(Entity entity)
        {
            Instance.allEntities.Remove(entity.ID);
            Instance.haveEntitiesChanged = true;
        }
        public static void RemoveEntity(uint entityID)
        {
            if (Instance.allEntities.ContainsKey(entityID))
            {
                Instance.allEntities.Remove(entityID);
                Instance.haveEntitiesChanged = true;
            }
        }

        public static EntitySpawnPosition GetRandomSpawnpoint()
        {
            return Instance.entitySpawnPositions[UnityEngine.Random.Range(0, Instance.entitySpawnPositions.Count)];
        }

        public static Entity GetEntityPrefabByID(uint ID)
        {
            if (Instance.entityPrefabs.Length > ID)
                return Instance.entityPrefabs[ID];

            return null;
        }

        #endregion

        #region //======            ITEM METHODS           ======\\

        public static void AddItem(Item item)
        {
            if (!Instance.allItems.ContainsKey(item.ID))
                Instance.allItems.Add(item.ID, item);
        }

        public static void AddWeapon(Weapon weapon)
        {
            if (!Instance.allWeapons.ContainsKey(weapon.ID))
                Instance.allWeapons.Add(weapon.ID, weapon);
        }

        public static Weapon GetWeapon(short ID)
        {
            Weapon weapon = null;
            Instance.allWeapons.TryGetValue(ID, out weapon);
            return weapon;
        }

        public static Item GetItem(short ID)
        {
            Item item = null;
            Instance.allItems.TryGetValue(ID, out item);
            return item;
        }

        #endregion

        #region //======            POSITION SYNC           ======\\

        public class PositionMessage : MessageBase
        {
            public uint ID;
            public Vector3 position;
            public Vector3 destination;
        }

        public void GetEntityPosition(uint entityID)
        {
            PositionMessage msg = new PositionMessage()
            {
                ID = entityID
            };
            if (!isServer)
                NetworkClient.Send(msg);
        }

        private void ServerPositionMessageHandler(NetworkConnection conn, PositionMessage msg)
        {
            Entity entity = GetEntity(msg.ID);
            PositionMessage nmsg = new PositionMessage()
            {
                ID = entity.ID,
                position = entity.agent.nextPosition,
                destination = entity.agent.destination
            };
            conn.Send(nmsg);
        }

        private void ClientPositionMessageHandler(NetworkConnection conn, PositionMessage msg)
        {
            Entity entity = GetEntity(msg.ID);
            if (entity && entity.gameObject)
            {
                entity.gameObject.SetActive(true);
                entity.agent.Warp(msg.position);
                entity.agent.SetDestination(msg.destination);
            }
        }

        #endregion
    }
}
