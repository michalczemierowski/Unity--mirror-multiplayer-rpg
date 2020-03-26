/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace MULTIPLAYER_GAME.Inventory.Items
{
    [CreateAssetMenu(fileName = "UsableItem_0", menuName = "UsableItem")]
    public class UsableItem : Item
    {
        [Header("Usable item data")]
        public int ArmorBonus;
        public int HealthBonus;
        public int MaxHealthBonus;
        public int StrengthBonus;
        public int IntelligenceBonus;
        public int StaminaBonus;
    }
}
