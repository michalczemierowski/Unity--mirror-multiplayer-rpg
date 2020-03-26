/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using UnityEngine;

namespace MULTIPLAYER_GAME.Inventory.Items
{
    public class ItemPickup : NetworkBehaviour
    {
        #region //======            VARIABLES           ======\\

        public short ID;
        public byte count;

        #endregion

        #region //======            MONOBEHAVIOURS           ======\\

        [Server]
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                Player player = other.GetComponent<Player>();

                // true if can add item to player's inventory
                if (player.AddItem(new ItemData(ID, count)))
                {
                    // destroy gameObject
                    NetworkServer.Destroy(gameObject);
                }
            }
        }

        #endregion
    }
}