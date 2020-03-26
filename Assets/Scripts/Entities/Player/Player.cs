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
using MULTIPLAYER_GAME.UI;
using UnityEngine.UI;
using MULTIPLAYER_GAME.Inventory;
using MULTIPLAYER_GAME.Inventory.UI;
using MULTIPLAYER_GAME.Inventory.Items;

#if UNITY_EDITOR

using UnityEditor;

#endif

/*
 * Player controller class
 * - handle all Cmd's and Rpc's in this class
 */
namespace MULTIPLAYER_GAME.Entities
{
    [RequireComponent(typeof(PositionSynchronization))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Player : Entity
    {
        public static Player localPlayer;                               // singleton

        #region //======            VARIABLES           ======\\

        private Camera m_Camera;                                        // main camera

        [Header("Player data")]
        [SerializeField] private LayerMask moveLayerMask;               // layer mask on which player can click and walk
        [SerializeField] private GameObject indicatorPrefab;            // idicator that shows after setting agent target

        [Header("UI")]
        [SerializeField] private Text nameText;                         // UI text for displaying player nickname

        [Header("Player attributes")]                                   // player attributes
        [SyncVar] public long Experience;
        [SyncVar] public int Armor;
        [SyncVar] public int Strength;
        [SyncVar] public int Intelligence;
        [SyncVar] public int Stamina;

        [Header("Movement")]

        [SerializeField] private float walkingSpeed;                    // movement speed while walking
        [SerializeField] private float runningSpeed;                    // movement speed while running
        [SerializeField] private float sprintingSpeed;                  // movement speed while sprinting

        public float Speed
        {
            get
            {
                return isWalking ? walkingSpeed : isSprinting ? sprintingSpeed : runningSpeed;
            }
        }                                           // player current movement speed

        [Space]

        [Header("Unity Events")]
        [SerializeField] private UnityEvent onStartIfNotLocal;          // event invoked on start if player is not local
        [SerializeField] private UnityEvent onStartIfLocal;             // event invoked on start if player is local

        [Space]

        private Transform indicator;                                    // idicator that shows after setting agent target
        private Animator indicatorAnimator;                             // ^ indicator animator

        private PositionSynchronization positionSynchronization;        // position synchronization script reference
        private WeaponController weaponController;                      // weapon controller script reference

        private Entity target;                                          // current target
        private Vector3 lastTargetPosinion;                             // last position of current target

        private float localCooldown;
        private float movementClickCooldown;                            // cooldown when holding move button

        // network
        public SyncDictionaryInventoryData inventoryData;               // inventory data
        public SyncDictionaryEquipmentData equipmentData;               // equipment data

        [SyncVar] public bool isSprinting;                              // bools for synchronizing animations
        [SyncVar] public bool isWalking;                                // bools for synchronizing animations

        #endregion

        #region //======            MONOBEHAVIOURS           ======\\

        public override void Start()
        {
            base.Start();

            // UI
            nameText.text = Name.ToString();

            if (!isLocalPlayer)
            {
                onStartIfNotLocal.Invoke();
                return;
            }

            localPlayer = this;

            // camera
            m_Camera = Camera.main;
            m_Camera.GetComponentInParent<CameraController>().SetTarget(transform);

            // scripts
            positionSynchronization = GetComponent<PositionSynchronization>();
            weaponController = GetComponent<WeaponController>();

            // indicator
            indicator = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.identity).transform;
            indicatorAnimator = indicator.GetComponent<Animator>();

            onStartIfLocal.Invoke();
            StartCoroutine(CheckTarget());

            // show inventory
            InventorySystem.Instance.Reset();
            foreach (var key in inventoryData.Keys)
            {
                InventorySystem.Instance.OnInventoryUpdate(key, inventoryData[key]);
            }

            foreach (var key in equipmentData.Keys)
            {
                EquipmentSystem.OnEquipmentUpdate(SyncIDictionary<EquipmentSlot, ItemData>.Operation.OP_ADD, key, equipmentData[key]);
            }

            inventoryData.Callback += OnInventoryUpdate;
            equipmentData.Callback += EquipmentSystem.OnEquipmentUpdate;

            UIStatsPanel.SetNameText(Name);
            EquipmentSystem.SetPlayer(this);

            InventorySystem.LoadToolbar();
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

                    MessageSystem.SetEntityInfoData(target);
                }
                else
                    HandleMouseHover();

                // ATTACKING
                if (localCooldown > 0)
                    localCooldown -= Time.deltaTime;

                // ANIMATION
                bool nIsSprinting = isRunning && Input.GetKey(KeyCode.LeftShift);
                bool nIsWalking = isRunning && Input.GetKey(KeyCode.LeftControl);
                if (nIsSprinting != isSprinting || nIsWalking != isWalking)
                    CmdSetInput(nIsSprinting, nIsWalking);

                // MOVEMENT
                if (movementClickCooldown > 0)
                    movementClickCooldown -= Time.deltaTime;

                UIStatsPanel.SetHealthbarValue(Health, maxHealth);
            }

            agent.speed = Speed;
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            m_Animator.SetBool("Running", isRunning);
            m_Animator.SetBool("Sprinting", isSprinting);
        }

        #endregion

        #region //======            EVENT LISTENERS           ======\\

        // EVENT LISTENERS WILL RUN ON NOT LOCAL PLAYERS TOO
        public override void OnHeal(int value)
        {
            base.OnHeal(value);

            // ON HEAL
        }

        public override void OnDamage(uint attackerID, int value)
        {
            base.OnDamage(attackerID, value);

            // ON DAMAGE
        }

        /// <summary>
        /// On inventory update event callback
        /// </summary>
        /// <param name="op">Operation (ADD, SET, REMOVE...)</param>
        /// <param name="key">inventory item position</param>
        /// <param name="item">item data</param>
        private void OnInventoryUpdate(SyncIDictionary<Vector2Byte, ItemData>.Operation op, Vector2Byte key, ItemData item)
        {
            switch (op)
            {
                case SyncIDictionary<Vector2Byte, ItemData>.Operation.OP_ADD:
                case SyncIDictionary<Vector2Byte, ItemData>.Operation.OP_SET:
                    InventorySystem.Instance.OnInventoryUpdate(key, item);
                    break;
                case SyncIDictionary<Vector2Byte, ItemData>.Operation.OP_REMOVE:
                    InventorySystem.DropItem(key);
                    break;
            }
        }

        #endregion

        #region //======            INPUT           ======\\

        /// <summary>
        /// Handle mouse hover
        /// </summary>
        private void HandleMouseHover()
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            Entity entity = null;

            if (Physics.Raycast(ray, out hit) && (hit.transform.tag == "Player" || hit.transform.tag == "Entity"))
            {
                entity = hit.transform.GetComponent<Entity>();
                if (entity == this) entity = null;
            }
            MessageSystem.SetEntityInfoData(entity);
        }

        /// <summary>
        /// Handle player's input
        /// </summary>
        public void HandleInput()
        {
            if (CameraController.isLooking) return;

            if (!CursorController.IsCursorOverUI())
            {
                if (Input.GetButtonDown("Move")) movementClickCooldown = 0;

                if (Input.GetButton("Move") && movementClickCooldown <= 0)
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
                if (Input.GetButtonDown("Attack") && usedWeapon)
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
            }

            if (Input.GetButtonDown("ClosestEnemy"))
            {
                target = GetClosestEntity();
            }

            #region //======    TOOLBAR    ======\\

            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetButtonDown("Toolbar" + i))
                {
                    ToolbarCellUI toolbarCell = InventorySystem.GetToolbarCell(i - 1);
                    if (toolbarCell & toolbarCell.Cell)
                    {
                        toolbarCell.Cell.item.UseItem(new Vector2Byte(toolbarCell.Cell.indexPosition));
                    }
                }
            }

            #endregion

            if (Input.GetKeyDown(KeyCode.E))
            {
                ItemData data = new ItemData(0, 15);
                CmdAddItem(data);
            }
        }

        /// <summary>
        /// Attack entity
        /// </summary>
        /// <param name="entity">Target entity</param>
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

        /// <summary>
        /// Set target entity
        /// </summary>
        /// <param name="target">Target entity</param>
        public void SetTarget(Entity target)
        {
            if (target == this)
                this.target = null;
            else
            {
                this.target = target;

                positionSynchronization.CmdResetPath();
            }
        }

        /// <summary>
        /// Check if target entity is not null
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Move player to target entity
        /// </summary>
        /// <param name="target">Target entity</param>
        public void SetDestinationToEntity(Entity target)
        {
            Vector3 direction = (transform.position - target.transform.position).normalized;
            Vector3 destination = usedWeapon.isRanged ? target.transform.position - direction + (direction * usedWeapon.attackRange) : target.transform.position;

            positionSynchronization.CmdSetDestination(destination);

            lastTargetPosinion = target.transform.position;
        }

        /// <summary>
        /// Get entity that is closest to the player and is in the player's attack range
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Place movement indicator at position
        /// </summary>
        /// <param name="position">Indicator position</param>
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

        #endregion

        #region //======            ATTRIBUTES           ======\\

        /// <summary>
        /// [Server] Add experience to player
        /// </summary>
        /// <param name="experience">experience</param>
        [Server]
        public void AddExperience(int experience)
        {
            Experience += experience;
        }

        #endregion

        #region //======            CMD | RPC           ======\\

        //======            MESSAGES            ======\\

        /// <summary>
        /// [Command] Send command to add message for all players
        /// </summary>
        /// <param name="text">Message content</param>
        [Command]
        public void CmdAddMessage(string text)
        {
            RpcAddMessage(text);
        }

        /// <summary>
        /// [ClientRpc] Add message for all players
        /// </summary>
        /// <param name="text">Message content</param>
        [ClientRpc]
        public void RpcAddMessage(string text)
        {
            MessageSystem.InstantiateMessage(text);
        }

        //======            INPUT               ======\\

        /// <summary>
        /// [Command] Send command to update animator bools for network players
        /// </summary>
        /// <param name="isSprinting">Is player sprinting</param>
        /// <param name="isWalking">Is player walking</param>
        [Command]
        private void CmdSetInput(bool isSprinting, bool isWalking)
        {
            this.isSprinting = isSprinting;
            this.isWalking = isWalking;
        }

        //======            INVENTORY           ======\\

        /// <summary>
        /// [Command] Send message to remove item
        /// </summary>
        /// <param name="position">Item position</param>
        [Command]
        public void CmdDropItem(Vector2Byte position)
        {
            DropItem(position);
        }

        /// <summary>
        /// [Server] Remove item
        /// </summary>
        /// <param name="position">Item position</param>
        [Server]
        public void DropItem(Vector2Byte position)
        {
            if (inventoryData.ContainsKey(position))
            {
                inventoryData.Remove(position);
            }
        }

        /// <summary>
        /// Send command to swap two items positions
        /// </summary>
        /// <param name="positionOne">First item position</param>
        /// <param name="positionTwo">Second item position</param>
        [Command]
        public void CmdSwapItems(Vector2Byte positionOne, Vector2Byte positionTwo)
        {
            bool one = inventoryData.ContainsKey(positionOne);
            bool two = inventoryData.ContainsKey(positionTwo);
            if (one && !two)
            {
                inventoryData.Add(positionTwo, inventoryData[positionOne]);
                inventoryData.Remove(positionOne);
            }
            else if (two && !one)
            {
                inventoryData.Add(positionOne, inventoryData[positionTwo]);
                inventoryData.Remove(positionTwo);
            }
            else if (one && two)
            {
                ItemData itemOne = inventoryData[positionOne];
                ItemData itemTwo = inventoryData[positionTwo];

                inventoryData.Remove(positionOne);
                inventoryData.Remove(positionTwo);

                inventoryData.Add(positionOne, itemTwo);
                inventoryData.Add(positionTwo, itemOne);
            }
        }

        /// <summary>
        /// [Command] Send command to item to inventory
        /// </summary>
        /// <param name="item">Item data</param>
        [Command]
        public void CmdAddItem(ItemData item)
        {
            AddItem(item);
        }

        /// <summary>
        /// [Server] Add item to inventory
        /// </summary>
        /// <param name="item">Item data</param>
        [Server]
        public bool AddItem(ItemData item)
        {
            int max = InventorySystem.Instance.maxCellCapacity;
            short toAdd = item.Count;

            if (item.ID < 10000)
            {
                for (int x = 0; x < InventorySystem.Instance.inventorySize.x; x++)
                {
                    for (int y = 0; y < InventorySystem.Instance.inventorySize.y; y++)
                    {
                        Vector2Byte key = new Vector2Byte(x, y);
                        if (inventoryData.ContainsKey(key))
                        {
                            if (toAdd == 0)
                                break;
                            ItemData nItem = inventoryData[key];
                            if (nItem.ID == item.ID && nItem.Count < max)
                            {
                                short res = (short)(max - nItem.Count);
                                if (res <= toAdd)
                                {
                                    inventoryData[key] = new ItemData(nItem.ID, (short)(nItem.Count + res));
                                    toAdd -= res;
                                }
                                else
                                {
                                    inventoryData[key] = new ItemData(nItem.ID, (short)(nItem.Count + toAdd));
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            if (toAdd > 0)
            {
                Vector2Int? pos = InventorySystem.Instance.FindFirstPosition(inventoryData);
                if (pos != null)
                {
                    Vector2Int position = (Vector2Int)pos;
                    Vector2Byte itemPosition = new Vector2Byte(position.x, position.y);

                    if (!inventoryData.ContainsKey(itemPosition))
                    {
                        ItemData itemData = new ItemData()
                        {
                            ID = item.ID,
                            Count = toAdd,
                        };
                        inventoryData.Add(itemPosition, itemData);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// [Command] Send command to remove item from inventory
        /// </summary>
        /// <param name="position">Item position</param>
        /// <param name="count">How much to remove</param>
        [Command]
        public void CmdRemoveItem(Vector2Byte position, short count)
        {
            RemoveItem(position, count);
        }

        /// <summary>
        /// [Server] Remove item from inventory
        /// </summary>
        /// <param name="position">Item position</param>
        /// <param name="count">How much to remove</param>
        [Server]
        public void RemoveItem(Vector2Byte position, short count)
        {
            if (inventoryData.ContainsKey(position))
            {
                ItemData item = inventoryData[position];
                short nCount = (short)(item.Count - count);
                if (nCount <= 0)
                    DropItem(position);
                else
                    inventoryData[position] = new ItemData(item.ID, nCount);
            }
        }

        //======            EQUIPMENT           ======\\

        /// <summary>
        /// [Command] Equip item
        /// </summary>
        /// <param name="ID">Equipment slot</param>
        /// <param name="itemData">Inventory item data</param>
        /// <param name="position">Inventory item position</param>
        [Command]
        public void CmdSetEquipment(EquipmentSlot ID, ItemData itemData, Vector2Byte position)
        {
            if (ID < 0 || (int)ID > 4) return;

            Item item = ObjectDatabase.GetItem(itemData.ID);
            if (item && item is Equipment)
            {
                Equipment equipment = (Equipment)item;
                Equipment oldEquipment = null;

                // add new equipment
                if (equipmentData.ContainsKey(ID))
                {
                    oldEquipment = (Equipment)ObjectDatabase.GetItem(equipmentData[ID].ID);
                    equipmentData[ID] = itemData;
                }
                else
                    equipmentData.Add(ID, itemData);

                if (item.GetType() == typeof(Weapon))
                {
                    usedWeapon = (Weapon)item;
                    usedWeaponID = item.ID;
                }

                Armor += equipment.ArmorBonus;
                maxHealth += equipment.HealthBonus;
                Strength += equipment.StrengthBonus;
                Intelligence += equipment.IntelligenceBonus;
                Stamina += equipment.StaminaBonus;

                inventoryData.Remove(position);

                // remove old equipment
                if (oldEquipment)
                {
                    Armor -= oldEquipment.ArmorBonus;
                    maxHealth -= oldEquipment.HealthBonus;
                    if (Health > maxHealth)
                        Health = maxHealth;
                    Strength -= oldEquipment.StrengthBonus;
                    Intelligence -= oldEquipment.IntelligenceBonus;
                    Stamina -= oldEquipment.StaminaBonus;

                    ItemData newItemData = new ItemData(oldEquipment.ID, 1);
                    if (!inventoryData.ContainsKey(position))
                        inventoryData.Add(position, newItemData);
                    else
                        AddItem(newItemData);
                }
            }
        }

        /// <summary>
        /// [Command] Take off equipment if there is space left in inventory
        /// </summary>
        /// <param name="ID">Equipment slot to take item from</param>
        [Command]
        public void CmdTakeOffEquipment(EquipmentSlot ID)
        {
            if (ID < 0 || (int)ID > 4) return;

            if (equipmentData.ContainsKey(ID))
            {
                Equipment oldEquipment = (Equipment)ObjectDatabase.GetItem(equipmentData[ID].ID);
                if (oldEquipment && AddItem(new ItemData(oldEquipment.ID, 1)))
                {
                    // set used weapon to null if player is taking off weapon
                    if (oldEquipment.GetType() == typeof(Weapon))
                    {
                        usedWeapon = null;
                        usedWeaponID = -1;
                    }

                    Armor -= oldEquipment.ArmorBonus;
                    maxHealth -= oldEquipment.HealthBonus;
                    if (Health > maxHealth)
                        Health = maxHealth;
                    Strength -= oldEquipment.StrengthBonus;
                    Intelligence -= oldEquipment.IntelligenceBonus;
                    Stamina -= oldEquipment.StaminaBonus;

                    equipmentData.Remove(ID);
                }
            }
        }

        //======            ITEM USAGE          ======\\

        /// <summary>
        /// [Command] Use item at inventory position
        /// </summary>
        /// <param name="position">Inventory position</param>
        [Command]
        public void CmdUseItem(Vector2Byte position)
        {
            if (inventoryData.ContainsKey(position))
            {
                Item item = ObjectDatabase.GetItem(inventoryData[position].ID);
                if (item is UsableItem)
                {
                    UsableItem usableItem = (UsableItem)item;

                    Armor += usableItem.ArmorBonus;
                    maxHealth += usableItem.MaxHealthBonus;
                    Health += usableItem.HealthBonus;
                    Strength += usableItem.StrengthBonus;
                    Intelligence += usableItem.IntelligenceBonus;
                    Stamina += usableItem.StaminaBonus;
                    // TODO: TIME BUFFS
                }

                RemoveItem(position, 1);
            }
        }

        #endregion

        #region //======            PATH DRAWING           ======\\

        /// <summary>
        /// Set NavMesh agent destination
        /// </summary>
        /// <param name="destination">Target destination</param>
        public void SetPath(Vector3 destination)
        {
            agent.SetDestination(destination);
            //NavMeshPath NavMeshPath = new NavMeshPath();
            //agent.CalculatePath(destination, NavMeshPath);

            //StartCoroutine(DrawPath());
            //StartCoroutine("WaitForPathFinish");
        }

        /// <summary>
        /// Wait for path to finish
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForPathFinish()
        {
            yield return null;
            yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
            PathRenderer.ResetPath();
        }

        /// <summary>
        /// Draw current agent path
        /// </summary>
        /// <returns></returns>
        private IEnumerator DrawPath()
        {
            while (true)
            {
                if (agent.hasPath)
                {
                    PathRenderer.DrawPath(agent.path.corners);
                }
                yield return new WaitForSeconds(0.25f);
            }
        }

        #endregion

        #region //======            DATABASE           ======\\

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Save character data to database
            if (isServer)
                Database.Database.SaveCharacter(Database.CharacterData.FromPlayerEntity(this));
        }

        private void OnDisable()
        {
            if (hasAuthority)
                InventorySystem.SaveToolbar();
        }

        #endregion
    }
}
