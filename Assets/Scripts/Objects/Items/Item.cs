/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Interfaces;
using UnityEngine;

/*
 * Base class for all items, equipment etc.
 */

namespace MULTIPLAYER_GAME.Inventory.Items
{
    [CreateAssetMenu(fileName = "Item_0", menuName = "Item")]
    public class Item : ScriptableObject
    {
        [Header("Item data")]
        public short ID;
        public Sprite icon;

        public virtual void UseItem(Vector2Byte position)
        {
            Player.localPlayer.CmdUseItem(position);
        }
    }
}
