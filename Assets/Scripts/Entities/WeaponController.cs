/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory;
using MULTIPLAYER_GAME.Inventory.Items;
using MULTIPLAYER_GAME.Systems;
using UnityEngine;

namespace MULTIPLAYER_GAME.Client
{
    public class WeaponController : NetworkBehaviour
    {
        public float cooldown { get; set; }                     // attack cooldown
        Player player;                                          // Player reference

        #region //======            CMD | RPC           ======\\

        /// <summary>
        /// [Command] Send command to fire at target entity
        /// </summary>
        /// <param name="targetEntityID">target entity ID</param>
        [Command]
        public void CmdFireAtTarget(uint targetEntityID)
        {
            if (cooldown <= 0)
            {
                if (!player) return;

                Weapon weapon = player.usedWeapon;

                if (weapon)
                {
                    Entity targetEntity = ObjectDatabase.GetEntity(targetEntityID);

                    if (!targetEntity) return;

                    AttackSystem.Instance.ServerFireAtTarget(player.ID, targetEntity.ID, weapon.ID);
                }
            }
        }

        /// <summary>
        /// [ClientRpc] Fire at target entity
        /// </summary>
        /// <param name="entityID">attacker entity ID</param>
        /// <param name="targetEntityID">target entity ID</param>
        /// <param name="weaponID">attacker weapon ID</param>
        [ClientRpc]
        public void RpcFireAtTarget(uint entityID, uint targetEntityID, short weaponID)
        {
            Entity entity = ObjectDatabase.GetEntity(entityID);
            Weapon weapon = ObjectDatabase.GetWeapon(weaponID);

            player.SetTrigger(weapon.animationTrigger);

            // bullet
            BulletController bulletController = Instantiate(weapon.bulletPrefab, entity.transform.position, Quaternion.identity).GetComponent<BulletController>();
            Entity targetEntity = ObjectDatabase.GetEntity(targetEntityID);

            RotateTowards(targetEntity.transform.position);

            //bulletController.target = targetEntity.transform;
            //bulletController.speed = weapon.bulletSpeed;

            entity.BlockMovement(weapon.cantMoveTime);
        }

        /// <summary>
        /// [Command] Send command to melee attack target entity
        /// </summary>
        /// <param name="targetEntityID">target entity ID</param>
        [Command]
        public void CmdMeleeAttack(uint targetEntityID)
        {
#if SERVER
            if (cooldown <= 0)
            {
                if (!player) return;

                Weapon weapon = player.usedWeapon;

                if (weapon)
                {
                    Entity targetEntity = ObjectDatabase.GetEntity(targetEntityID);

                    if (!targetEntity) return;

                    if (Vector3.Distance(targetEntity.transform.position, transform.position) <= weapon.attackRange + 2)
                    {
                        targetEntity.Damage(player.ID, weapon.Damage);

                        RpcMeleeAttack(player.ID, targetEntityID, weapon.ID);
                        cooldown = weapon.attackCooldown;
                    }
                }
            }
#endif
        }

        /// <summary>
        /// [ClientRpc] Melee attack target entity
        /// </summary>
        /// <param name="entityID">attacker entity ID</param>
        /// <param name="targetEntityID">target entity ID</param>
        /// <param name="weaponID">attacker weapon ID</param>
        [ClientRpc]
        public void RpcMeleeAttack(uint entityID, uint targetEntityID, short weaponID)
        {
            Entity entity = ObjectDatabase.GetEntity(entityID);
            Weapon weapon = ObjectDatabase.GetWeapon(weaponID);
            Entity targetEntity = ObjectDatabase.GetEntity(targetEntityID);

            RotateTowards(targetEntity.transform.position);

            entity.SetTrigger(weapon.animationTrigger);
            entity.BlockMovement(weapon.cantMoveTime);
        }

        #endregion

        #region //======            MONOBEHAVIOURS           ======\\

        private void Update()
        {
            if (cooldown > 0)
                cooldown -= Time.deltaTime;
        }

        private void Start()
        {
            player = GetComponent<Player>();
        }

        #endregion

        /// <summary>
        ///  Rotate player towards position
        /// </summary>
        /// <param name="position"></param>
        private void RotateTowards(Vector3 position)
        {
            var lookPos = position - transform.position;
            lookPos.y = 0;
            transform.rotation = Quaternion.LookRotation(lookPos);
        }
    }
}
