/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MULTIPLAYER_GAME.Inventory.UI
{
    public class InventoryCellUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image itemIcon;                // item data
        [SerializeField] private Text itemName;                 // item data       
        [SerializeField] private Text itemCount;                // item data

        [HideInInspector] public Vector2Int indexPosition;      // cell position in grid

        public Item item;
        public int count;

        private CanvasGroup canvasGroup;
        private Vector3 startPosition;

        public int toolbarIndex = -1;                           // assigned toolbar index (-1 if there is no toolbar assigned)

        #region //======            MONOBEHAVIOURS           ======\\

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        #endregion

        #region //======            ITEM METHODS           ======\\

        /// <summary>
        /// Update item (icon, name, count)
        /// </summary>
        public void UpdateData()
        {
            itemIcon.sprite = item.icon;
            itemName.text = item.name;
            itemCount.text = count.ToString();
        }

        /// <summary>
        /// Clear cell data (icon, name, count)
        /// </summary>
        public void ClearData()
        {
            itemIcon.sprite = null;
            itemName.text = string.Empty;
            itemCount.text = string.Empty;

            InventorySystem.GetToolbarCell(toolbarIndex)?.ClearCell();
        }

        /// <summary>
        /// Add item to cell.
        /// If new item is same as current one - add count.
        /// If new item is different - replace current item with new.
        /// If new item is null - clear data.
        /// </summary>
        /// <param name="item">Item object</param>
        /// <param name="count">Item count</param>
        public void AddItem(Item item, int count)
        {
            if (item == null)
                ClearData();
            else if (this.item != null && this.item.ID == item.ID)
                AddCount(count);
            else
                InsertItem(item, count);
        }

        /// <summary>
        /// Remove item from cell and clear data
        /// </summary>
        public void RemoveItem()
        {
            ClearData();

            item = null;
            count = 0;
        }

        /// <summary>
        /// Insert item into cell.
        /// If cell already has item - replace it.
        /// </summary>
        /// <param name="item">Item object</param>
        /// <param name="count">Item count</param>
        public void InsertItem(Item item, int count)
        {
            this.item = item;
            this.count = count;

            UpdateData();
            InventorySystem.GetToolbarCell(toolbarIndex)?.SetCell(this);
        }

        /// <summary>
        /// Add count to current item
        /// </summary>
        /// <param name="count">Item count</param>
        private void AddCount(int count)
        {
            this.count += count;
            itemCount.text = this.count.ToString();
        }

        #endregion

        #region //======            EVENT SYSTEMS           ======\\

        public void OnDrag(PointerEventData eventData)
        {
            if (item == null) return;

            transform.position = eventData.position;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (item == null) return;

            // set canvas group values
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.5f;

            // save start position
            startPosition = transform.position;
            // move object to top in hierarchy
            transform.SetAsLastSibling();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (item == null) return;

            // used to know if player wants to drop item, if true player won't drop item
            bool setToolbar = false;
            // get object below mouse
            GameObject currentRaycast = eventData.pointerCurrentRaycast.gameObject;
            if (currentRaycast)
            {
                if (currentRaycast.tag == "InventorySlot")
                {
                    // get inventory cell below mouse
                    InventoryCellUI target = currentRaycast.gameObject.GetComponent<InventoryCellUI>();
                    // swap current cell with target cell ^
                    Player.localPlayer.CmdSwapItems(new Vector2Byte(indexPosition), new Vector2Byte(target.indexPosition));

                    ToolbarCellUI currentCell = InventorySystem.GetToolbarCell(toolbarIndex);
                    ToolbarCellUI targetCell = InventorySystem.GetToolbarCell(target.toolbarIndex);

                    // swap toolbar indexes
                    int nToolbarIndex = toolbarIndex;
                    toolbarIndex = target.toolbarIndex;
                    target.toolbarIndex = nToolbarIndex;

                    // swap toolbar cells
                    currentCell?.SetCell(target);
                    targetCell?.SetCell(this);
                }
                else if (currentRaycast.tag == "ToolbarSlot")
                {
                    // get toolbar cell below mouse
                    ToolbarCellUI toolbarCell = currentRaycast.GetComponent<ToolbarCellUI>() ?? currentRaycast.GetComponentInChildren<ToolbarCellUI>() ?? currentRaycast.GetComponentInParent<ToolbarCellUI>();
                    int index = toolbarCell.index;

                    // if can set current item to toolbar
                    if (InventorySystem.CanSetToolbarItem(index, this, out toolbarCell))
                    {
                        // clear toolbar
                        InventorySystem.ClearToolbarItem(toolbarIndex);
                        // don't drop item
                        setToolbar = true;
                        // assing cell toolbarIndex
                        toolbarIndex = index;
                        // set toolbarCell to this
                        toolbarCell.SetCell(this);
                    }
                }
            }

            // drop item if mouse position X is lower than inventory panel position X (left side) and toolbarIndex == false
            if (!setToolbar && eventData.position.x < InventorySystem.Instance.dropItemPositionX)
            {
                Player.localPlayer.CmdDropItem(new Vector2Byte(indexPosition));
            }

            // reset position etc.
            transform.position = startPosition;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // return if there is no item in cell
            if (!item) return;

            switch (eventData.button)
            {
                case (PointerEventData.InputButton.Left):
                    // use item
                    item.UseItem(new Vector2Byte(indexPosition));
                    break;
                case (PointerEventData.InputButton.Middle):
                    Debug.Log("ON CLICK Middle");
                    break;
                case (PointerEventData.InputButton.Right):
                    Debug.Log("ON CLICK Right");
                    break;
            }
        }

        #endregion
    }
}
