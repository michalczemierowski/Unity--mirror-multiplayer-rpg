/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace MULTIPLAYER_GAME.Entities
{
    public class EntitySpawnPosition : MonoBehaviour
    {
        public float spawnRange = 1;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(spawnRange, 0, spawnRange));
        }

        public Vector3 GetPosition()
        {
            Vector3 position = transform.position;

            float halfRange = spawnRange / 2;
            position += new Vector3(Random.Range(-halfRange, halfRange), 0, Random.Range(-halfRange, halfRange));

            return position;
        }
    }
}
