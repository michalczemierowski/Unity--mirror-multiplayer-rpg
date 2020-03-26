/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Systems;
using UnityEngine;

namespace MULTIPLAYER_GAME.Server
{
    public class EntityManager : NetworkBehaviour
    {
        #region //======            VARIABLES           ======\\

        private Mob[] mobs;

        #endregion

        #region //======            NETWORKBEHAVIOURS           ======\\

        public override void OnStartServer()
        {
            base.OnStartServer();

            //InvokeRepeating("FindEntityDestination", 2, 2);

            for (int i = 0; i < 100; i++)
            {
                Vector3 spawnPosition = ObjectDatabase.GetRandomSpawnpoint().GetPosition();
                GameObject entity = Instantiate(ObjectDatabase.GetEntityPrefabByID(0).gameObject, spawnPosition, Quaternion.identity);
                ObjectDatabase.AddEntity(entity.GetComponent<Entity>());

                NetworkServer.Spawn(entity);
            }
        }

        #endregion

        /// <summary>
        /// Find random destination for all mob entities
        /// </summary>
        [Server]
        private void FindEntityDestination()
        {
            if (ObjectDatabase.Instance.haveEntitiesChanged)
            {
                mobs = ObjectDatabase.GetAllEntities<Mob>();
                ObjectDatabase.Instance.haveEntitiesChanged = false;
            }

            foreach (var mob in mobs)
            {
                if (mob.targetEntity)
                {
                    Debug.Log(mob.targetEntity.Name + " " + mob.targetEntity.transform.position);
                    Vector3 targetPosition = mob.targetEntity.transform.position;
                    mob.RpcSetDestination(targetPosition.x, targetPosition.z);
                    mob.agent.SetDestination(targetPosition);
                }
                else
                {
                    Vector3 position = mob.transform.position;
                    Vector3 destination = new Vector3(position.x + Random.Range(-20, 20), 1, position.z + Random.Range(-20, 20));
                    mob.RpcSetDestination(destination.x, destination.z);
                    mob.agent.SetDestination(destination);
                }
            }
        }
    }
}
