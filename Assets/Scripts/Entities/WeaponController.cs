/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory;
using MULTIPLAYER_GAME.Systems;
using UnityEngine;

namespace MULTIPLAYER_GAME.Client
{
    public class WeaponController : NetworkBehaviour
    {
        public float cooldown { get; set; }
        Player player;

        [Command]
        public void CmdFireAtTarget(int targetEntityID)
        {
            if (cooldown <= 0)
            {
                if (!player) return;

                Weapon weapon = player.usedWeapon;

                if (weapon)
                {
                    Entity targetEntity = ObjectDatabase.GetEntity(targetEntityID);

                    if (!targetEntity) return;

                    RpcFireAtTarget(player.ID, targetEntityID, weapon.ID);
                    cooldown = weapon.attackCooldown;

                    // bullet
                    BulletController bulletController = Instantiate(weapon.bulletPrefab, transform.position, Quaternion.identity).GetComponent<BulletController>();

                    bulletController.isServer = true;
                    bulletController.attackerID = player.ID;
                    bulletController.target = targetEntity.transform;
                    bulletController.damage = weapon.Damage;
                    bulletController.speed = weapon.bulletSpeed;
                }
            }
        }

        [ClientRpc]
        public void RpcFireAtTarget(int entityID, int targetEntityID, int weaponID)
        {
            Entity entity = ObjectDatabase.GetEntity(entityID);
            Weapon weapon = entity.usedWeapon;

            player.SetTrigger(weapon.animationTrigger);

            // bullet
            BulletController bulletController = Instantiate(weapon.bulletPrefab, entity.transform.position, Quaternion.identity).GetComponent<BulletController>();
            Entity targetEntity = ObjectDatabase.GetEntity(targetEntityID);

            RotateTowards(targetEntity.transform.position);

            bulletController.attackerID = player.ID;
            bulletController.target = targetEntity.transform;
            bulletController.damage = weapon.Damage;
            bulletController.speed = weapon.bulletSpeed;

            entity.StartCoroutine(entity.BlockMovement(weapon.cantMoveTime));
        }

        [Command]
        public void CmdMeleeAttack(int targetEntityID)
        {
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
        }

        [ClientRpc]
        public void RpcMeleeAttack(int entityID, int targetEntityID, int weaponID)
        {
            Entity entity = ObjectDatabase.GetEntity(entityID);
            // TODO: USE WEAPON ID
            Weapon weapon = entity.usedWeapon;
            Entity targetEntity = ObjectDatabase.GetEntity(targetEntityID);

            RotateTowards(targetEntity.transform.position);

            entity.SetTrigger(weapon.animationTrigger);
            entity.StartCoroutine(entity.BlockMovement(weapon.cantMoveTime));
        }

        private void Update()
        {
            if (cooldown > 0)
                cooldown -= Time.deltaTime;
        }

        private void Start()
        {
            player = GetComponent<Player>();
        }

        private void RotateTowards(Vector3 position)
        {
            var lookPos = position - transform.position;
            lookPos.y = 0;
            transform.rotation = Quaternion.LookRotation(lookPos);
        }
    }
}
