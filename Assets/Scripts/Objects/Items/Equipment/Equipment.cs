/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory.UI;
using UnityEngine;

namespace MULTIPLAYER_GAME.Inventory.Items
{
    [CreateAssetMenu(fileName = "Equipment_0", menuName = "Equipment")]
    public class Equipment : Item
    {
        [Header("Equipment data")]
        public Systems.EquipmentSlot equipmentSlot;
        public int ArmorBonus;
        public int HealthBonus;
        public int StrengthBonus;
        public int IntelligenceBonus;
        public int StaminaBonus;

        /// <summary>
        /// Equip item
        /// </summary>
        /// <param name="position"></param>
        public override void UseItem(Vector2Byte position)
        {
            // get inventory cell
            InventoryCellUI cell = InventorySystem.Instance.GetInventoryCell(position.x, position.y);

            // check if player can set equipment and remove item from toolbar if so
            if (cell && (!Player.localPlayer.equipmentData.ContainsKey(equipmentSlot) || Player.localPlayer.equipmentData[equipmentSlot].Count == 0 || Player.localPlayer.equipmentData[equipmentSlot].ID != ID))
            {
                // clear cell data
                cell.ClearData();
                cell.toolbarIndex = -1;

                // equip item
                Player.localPlayer.CmdSetEquipment(equipmentSlot, new ItemData(ID, 1), position);
            }
        }
    }
}
