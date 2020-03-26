/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Systems;
using UnityEngine;

/*
 * Item data structure for network synchronization
 */

namespace MULTIPLAYER_GAME.Inventory
{
    [System.Serializable]
    public struct ItemData
    {
        public short ID;
        public short Count;

        public ItemData(short iD, short count)
        {
            ID = iD;
            Count = count;
        }
    }

    /*
     * Vector2Byte is used to synchronize cell indexes to save network bandwidth
     * 
     * Vector2Byte size - 2 bytes
     * Vector2Int size - 8 bytes
     */
    public struct Vector2Byte
    {
        public byte x, y;

        public Vector2Byte(byte x, byte y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Byte(int x, int y)
        {
            this.x = (byte)x;
            this.y = (byte)y;
        }

        public Vector2Byte(Vector2Int vector)
        {
            this.x = (byte)vector.x;
            this.y = (byte)vector.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }
    }

    [System.Serializable]
    public class SyncDictionaryInventoryData : SyncDictionary<Vector2Byte, ItemData> { }

    [System.Serializable]
    public class SyncDictionaryEquipmentData : SyncDictionary<EquipmentSlot, ItemData> { }
}
