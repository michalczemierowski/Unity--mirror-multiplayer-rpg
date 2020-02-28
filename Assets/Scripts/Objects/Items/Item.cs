/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace MULTIPLAYER_GAME.Inventory
{
    [CreateAssetMenu(fileName = "Item_0", menuName = "Item")]
    public class Item : ScriptableObject
    {
        [Header("Item data")]
        public int ID;
        public Sprite icon;
        // TODO
    }
}
