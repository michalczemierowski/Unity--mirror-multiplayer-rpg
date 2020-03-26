/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Database;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory;
using MULTIPLAYER_GAME.Inventory.Items;
using MULTIPLAYER_GAME.Systems;
using System.Collections.Generic;
using UnityEngine;

public class SERVERNetworkManager : NetworkManager
{
    public class CreateCharacterMessage : MessageBase
    {
        public int ID;
        public string Name;
    }                                                                   // connection request message base

    #region //======            NETWORKBEHAVIOURS           ======\\

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CreateCharacterMessage>(OnCreateCharacter);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        // load player nickname from PlayerPrefs and send connect request to server
        CreateCharacterMessage characterMessage = new CreateCharacterMessage
        {
            ID = 0,
            Name = PlayerPrefs.GetString("nickname")
        };

        conn.Send(characterMessage);
    }

    #endregion

    private void OnCreateCharacter(NetworkConnection conn, CreateCharacterMessage createCharacterMessage)
    {
        Player player = Instantiate(playerPrefab).GetComponent<Player>();
        player.Name = createCharacterMessage.Name;                      // assing player name

        CharacterData data = Database.LoadCharacter(player.Name);       // load data from database
        if (data != null)
        {
            Dictionary<Vector2Byte, ItemData> inventoryData = Database.StringToItemData(data.inventory);
            foreach (var key in inventoryData.Keys)                     // add items to inventory
            {
                player.inventoryData.Add(key, inventoryData[key]);
            }

            Dictionary<EquipmentSlot, ItemData> equipmentData = Database.StringToEquipmentData(data.equipment);
            foreach (var key in equipmentData.Keys)                     // add equipment ant its bonuses to player
            {
                EquipmentSlot ID = key;
                ItemData itemData = equipmentData[key];

                if (ID < 0 || (int)ID > 4) continue;

                Item item = ObjectDatabase.GetItem(itemData.ID);
                if (item && item is Equipment)
                {
                    Equipment equipment = (Equipment)item;
                    player.equipmentData.Add(key, equipmentData[key]);

                    player.Armor += equipment.ArmorBonus;
                    player.maxHealth += equipment.HealthBonus;
                    player.Strength += equipment.StrengthBonus;
                    player.Intelligence += equipment.IntelligenceBonus;
                    player.Stamina += equipment.StaminaBonus;

                    if (item.GetType() == typeof(Weapon))
                    {
                        player.usedWeapon = (Weapon)item;
                        player.usedWeaponID = item.ID;
                    }
                }
            }

            player.transform.position = data.position;                      // move player to his last position
            player.Health = data.Health;                                    // set player's health
            player.Experience = data.experience;                            // set player's experience
            player.usedWeapon = ObjectDatabase.GetWeapon(data.usedWeapon);  // set player's weapon
        }

        ObjectDatabase.AddEntity(player);                                   // add player to entity database

        NetworkServer.AddPlayerForConnection(conn, player.gameObject);
    }   // Load character data from database
}
