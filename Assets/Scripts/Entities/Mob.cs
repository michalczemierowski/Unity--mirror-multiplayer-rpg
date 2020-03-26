/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Interfaces;
using MULTIPLAYER_GAME.Systems;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace MULTIPLAYER_GAME.Entities
{
    public class Mob : Entity, IEntityMovement
    {
        [SerializeField] private Material onDamageMaterial;                         // material applied to MeshRenderer in OnDamage event

        [HideInInspector] public Entity targetEntity;                               // target entity - will be followed and attacked when in range

        private Material startMaterial;                                             // default material
        private MeshRenderer meshRenderer;                                          // entity mesh renderer reference used in OnDamage

        [SerializeField] private float destinationCooldownWithTarget = 1f;          // find destination cooldown if there is target
        [SerializeField] private float destinationCooldownWithoutTarget = 3f;       // find destination cooldown when there is no target

        private float findDestinationCooldown;                                      // time to pick next destination
        private float attackCooldown;                                               // time to next attack

        #region //======            MONOBEHAVIOURS           ======\\

        public override void Start()
        {
            base.Start();
            meshRenderer = GetComponent<MeshRenderer>();
            if(!meshRenderer) meshRenderer = GetComponentInChildren<MeshRenderer>();

            startMaterial = meshRenderer.sharedMaterial;

            EventOnDamage += OnDamage;
            EventOnHeal += OnHeal;
        }

        [Server]
        private void Update()
        {
            if (findDestinationCooldown > 0)
                findDestinationCooldown -= Time.deltaTime;
            else
                FindDestination();

            if (targetEntity)
            {
                if (attackCooldown <= 0 && Vector3.Distance(targetEntity.transform.position, transform.position) <= usedWeapon.attackRange)
                    Attack();
            }

            if (attackCooldown > 0)
                attackCooldown -= Time.deltaTime;
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (!usedWeapon) return;

            Color c = Color.red;
            Handles.color = c;
            Handles.DrawWireDisc(transform.position, transform.up, usedWeapon.attackRange);
            c.a = 0.02f;
            Handles.color = c;
            Handles.DrawSolidDisc(transform.position, transform.up, usedWeapon.attackRange);
        }

#endif

        #endregion

        #region //======            HOOKS           ======\\

        /// <summary>
        /// Called on weapon change
        /// </summary>
        /// <param name="oldWeaponID">old weapon ID</param>
        /// <param name="newWeaponID">new weapon ID</param>
        public override void OnChangeWeapon(short oldWeaponID, short newWeaponID)
        {
            base.OnChangeWeapon(oldWeaponID, newWeaponID);

            // reset attack cooldown
            attackCooldown = usedWeapon.attackCooldown;
        }

        #endregion

        #region //======            EVENT LISTENERS           ======\\

        public override void OnHeal(int value)
        {
            base.OnHeal(value);
        }

        public override void OnDamage(uint attackerID, int value)
        {
            base.OnDamage(attackerID, value);

            StartCoroutine(OnDamageCoroutine());
        }

        #endregion

        #region //======            SERVER           ======\\

        /// <summary>
        /// Damage entity
        /// </summary>
        /// <param name="attackerID">ID of attacker</param>
        /// <param name="damage">damage</param>
        [Server]
        public override void Damage(uint attackerID, int damage)
        {
            base.Damage(attackerID, damage);

            if (!targetEntity && usedWeapon)
                attackCooldown = usedWeapon.attackCooldown;

            targetEntity = ObjectDatabase.GetEntity(attackerID);

            if (Health <= 0)
            {
                NetworkServer.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Find next NavMesh destination for entity
        /// </summary>
        [Server]
        public void FindDestination()
        {
            if(targetEntity)
            {
                // move to target
                Vector3 direction = (transform.position - targetEntity.transform.position).normalized;
                Vector3 destination = usedWeapon.isRanged ? targetEntity.transform.position - direction + (direction * usedWeapon.attackRange) : targetEntity.transform.position;

                RpcSetDestination(destination.x, destination.z);
                agent.SetDestination(destination);

                // reset cooldown
                findDestinationCooldown = destinationCooldownWithTarget;
            }
            else
            {
                // move to random position in range (-20, 20)
                Vector3 position = transform.position;
                Vector3 destination = new Vector3(position.x + Random.Range(-20, 20), 1, position.z + Random.Range(-20, 20));

                RpcSetDestination(destination.x, destination.z);
                agent.SetDestination(destination);

                findDestinationCooldown = destinationCooldownWithoutTarget;
            }
        }

        /// <summary>
        /// Attack current target entity with used weapon
        /// </summary>
        [Server]
        public void Attack()
        {
            // move to target
            Vector3 direction = (transform.position - targetEntity.transform.position).normalized;
            Vector3 destination = usedWeapon.isRanged ? targetEntity.transform.position - direction + (direction * usedWeapon.attackRange) : targetEntity.transform.position;

            RpcSetDestination(destination.x, destination.z);
            agent.SetDestination(destination);

            // reset cooldown
            findDestinationCooldown = destinationCooldownWithTarget;
            attackCooldown = usedWeapon.attackCooldown;

            // attack
            AttackSystem.Instance.ServerFireAtTarget(ID, targetEntity.ID, usedWeapon.ID, rotateToTarget:false);
        }

        #endregion

        #region //======            IENTITYMOVEMENT          ======\\

        /// <summary>
        /// Set NavMesh agent target
        /// </summary>
        /// <param name="x">position X</param>
        /// <param name="z">position Z</param>
        [ClientRpc]
        public void RpcSetDestination(float x, float z)
        {
            agent.SetDestination(new Vector3(x, 1, z));
        }

        #endregion

        /// <summary>
        /// Coroutine called in OnDamage event
        /// </summary>
        /// <returns></returns>
        private IEnumerator OnDamageCoroutine()
        {
            meshRenderer.sharedMaterial = onDamageMaterial;
            yield return new WaitForSeconds(0.1f);
            meshRenderer.sharedMaterial = startMaterial;
        }
    }
}
