/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Interfaces;
using MULTIPLAYER_GAME.Inventory;
using MULTIPLAYER_GAME.Systems;
using MULTIPLAYER_GAME.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MULTIPLAYER_GAME.Entities
{
    public class Entity : NetworkBehaviour, IDamageable, IEntityAnimation
    {
        [Header("Entity data")]
        [SyncVar]
        public int ID;

        public NavMeshAgent agent;
        public Weapon usedWeapon;

        [SyncVar]
        [Header("Attributes")]
        public float Health = 1;

        public delegate void OnDamageDelegate(int attackerID, float value);
        public delegate void OnHealDelegate(float value);
        [SyncEvent]
        public event OnDamageDelegate EventOnDamage;
        [SyncEvent]
        public event OnHealDelegate EventOnHeal;

        [HideInInspector] public Animator m_Animator;

        public bool isRunning { get; protected set; }

        #region Damage

        // SERVER ONLY
        [Server]
        public virtual void Damage(int attackerID, float value)
        {
            Health -= value;
            AddExperience(attackerID, 10);
            EventOnDamage(attackerID, value);
        }

        public virtual void Heal(float value)
        {
            Health += value;
            EventOnHeal(value);
        }

        private void AddExperience(int attackerID, int experience)
        {
            if (Health <= 0)
            {
                ObjectDatabase.GetPlayerEntity(attackerID)?.AddExperience(experience);
            }
        }

        #endregion

        #region IEntityAnimation

        public virtual void PlayAnimation(string name)
        {
            m_Animator.Play(name);
        }

        public virtual void SetTrigger(string name)
        {
            m_Animator.SetTrigger(name);
        }

        #endregion

        public virtual void Start()
        {
            if (!isServer)
                ObjectDatabase.AddEntity(this);
            MessageSystem.AddMessage($"ADD ENTITY  ID = [{ID}]  POSITION = [{transform.position}]");

            m_Animator = GetComponentInChildren<Animator>();
        }

        public virtual void LateUpdate()
        {
            if (m_Animator != null)
            {
                isRunning = agent.velocity != Vector3.zero;
                m_Animator.SetFloat("Speed", Mathf.Max(Mathf.Abs(agent.velocity.x), Mathf.Abs(agent.velocity.z)));
            }
        }

        public virtual void OnDestroy()
        {
            ObjectDatabase.RemoveEntity(this);
        }

        public IEnumerator BlockMovement(float time)
        {
            agent.isStopped = true;
            yield return new WaitForSeconds(time);
            agent.isStopped = false;
        }
    }
}
