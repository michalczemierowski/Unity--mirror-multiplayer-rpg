/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Interfaces;
using MULTIPLAYER_GAME.Inventory.Items;
using MULTIPLAYER_GAME.Systems;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 * Base class for all entities
 */

namespace MULTIPLAYER_GAME.Entities
{
    [SelectionBase]
    public class Entity : NetworkBehaviour, IDamageable, IEntityAnimation
    {
        [Header("Entity data")]
        [SyncVar] public uint ID = 0;                                           // unique entity ID
        [SyncVar] public string Name;                                           // entity name

        [Header("NavMesh")]
        public NavMeshAgent agent;                                              // entity NavMesh agent

        [HideInInspector]
        [SyncVar(hook = nameof(OnChangeWeapon))]                                // hook will be called on variable update
        public short usedWeaponID;
        [Header("Weapon")]
        public Weapon usedWeapon;                                               // weapon used by entity

        [Header("Attributes")]
        [SyncVar] private int health = 100;                                     // entity current health
        public int Health                                                       // entity current health
        {
            get
            {
                return health;
            }
            set
            {
                value = value > maxHealth ? maxHealth : value;
                if (health < value)
                {
                    EventOnHeal(value - health);
                }

                health = value;
            }
        }
        [HideInInspector] [SyncVar] public int maxHealth;                       // entity max health

        public delegate void OnDamageDelegate(uint attackerID, int value);       // event called when entity gets damaged
        public delegate void OnHealDelegate(int value);                         // event called when entity receives heal
        [SyncEvent] public event OnDamageDelegate EventOnDamage;
        [SyncEvent] public event OnHealDelegate EventOnHeal;

        [HideInInspector] public Animator m_Animator;                           // entity animator

        public bool isRunning { get; protected set; }                           // bools for synchronizing animations

        #region //======            ATTRIBUTE METHODS           ======\\

        /// <summary>
        /// Damage entity
        /// </summary>
        /// <param name="attackerID">ID of attacker</param>
        /// <param name="damage">damage</param>
        [Server]
        public virtual void Damage(uint attackerID, int damage)
        {
            Health -= damage;
            EventOnDamage(attackerID, damage);
        }

        #endregion

        #region //======            ANIMATION           ======\\

        /// <summary>
        /// Play animation if entity has Animator
        /// </summary>
        /// <param name="name">Animator state name</param>
        public virtual void PlayAnimation(string name)
        {
            if (!m_Animator) return;

            m_Animator.Play(name);
        }

        /// <summary>
        /// Set Animator trigger if entity has Animator
        /// </summary>
        /// <param name="name">Trigger variable name</param>
        public virtual void SetTrigger(string name)
        {
            if (!m_Animator) return;

            m_Animator.SetTrigger(name);
        }

        #endregion

        #region //======            HOOKS           ======\\

        /// <summary>
        /// Called on local client when the weapon changes
        /// </summary>
        public virtual void OnChangeWeapon(short oldWeapon, short newWeapon)
        {
            usedWeapon = ObjectDatabase.GetWeapon(newWeapon);
        }

        #endregion

        #region //======            EVENT LISTENERS           ======\\

        public virtual void OnHeal(int value)
        {
            Debug.Log("ON HEAL +" + value);
        }

        public virtual void OnDamage(uint attackerID, int value)
        {
            Debug.Log("ON DAMAGE -" + value);
        }

        #endregion

        #region //======            MONOBEHAVIOURS           ======\\

        private void Awake()
        {
            maxHealth = Health;
            EventOnDamage += OnDamage;
            EventOnHeal += OnHeal;
        }

        public virtual void Start()
        {
            if (!isServer)
                ObjectDatabase.AddEntity(this);

            m_Animator = GetComponentInChildren<Animator>();
        }

        public virtual void LateUpdate()
        {
            if (m_Animator != null)
            {
                // check if entity is moving
                isRunning = agent.velocity != Vector3.zero;

                // set speed variable based on movement speed
                m_Animator.SetFloat("Speed", Mathf.Max(Mathf.Abs(agent.velocity.x), Mathf.Abs(agent.velocity.z)));
            }
        }

        public virtual void OnDestroy()
        {
            // remove entity from database
            ObjectDatabase.RemoveEntity(this);
        }

        public virtual void OnEnable()
        {
            // wait for sync var to load
            StartCoroutine(WaitForID());
        }

        #endregion

        /// <summary>
        /// Block movement possibility 
        /// </summary>
        /// <param name="time">block duration</param>
        public void BlockMovement(float time)
        {
            StartCoroutine(IEBlockMovement(time));
        }

        #region //======            COROUTINES           ======\\

        private IEnumerator WaitForID()
        {
            yield return null;
            ObjectDatabase.Instance.GetEntityPosition(ID);
        }

        private IEnumerator IEBlockMovement(float time)
        {
            agent.isStopped = true;
            yield return new WaitForSeconds(time);
            agent.isStopped = false;
        }

        #endregion
    }
}
