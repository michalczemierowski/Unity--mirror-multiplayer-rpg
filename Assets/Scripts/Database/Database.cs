/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using Mirror;
using System;
using System.IO;
using System.Collections.Generic;
using SQLite;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory;
using MULTIPLAYER_GAME.Systems;


/*
 * Database connection class
 */
namespace MULTIPLAYER_GAME.Database
{
    public static class Database
    {
        public static string databaseFile = "Database.sqlite";                      // path to database file
        static SQLiteConnection connection;                                         // reference to connection

        #region //======            DATABASE METHODS           ======\\

        /// <summary>
        /// Initialize database - open connection and create tables if they don't exist
        /// </summary>
        public static void Init()
        {
            string path = Path.Combine(Application.dataPath, databaseFile);
            connection = new SQLiteConnection(path);

            connection.CreateTable<character>();
        }

        /// <summary>
        /// Close database connection if connected
        /// </summary>
        public static void CloseConnection()
        {
            connection?.Close();
        }

        #endregion

        #region //======            DATABASE TABLES           ======\\

        class character     // character table
        {
            [PrimaryKey]
            [Indexed]
            [Collation("NOCASE")]
            public string name { get; set; }

            // transform
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }

            // attributes
            public int Health { get; set; }

            public long experience { get; set; }
            public long coins { get; set; }

            // weapons
            public short usedWeapon { get; set; }

            // inventory
            public string inventory { get; set; }

            // equipment
            public string equipment { get; set; }
        }

        #endregion

        #region //======            BOOLS           ======\\

        public static bool CharacterExists(string characterName)
        {
            return connection.FindWithQuery<character>("SELECT * FROM character WHERE name=?", characterName) != null;
        }

        #endregion

        #region //======            OBJECTS TO STRINGS | STRINGS TO OBJECTS           ======\\

        public static string ItemDataToString(SyncDictionaryInventoryData itemData)
        {
            string result = string.Empty;

            foreach (var key in itemData.Keys)
            {
                ItemData data = itemData[key];
                string item = key.x + ";" + key.y + ";" + data.ID + ";" + data.Count + "|";
                result += item;
            }

            return result;
        }

        public static string EquipmentDataToString(SyncDictionaryEquipmentData equipmentData)
        {
            string result = string.Empty;

            foreach (var key in equipmentData.Keys)
            {
                ItemData data = equipmentData[key];
                string item = (int)key + ";" + data.ID + ";" + data.Count + "|";
                result += item;
            }

            return result;
        }

        public static Dictionary<Vector2Byte, ItemData> StringToItemData(string itemData)
        {
            Dictionary<Vector2Byte, ItemData> result = new Dictionary<Vector2Byte, ItemData>();

            string[] items = itemData.Split('|');
            foreach (string line in items)
            {
                string[] item = line.Split(';');

                if (item.Length != 4) continue;

                Vector2Byte position = new Vector2Byte(int.Parse(item[0]), int.Parse(item[1]));
                short ID = short.Parse(item[2]);
                short Count = short.Parse(item[3]);

                result.Add(position, new ItemData(ID, Count));
            }

            return result;
        }

        public static Dictionary<EquipmentSlot, ItemData> StringToEquipmentData(string equipmentData)
        {
            Dictionary<EquipmentSlot, ItemData> result = new Dictionary<EquipmentSlot, ItemData>();

            string[] items = equipmentData.Split('|');
            foreach (string line in items)
            {
                string[] item = line.Split(';');

                if (item.Length != 3) continue;

                EquipmentSlot slot = (EquipmentSlot)int.Parse(item[0]);
                short ID = short.Parse(item[1]);
                short Count = short.Parse(item[2]);

                result.Add(slot, new ItemData(ID, Count));
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Save character to database
        /// </summary>
        /// <param name="player">Player to save</param>
        public static void SaveCharacter(CharacterData player)
        {
            connection.InsertOrReplace(new character
            {
                name = player.Name,

                // transform
                x = player.position.x,
                y = player.position.y,
                z = player.position.z,

                // attributes
                Health = player.Health,

                experience = player.experience,
                coins = player.coins,

                // weapons
                usedWeapon = player.usedWeapon,

                // inventory
                inventory = player.inventory,

                equipment = player.equipment
            });
        }

        /// <summary>
        /// Load character from database
        /// </summary>
        /// <param name="Name">Player nickname</param>
        /// <returns>Character data</returns>
        public static CharacterData LoadCharacter(string Name)
        {
            CharacterData result = null;
            var player = connection.FindWithQuery<character>("SELECT * FROM character WHERE name=?", Name);
            if (player != null)
            {
                result = new CharacterData()
                {
                    Name = player.name,
                    position = new Vector3(player.x, player.y, player.z),
                    Health = player.Health,
                    experience = player.experience,
                    coins = player.coins,
                    usedWeapon = player.usedWeapon,
                    inventory = player.inventory,
                    equipment = player.equipment
                };
            }

            return result;
        }
    }

    /*
     * Class containing player data
     */
    public class CharacterData
    {
        public string Name;
        public Vector3 position;

        public int Health;

        public long experience;
        public long coins;

        public short usedWeapon;

        public string inventory;
        public string equipment;

        public CharacterData() { }

        public CharacterData(Vector3 position, int health, long experience, long coins, short usedWeapon, string inventory, string equipment)
        {
            this.position = position;
            Health = health;
            this.experience = experience;
            this.coins = coins;
            this.usedWeapon = usedWeapon;
            this.inventory = inventory;
            this.equipment = equipment;
        }

        public static CharacterData FromPlayerEntity(Player player)
        {
            if (player == null) return null;

            CharacterData result = new CharacterData()
            {
                Name = player.Name,
                position = player.transform.position,

                Health = player.Health,

                experience = player.Experience,
                coins = 0,              // TODO

                usedWeapon = player.usedWeapon.ID,

                inventory = Database.ItemDataToString(player.inventoryData),
                equipment = Database.EquipmentDataToString(player.equipmentData)
            };
            return result;
        }
    }
}
