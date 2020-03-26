/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory;
using MULTIPLAYER_GAME.Inventory.Items;
using MULTIPLAYER_GAME.Inventory.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MULTIPLAYER_GAME.Systems
{
    public class EquipmentSystem : MonoBehaviour
    {
        public static EquipmentSystem Instance;                             // singleton

        #region //======            VARIABLES           ======\\

        [SerializeField] private Text statsText;
        [SerializeField] private Image equipmentHeadIcon;                   // head armor icon will be displayed in this image
        [SerializeField] private Image equipmentChestIcon;                  // chest armor icon will be displayed in this image
        [SerializeField] private Image equipmentLegsIcon;                   // legs armor icon will be displayed in this image
        [SerializeField] private Image equipmentFootsIcon;                  // foots armor icon will be displayed in this image

        [SerializeField] private Image equipmentWeaponIcon;                 // weapon icon will be displayed in this image

        private Player localPlayer;                                         // local player reference

        #endregion

        #region //======            MONOBEHAVIOURS           ======\\

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);
        }

        private void Update()
        {
            if (!localPlayer) return;

            string text =
                    $"Armor: {localPlayer.Armor}\n" +
                    $"Health: {localPlayer.Health}\n" +
                    $"Max health: {localPlayer.maxHealth}\n" +
                    $"Strength: {localPlayer.Strength}\n" +
                    $"Intelligence: {localPlayer.Intelligence}\n" +
                    $"Stamina: {localPlayer.Stamina}\n";

            statsText.text = text;
        }

        #endregion

        public static void SetPlayer(Player player)
        {
            Instance.localPlayer = player;
        }                       // set player reference

        /// <summary>
        /// Called on equipment update
        /// </summary>
        /// <param name="op">Operation (SET/UPDATE/REMOVE...)</param>
        /// <param name="key">Equipment slot</param>
        /// <param name="item">Item data</param>
        public static void OnEquipmentUpdate(SyncIDictionary<EquipmentSlot, ItemData>.Operation op, EquipmentSlot key, ItemData item)
        {
            switch(op)
            {
                case SyncIDictionary<EquipmentSlot, ItemData>.Operation.OP_SET:
                case SyncIDictionary<EquipmentSlot, ItemData>.Operation.OP_ADD:
                    Equipment equipment = (Equipment)ObjectDatabase.GetItem(item.ID);
                    if (equipment)
                    {
                        Instance.SetIcon(key, equipment.icon);
                    }
                    break;
                case SyncIDictionary<EquipmentSlot, ItemData>.Operation.OP_REMOVE:
                    Instance.SetIcon(key, null);
                    break;
            }
        }

        /// <summary>
        /// Set equipment slot icon
        /// </summary>
        /// <param name="slot">Equipment slot</param>
        /// <param name="icon">Icon</param>
        private void SetIcon(EquipmentSlot slot, Sprite icon)
        {
            switch(slot)
            {
                case EquipmentSlot.HEAD:
                    equipmentHeadIcon.sprite = icon;
                    break;
                case EquipmentSlot.CHEST:
                    equipmentChestIcon.sprite = icon;
                    break;
                case EquipmentSlot.LEGS:
                    equipmentLegsIcon.sprite = icon;
                    break;
                case EquipmentSlot.FOOTS:
                    equipmentFootsIcon.sprite = icon;
                    break;
                case EquipmentSlot.WEAPON:
                    equipmentWeaponIcon.sprite = icon;
                    break;
            }
        }
    }

    public enum EquipmentSlot : byte
    {
        HEAD,
        CHEST,
        LEGS,
        FOOTS,
        WEAPON
    }
}
