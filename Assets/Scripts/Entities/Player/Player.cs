/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

using MULTIPLAYER_GAME.Client;
using MULTIPLAYER_GAME.Systems;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MULTIPLAYER_GAME.Entities
{
    [RequireComponent(typeof(PositionSynchronization))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Player : Entity
    {
        private Camera m_Camera;
        [Header("Player data")]
        [SerializeField] private LayerMask moveLayerMask;
        [SerializeField] private GameObject indicatorPrefab;

        [SyncVar]
        [Header("Player attributes")]
        public int Experience;

        [Header("Movement")]

        // MOVEMENT SPEED
        [SerializeField] private float walkingSpeed;
        [SerializeField] private float runningSpeed;
        [SerializeField] private float sprintingSpeed;

        public float Speed
        {
            get
            {
                return isWalking ? walkingSpeed : isSprinting ? sprintingSpeed : runningSpeed;
            }
        }

        [Space]
        [Header("Unity Events")]
        [SerializeField] private UnityEvent onStartIfNotLocal;
        [SerializeField] private UnityEvent onStartIfLocal;
        [Space]

        private Transform indicator;
        private Animator indicatorAnimator;

        private PositionSynchronization positionSynchronization;
        private WeaponController weaponController;

        private Entity target;
        private Vector3 lastTargetPosinion;

        private float localCooldown;
        private float movementClickCooldown;

        [SyncVar]
        public bool isSprinting;
        [SyncVar]
        public bool isWalking;

        #region Unity methods

        public override void Start()
        {
            base.Start();

            // entity events
            EventOnDamage += OnDamage;
            EventOnHeal += OnHeal;

            if (!isLocalPlayer)
            {
                onStartIfNotLocal.Invoke();
                return;
            }

            // camera
            m_Camera = Camera.main;
            m_Camera.GetComponent<CameraController>().SetTarget(transform);

            // scripts
            positionSynchronization = GetComponent<PositionSynchronization>();
            weaponController = GetComponent<WeaponController>();

            // indicator
            indicator = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.identity).transform;
            indicatorAnimator = indicator.GetComponent<Animator>();

            onStartIfLocal.Invoke();
            StartCoroutine(CheckTarget());
            CmdGetPositions();
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (usedWeapon == null) return;

            Color c = Color.red;
            Handles.color = c;
            Handles.DrawWireDisc(transform.position, transform.up, usedWeapon.attackRange);
            c.a = 0.02f;
            Handles.color = c;
            Handles.DrawSolidDisc(transform.position, transform.up, usedWeapon.attackRange);
        }

#endif

        private void Update()
        {
            if (isLocalPlayer)
            {
                HandleInput();
                if (target != null)
                {
                    if (localCooldown <= 0 && Vector3.Distance(target.transform.position, transform.position) <= usedWeapon.attackRange)
                        Attack(target);
                }

                if (localCooldown > 0)
                    localCooldown -= Time.deltaTime;

                bool nIsSprinting = isRunning && Input.GetKey(KeyCode.LeftShift);
                bool nIsWalking = isRunning && Input.GetKey(KeyCode.LeftControl);
                if(nIsSprinting != isSprinting || nIsWalking != isWalking)
                    CmdSetInput(nIsSprinting, nIsWalking);

                if (movementClickCooldown > 0)
                    movementClickCooldown -= Time.deltaTime;
            }

            agent.speed = Speed;
        }

        [Command]
        private void CmdSetInput(bool isSprinting, bool isWalking)
        {
            this.isSprinting = isSprinting;
            this.isWalking = isWalking;
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            m_Animator.SetBool("Running", isRunning);
            m_Animator.SetBool("Sprinting", isSprinting);
        }

        #endregion

        #region Event listeners

        // EVENT LISTENERS WILL RUN ON NOT LOCAL PLAYERS TOO
        private void OnHeal(float value)
        {
            Debug.Log("ON HEAL", this);
        }

        private void OnDamage(int attackerID, float value)
        {
            Debug.Log("ON DAMAGE", this);
        }

        #endregion

        public void HandleInput()
        {
            if (Input.GetMouseButtonDown(1)) movementClickCooldown = 0;

            if (Input.GetMouseButton(1) && movementClickCooldown <= 0)
            {
                RaycastHit hit;
                Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, moveLayerMask))
                {
                    positionSynchronization.CmdSetDestination(hit.point);
                }
                target = null;

                movementClickCooldown = 0.25f;
            }
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit) && (hit.transform.tag == "Player" || hit.transform.tag == "Entity"))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    Entity entity = hit.transform.GetComponent<Entity>();

                    if (distance < usedWeapon.attackRange)
                    {
                        SetTarget(entity);
                    }
                    else
                    {
                        SetTarget(entity);
                        SetDestinationToEntity(entity);
                    }
                }

            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                target = GetClosestEntity();
            }

            // test
            if (Input.GetKeyDown(KeyCode.F))
            {
                CmdSpawnEntity(0);
            }

            // test
            if (Input.GetKeyDown(KeyCode.G))
            {
                CmdGetPositions();
            }
        }

        private void Attack(Entity entity)
        {
            if (localCooldown <= 0)
            {
                if (usedWeapon.isRanged)
                    weaponController.CmdFireAtTarget(entity.ID);
                else
                    weaponController.CmdMeleeAttack(entity.ID);

                localCooldown = usedWeapon.attackCooldown;
            }
        }

        public void SetTarget(Entity target)
        {
            this.target = target;
        }

        [Command]
        private void CmdSpawnEntity(int entityID)
        {
            //RpcSpawnEntity(entityID);
            Vector3 spawnPosition = ObjectDatabase.GetRandomSpawnpoint().GetPosition();
            GameObject entity = Instantiate(ObjectDatabase.GetEntityPrefabByID(entityID).gameObject, spawnPosition, Quaternion.identity);
            ObjectDatabase.AddEntity(entity.GetComponent<Entity>());

            NetworkServer.Spawn(entity);
        }

        [Command]
        private void CmdGetPositions()
        {
            int[] IDs;
            Vector3[] positions;

            ObjectDatabase.GetPosition(out IDs, out positions);
            RpcSetPositions(IDs, positions);
        }

        [ClientRpc]
        private void RpcSetPositions(int[] IDs, Vector3[] positions)
        {
            ObjectDatabase.SetPosition(IDs, positions);
        }

        private IEnumerator CheckTarget()
        {
            while (true)
            {
                if (target != null)
                {
                    if (lastTargetPosinion != target.transform.position & Vector3.Distance(target.transform.position, transform.position) > usedWeapon.attackRange)
                    {
                        SetDestinationToEntity(target);
                    }
                }
                yield return new WaitForSeconds(0.25f);
            }
        }

        public void SetDestinationToEntity(Entity target)
        {
            Vector3 direction = (transform.position - target.transform.position).normalized;
            Vector3 destination = usedWeapon.isRanged ? target.transform.position - direction + (direction * usedWeapon.attackRange) : target.transform.position;

            positionSynchronization.CmdSetDestination(destination);

            lastTargetPosinion = target.transform.position;
        }

        public Entity GetClosestEntity()
        {
            Entity[] entities = ObjectDatabase.GetAllEntities();
            Entity result = null;

            float minDistance = float.MaxValue;

            foreach (Entity entity in entities)
            {
                if (entity == null || entity.transform == transform) continue;

                float distance = Vector3.Distance(transform.position, entity.transform.position);
                if (distance <= usedWeapon.attackRange && distance < minDistance)
                {
                    result = entity;
                    minDistance = distance;
                }
            }

            return result;
        }

        public void SetIndicator(Vector3 position)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Ray(m_Camera.transform.position, (position - m_Camera.transform.position).normalized), out hit, Mathf.Infinity, moveLayerMask))
            {
                indicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }

            indicator.transform.position = position;
            indicatorAnimator.Play("indicator");
        }

        #region Experience

        public void AddExperience(int experience)
        {
            Experience += experience;
        }

        #endregion

        //public void SetPath(Vector3 destination)
        //{
        //    NavMeshPath navMeshPath = new NavMeshPath();
        //    agent.CalculatePath(destination, navMeshPath);

        //    StartCoroutine(DrawPath());
        //    StartCoroutine("WaitForPathFinish");
        //}

        //private IEnumerator WaitForPathFinish()
        //{
        //    yield return null;
        //    yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
        //    PathRenderer.ResetPath();
        //}

        //private IEnumerator DrawPath()
        //{
        //    while (true)
        //    {
        //        if (agent.hasPath)
        //        {
        //            PathRenderer.DrawPath(agent.path.corners);
        //        }
        //        yield return new WaitForSeconds(0.25f);
        //    }
        //}
    }
}
