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

        public event EventHandler<Animation.AnimationState> OnAnimationStateChangeEvent;
        public event EventHandler<float> OnDamageEvent;
        public event EventHandler<float> OnHealEvent;

        [HideInInspector] public Animator m_Animator;

        #region Damage

        public virtual void SetHealth(float Health)
        {
            // events
            if (Health < this.Health)
                OnDamageEvent?.Invoke(this, this.Health - Health);
            else if (Health > this.Health)
                OnHealEvent?.Invoke(this, Health - this.Health);

            this.Health = Health;
        }

        // SERVER ONLY
        public virtual void Damage(int attackerID, float value)
        {
            Health -= value;
            AddExperience(attackerID, 10);
            MessageSystem.AddMessage($"DAMAGE ENTITY  ID = [{ID}]  DAMAGE = [{value}]  HEALTH = [{Health}]");
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

        public virtual void SetState(Animation.AnimationState state)
        {
            OnAnimationStateChangeEvent.Invoke(this, state);
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
