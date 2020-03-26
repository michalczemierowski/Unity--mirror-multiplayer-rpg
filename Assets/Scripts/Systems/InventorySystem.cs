/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Inventory.Items;
using MULTIPLAYER_GAME.Inventory.UI;
using MULTIPLAYER_GAME.Systems;
using UnityEngine;

namespace MULTIPLAYER_GAME.Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance;                            // singleton

        #region //======            VARIABLES           ======\\

        public Item[,] Inventory;                                          // local copy of inventory data

        public float dropItemPositionX { get; private set; }               // value used in OnEndDrag in InventoryUICell

        public Vector2Int inventorySize;                                   // inventory size in cells
        public int maxCellCapacity = 64;                                   // max capacity of single cell

        [Space]

        [Header("Inventory UI settings")]
        [SerializeField] private int extraSpaceOnTop = 60;                  // extra space on top (for example for header)
        [SerializeField] private Vector2Int padding;
        [SerializeField] private Vector2Int spacing;
        [SerializeField] private Vector2Int cellSize;                       // cell size x - width, y - heigth

        [Space]

        [Header("Inventory panel")]
        [SerializeField] private GameObject inventoryCellPrefab;            // inventory cell prefab
        [SerializeField] private RectTransform inventoryCellPanel;          // inventory cell panel (parent for instantiating cells)

        [Space]

        [Header("Toolbar")]
        [SerializeField] private ToolbarCellUI[] toolbarCells;              // all toolbar cells

        private InventoryCellUI[,] inventoryUICells;                        // array containing reference to all inventory cells

        private const string TOOLBAR_PLAYERPREFS_KEY = "TOOLBAR_DATA";      // playerprefs key for toolbar data

        #endregion



        #region //======            MONOBEHAVIOURS           ======\\

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);

            Inventory = new Item[inventorySize.x, inventorySize.y];
            inventoryUICells = new InventoryCellUI[inventorySize.x, inventorySize.y];
            for (int x = 0; x < inventorySize.x; x++)
            {
                for (int y = 0; y < inventorySize.y; y++)
                {
                    Vector3 position = new Vector3(
                        -x * cellSize.x - padding.x,
                        y * cellSize.y + padding.y, 0
                    );

                    position += new Vector3(
                        -(x - 1) * spacing.x - cellSize.x / 2,
                        (y - 1) * spacing.y + cellSize.y / 2, 0
                    );

                    RectTransform rectTransform = Instantiate(inventoryCellPrefab, inventoryCellPanel).GetComponent<RectTransform>();

                    rectTransform.localScale = new Vector3(cellSize.x / 100f, cellSize.y / 100f, 1);
                    rectTransform.anchoredPosition = position;

                    inventoryUICells[x, y] = rectTransform.GetComponent<InventoryCellUI>();
                    inventoryUICells[x, y].indexPosition = new Vector2Int(x, y);
                }
            }

            inventoryCellPanel.sizeDelta = new Vector2(
                inventorySize.x * (cellSize.x + spacing.x) - 2 * spacing.x + 2 * padding.x,
                inventorySize.y * (cellSize.y + spacing.y) - 2 * spacing.y + 2 * padding.y + extraSpaceOnTop
            );

            dropItemPositionX = inventoryCellPanel.position.x - inventoryCellPanel.sizeDelta.x;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
                inventoryCellPanel.gameObject.SetActive(!inventoryCellPanel.gameObject.activeSelf);
        }

        #endregion

        #region //======            INVENTORY METHODS           ======\\

        /// <summary>
        /// Called on inventoru update
        /// </summary>
        /// <param name="pos">Index of new item</param>
        /// <param name="newItem">New item data</param>
        public void OnInventoryUpdate(Vector2Byte pos, ItemData newItem)
        {
            Debug.Log(pos.x + "  " + pos.y + "  " + newItem.ID);
            Item item = ObjectDatabase.GetItem(newItem.ID);
            if (item)
            {
                inventoryUICells[pos.x, pos.y].InsertItem(item, newItem.Count);
                Inventory[pos.x, pos.y] = item;
            }
            else
            {
                inventoryUICells[pos.x, pos.y].RemoveItem();
                Inventory[pos.x, pos.y] = null;
            }
        }

        /// <summary>
        /// Clear inventory
        /// </summary>
        public void Reset()
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                for (int y = 0; y < inventorySize.y; y++)
                {
                    inventoryUICells[x, y].RemoveItem();
                    Inventory[x, y] = null;
                }
            }
        }

        /// <summary>
        /// Remove item at index
        /// </summary>
        /// <param name="index">Vector2Byte item index</param>
        /// <returns></returns>
        public static bool DropItem(Vector2Byte index)
        {
            Instance.inventoryUICells[index.x, index.y].RemoveItem();
            Instance.Inventory[index.x, index.y] = null;
            return true;
        }

        /// <summary>
        /// Get inventory cell by X and Y position
        /// </summary>
        /// <param name="x">position X</param>
        /// <param name="y">position Y</param>
        /// <returns>InventoryCellUI</returns>
        public InventoryCellUI GetInventoryCell(int x, int y)
        {
            return inventoryUICells[x, y];
        }

        /// <summary>
        /// Find first avalible slot in inventory
        /// </summary>
        /// <param name="inventory">copy of inventory data</param>
        /// <returns>Vector2Int there is avalible slot, else null</returns>
        public Vector2Int? FindFirstPosition(SyncDictionaryInventoryData inventory)
        {
            Vector2Int? result = null;

            for (int y = inventorySize.y - 1; y >= 0; y--)
            {
                for (int x = inventorySize.x - 1; x >= 0; x--)
                {
                    if (!inventory.ContainsKey(new Vector2Byte(x, y)))
                        return new Vector2Int(x, y);
                }
            }

            return result;
        }

        #endregion

        #region //======            TOOLBAR METHODS           ======\\

        /// <summary>
        /// Check if player can set toolbar target to selected cell
        /// </summary>
        /// <param name="index">Toolbar cell index</param>
        /// <param name="cell">Inventory cell</param>
        /// <param name="toolbarCell">if true returns selected cell</param>
        /// <returns></returns>
        public static bool CanSetToolbarItem(int index, InventoryCellUI cell, out ToolbarCellUI toolbarCell)
        {
            toolbarCell = null;
            if (index < 0 || index >= Instance.toolbarCells.Length || cell.item == null) return false;

            toolbarCell = Instance.toolbarCells[index];

            return true;
        }

        /// <summary>
        /// Remove cell reference from toolbar
        /// </summary>
        /// <param name="index">Toolbar index</param>
        public static void ClearToolbarItem(int index)
        {
            if (index < 0 || index >= Instance.toolbarCells.Length) return;

            Instance.toolbarCells[index].ClearCell();
        }

        /// <summary>
        /// Get toolbar cell by index
        /// </summary>
        /// <param name="index">Toolbar index</param>
        /// <returns></returns>
        public static ToolbarCellUI GetToolbarCell(int index)
        {
            if (index < 0 || index >= Instance.toolbarCells.Length) return null;

            return Instance.toolbarCells[index];
        }

        /// <summary>
        /// Load toolbar data from PlayerPrefs
        /// </summary>
        public static void SaveToolbar()
        {
            string toolbarData = string.Empty;

            foreach (var toolbarCell in Instance.toolbarCells)
            {
                if (toolbarCell.Cell && toolbarCell.Cell.item)
                {
                    // save toolbar data in format: TOOLBAR_CELL_INDEX   ;   INVENTORY_CELL_INDEX_X   ;   INVENTORY_CELL_INDEX_Y   |
                    toolbarData += toolbarCell.index + ";" + toolbarCell.Cell.indexPosition.x + ";" + toolbarCell.Cell.indexPosition.y + "|";
                }
            }
            PlayerPrefs.SetString(TOOLBAR_PLAYERPREFS_KEY, toolbarData);
        }

        /// <summary>
        /// Save toolbar data to PlayerPrefs as string
        /// </summary>
        public static void LoadToolbar()
        {
            string toolbarData = PlayerPrefs.GetString(TOOLBAR_PLAYERPREFS_KEY);

            // split data
            string[] toolbarCells = toolbarData.Split('|');
            for(int i = 0; i < toolbarCells.Length - 1; i++)
            {
                // parse strings to ints
                string[] cellData = toolbarCells[i].Split(';');
                int index = int.Parse(cellData[0]);
                int indexPositionX = int.Parse(cellData[1]);
                int indexPositionY = int.Parse(cellData[2]);

                InventoryCellUI inventoryCell = Instance.GetInventoryCell(indexPositionX, indexPositionY);
                ToolbarCellUI toolbarCell;

                // if can set current item to toolbar
                if (CanSetToolbarItem(index, inventoryCell, out toolbarCell))
                {
                    // assing cell toolbarIndex
                    inventoryCell.toolbarIndex = index;
                    // set toolbarCell to inventoryCell
                    toolbarCell.SetCell(inventoryCell);
                }
            }
        }

        #endregion
    }
}
