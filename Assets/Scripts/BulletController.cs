/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Systems;
using MULTIPLAYER_GAME.UI;
using UnityEngine;

namespace MULTIPLAYER_GAME.Client
{
    public class BulletController : MonoBehaviour
    {
        #region //======            VARIABLES           ======\\

        public bool isServer        { get; private set; }                   // is bullet sent from server
        public uint attackerID      { get; private set; }                   // ID of attacker
        public string attackerName  { get; private set; }                   // Name of attacker
        public uint victimID        { get; private set; }                   // ID of victim
        public int damage           { get; private set; } = 25;             // bullet damage

        public Transform target     { get; private set; }                   // target transform
        public float speed          { get; private set; } = 10;             // bullet speed

        #endregion

        public void ClientSetTarget(Transform target, float speed)
        {
            this.target = target;
            this.speed = speed;
        }

        public void ServerSetTarget(Transform target, float speed, int damage, uint attackerID, uint victimID)
        {
            isServer = true;

            this.target = target;
            this.speed = speed;
            this.damage = damage;

            this.attackerID = attackerID;
            this.victimID = victimID;

            attackerName = ObjectDatabase.GetEntity(attackerID).Name;
        }

        #region //======            MONOBEHAVIOUR           ======\\

        private void FixedUpdate()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            if (Vector3.Distance(transform.position, target.position) > 0.25f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
            }
            else
            {
                if (isServer)
                {
                    Entity victim = ObjectDatabase.GetEntity(victimID);

                    MessageSystem.Instance.RpcAddMessageServer($"[{attackerName}] ATTACKED [{victim.Name}] FOR {damage}  | HEALTH: {victim.Health}");
                    victim.Damage(attackerID, damage);
                }
                Destroy(gameObject);
            }
        }

        #endregion
    }
}
