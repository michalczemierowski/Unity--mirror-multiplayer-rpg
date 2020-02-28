/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Inventory.UI;
using MULTIPLAYER_GAME.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MULTIPLAYER_GAME.Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance;

        #region  Public variables

        public Item[,] Inventory;
        public Item testItem;

        public float dropItemPositionX { get; private set; }

        #endregion

        #region Serializable variables

        [SerializeField] private Vector2Int inventorySize;
        [SerializeField] private int maxCellCapacity = 200;

        [Space]
        [Header("Inventory UI settings")]
        [SerializeField] private Vector2Int padding;
        [SerializeField] private Vector2Int spacing;
        [SerializeField] private Vector2Int cellSize;
        [Space]
        [SerializeField] private GameObject inventoryCellPrefab;
        [SerializeField] private RectTransform inventoryCellPanel;

        #endregion

        #region  Private variables

        private InventoryUICell[,] inventoryUICells;

        #endregion



        #region  Unity methods

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);

            Inventory = new Item[inventorySize.x, inventorySize.y];
            inventoryUICells = new InventoryUICell[inventorySize.x, inventorySize.y];
            for (int x = 0; x < inventorySize.x; x++)
            {
                for (int y = 0; y < inventorySize.y; y++)
                {
                    Vector3 position = new Vector3(-x * cellSize.x - padding.x, y * cellSize.y + padding.y, 0);
                    position += new Vector3(-(x - 1) * spacing.x - cellSize.x / 2, (y - 1) * spacing.y + cellSize.y / 2, 0);

                    RectTransform rectTransform = Instantiate(inventoryCellPrefab, inventoryCellPanel).GetComponent<RectTransform>();

                    rectTransform.localScale = new Vector3(cellSize.x / 100f, cellSize.y / 100f, 1);
                    rectTransform.anchoredPosition = position;

                    inventoryUICells[x, y] = rectTransform.GetComponent<InventoryUICell>();
                    inventoryUICells[x, y].indexPosition = new Vector2Int(x, y);
                }
            }

            inventoryCellPanel.sizeDelta = new Vector2(inventorySize.x * (cellSize.x + spacing.x) - 2 * spacing.x + 2 * padding.x, inventorySize.y * (cellSize.y + spacing.y) - 2 * spacing.y + 2 * padding.y);
            dropItemPositionX = inventoryCellPanel.position.x - inventoryCellPanel.sizeDelta.x;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
                AddItem(testItem, 15);

            if (Input.GetKeyDown(KeyCode.I))
                inventoryCellPanel.gameObject.SetActive(!inventoryCellPanel.gameObject.activeSelf);
        }

        #endregion

        private Vector2Int? FindSamePosition(Item item)
        {
            Vector2Int? result = null;

            for (int x = inventorySize.x - 1; x >= 0; x--)
            {
                for (int y = inventorySize.y - 1; y >= 0; y--)
                {
                    if (Inventory[x, y]?.ID == item.ID && Instance.inventoryUICells[x, y]?.count < maxCellCapacity)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return result;
        }

        private Vector2Int? FindFirstPosition(Item item)
        {
            Vector2Int? result = null;

            for (int y = inventorySize.y - 1; y >= 0; y--)
            {
                for (int x = inventorySize.x - 1; x >= 0; x--)
                {
                    if (result == null && Inventory[x, y] == null)
                        return new Vector2Int(x, y);
                }
            }

            return result;
        }

        //if (result == null && Inventory[x, y] == null)
        //result = new Vector2Int(x, y);

        public static bool AddItem(Item item, int count)
        {
            Vector2Int? position = Instance.FindSamePosition(item);
            if (position != null)
            {
                Vector2Int pos = (Vector2Int)position;
                InventoryUICell cell = Instance.inventoryUICells[pos.x, pos.y];
                if (cell.count + count >= Instance.maxCellCapacity)
                {
                    int ncount = Instance.maxCellCapacity - cell.count;
                    cell.SetItem(item, ncount);
                    Instance.AddNewItem(item, ncount);
                    return true;
                }
                else
                {
                    cell.SetItem(item, count);
                    return true;
                }
            }
            else
            {
                return Instance.AddNewItem(item, count);
            }
        }

        private bool AddNewItem(Item item, int count)
        {
            Vector2Int? position = Instance.FindFirstPosition(item);
            if (position != null)
            {
                Vector2Int pos = (Vector2Int)position;
                Instance.inventoryUICells[pos.x, pos.y].SetItem(item, count);
                Instance.Inventory[pos.x, pos.y] = item;
                return true;
            }
            return false;
        }

        public static bool DropItem(Vector2Int index)
        {
            Instance.inventoryUICells[index.x, index.y].RemoveItem();
            Instance.Inventory[index.x, index.y] = null;
            return true;
        }

        private void SwapItems(Vector2Int positionOne, Vector2Int positionTwo)
        {
            Item itemOne = Inventory[positionOne.x, positionOne.y];

            Inventory[positionOne.x, positionOne.y] = Inventory[positionTwo.x, positionTwo.y];
            Inventory[positionTwo.x, positionTwo.y] = itemOne;
        }

        public static void InventoryCellSwap(Vector2Int positionOne, Vector2Int positionTwo)
        {
            InventoryUICell cellOne = Instance.inventoryUICells[positionOne.x, positionOne.y];
            InventoryUICell cellTwo = Instance.inventoryUICells[positionTwo.x, positionTwo.y];

            cellOne.indexPosition = positionTwo;
            cellTwo.indexPosition = positionOne;

            Instance.inventoryUICells[positionOne.x, positionOne.y] = cellTwo;
            Instance.inventoryUICells[positionTwo.x, positionTwo.y] = cellOne;

            Instance.SwapItems(positionOne, positionTwo);
        }
    }
}
