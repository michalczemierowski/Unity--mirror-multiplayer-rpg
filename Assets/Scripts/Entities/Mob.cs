/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Interfaces;
using MULTIPLAYER_GAME.Systems;

namespace MULTIPLAYER_GAME.Entities
{
    public class Mob : Entity, IEntityMovement
    {
        public override void Start()
        {
            base.Start();
        }

        // SERVER ONLY
        public override void Damage(int attackerID, float value)
        {
            base.Damage(attackerID, value);

            if (Health <= 0)
            {
                Destroy(gameObject);
            }
        }

        public override void SetHealth(float Health)
        {
            base.SetHealth(Health);

            if (Health <= 0)
            {
                Destroy(gameObject);
            }
        }

        #region IEntityMovement

        [ClientRpc]
        public void RpcSetDestination(float x, float z)
        {
            agent.SetDestination(new UnityEngine.Vector3(x, 1, z));
        }

        #endregion
    }
}
