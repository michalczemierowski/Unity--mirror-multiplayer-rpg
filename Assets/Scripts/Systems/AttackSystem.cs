/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Client;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory.Items;
using UnityEngine;

namespace MULTIPLAYER_GAME.Systems
{
    public class AttackSystem : NetworkBehaviour
    {
        public static AttackSystem Instance;

        #region //======            MONOBEHAVIOURS           ======\\

        private void Awake()
        {
            // SINGLETON
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);
        }

        #endregion

        [Server]
        public void ServerFireAtTarget(uint attackerEntityID, uint targetEntityID, short weaponID, bool rotateToTarget = true)
        {
            Weapon weapon = ObjectDatabase.GetWeapon(weaponID);
            if (weapon)
            {
                Entity targetEntity = ObjectDatabase.GetEntity(targetEntityID);
                Entity attackerEntity = ObjectDatabase.GetEntity(attackerEntityID);

                if (!targetEntity || !attackerEntity) return;

                // bullet
                RpcFireAtTarget(attackerEntityID, targetEntityID, weaponID, rotateToTarget);

                GameObject bullet = Instantiate(weapon.bulletPrefab, attackerEntity.transform.position, Quaternion.identity);

                BulletController bulletController = bullet.GetComponent<BulletController>();
                bulletController.ServerSetTarget(targetEntity.transform, weapon.bulletSpeed, weapon.Damage, attackerEntityID, targetEntityID);
            }
        }

        [ClientRpc]
        public void RpcFireAtTarget(uint attackerEntityID, uint targetEntityID, short weaponID, bool rotateToTarget)
        {
            Weapon weapon = ObjectDatabase.GetWeapon(weaponID);
            if (weapon)
            {
                Entity targetEntity = ObjectDatabase.GetEntity(targetEntityID);
                Entity attackerEntity = ObjectDatabase.GetEntity(attackerEntityID);

                if (!targetEntity) return;

                attackerEntity.SetTrigger(weapon.animationTrigger);
                attackerEntity.BlockMovement(weapon.cantMoveTime);

                if (rotateToTarget)
                    RotateTowards(attackerEntity.transform, targetEntity.transform.position);

                GameObject bullet = Instantiate(weapon.bulletPrefab, attackerEntity.transform.position, Quaternion.identity);

                BulletController bulletController = bullet.GetComponent<BulletController>();
                bulletController.ClientSetTarget(targetEntity.transform, weapon.bulletSpeed);
            }
        }


        /// <summary>
        ///  Rotate entity towards position
        /// </summary>
        /// <param name="position"></param>
        public static void RotateTowards(Transform entity, Vector3 position)
        {
            var lookPos = position - entity.transform.position;
            lookPos.y = 0;
            entity.transform.rotation = Quaternion.LookRotation(lookPos);
        }
    }
}
