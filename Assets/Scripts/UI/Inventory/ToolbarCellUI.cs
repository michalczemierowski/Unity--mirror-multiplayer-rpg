/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MULTIPLAYER_GAME.Inventory.UI
{
    public class ToolbarCellUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public int index;                                           // toolbar index
        public Image icon;                                          // icon image
        public Text count;                                          // count text

        public InventoryCellUI Cell;                                // assigned inventory cell

        private CanvasGroup canvasGroup;                            // canvas group used to change alpha value
        private Vector3 startPosition;                              // start position used to reset position after dragging

        private void Start()
        {
            // get canvas group component
            canvasGroup = GetComponent<CanvasGroup>();
        }

        #region //======            EVENT SYSTEMS           ======\\

        public void OnDrag(PointerEventData eventData)
        {
            if (Cell == null) return;

            transform.position = eventData.position;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Cell == null) return;

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
            // return if there is no assigned cell
            if (Cell == null) return;

            // get object below mouse
            GameObject currentRaycast = eventData.pointerCurrentRaycast.gameObject;
            if (currentRaycast)
            {
                if (currentRaycast.tag == "ToolbarSlot")
                {
                    // get toolbar cell below mouse
                    ToolbarCellUI toolbarCell = currentRaycast.GetComponent<ToolbarCellUI>() ?? currentRaycast.GetComponentInChildren<ToolbarCellUI>() ?? currentRaycast.GetComponentInParent<ToolbarCellUI>();
                    int index = toolbarCell.index;

                    // if can set assigned cell's item to toolbar
                    if (InventorySystem.CanSetToolbarItem(index, Cell, out toolbarCell))
                    {
                        if (toolbarCell.Cell != null)
                        {
                            // swap cells
                            InventoryCellUI cell = Cell;

                            SetCell(toolbarCell.Cell);
                            toolbarCell.SetCell(cell);
                        }
                        else
                        {
                            // move cell
                            toolbarCell.SetCell(Cell);
                            ClearCell();
                        }
                    }
                }
            }
            else
            {
                // reset cell
                Cell.toolbarIndex = -1;
                ClearCell();
            }

            // reset position etc.
            transform.position = startPosition;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // return if cell or cell's item is null
            if (!Cell || !Cell.item) return;

            switch (eventData.button)
            {
                case (PointerEventData.InputButton.Left):
                    // use item
                    Cell.item.UseItem(new Vector2Byte(Cell.indexPosition));
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

        #region //======            INVENTORY CELL METHODS           ======\\

        public void UpdateCount(int newCount)
        {
            count.text = newCount.ToString();
        }

        public void UpdateData()
        {
            if (Cell == null) return;

            icon.sprite = Cell.item.icon;
            count.text = Cell.count.ToString();
        }

        public void SetCell(InventoryCellUI cell)
        {
            if (cell == null) return;
            if (cell.item == null)
            {
                ClearCell();
                return;
            }

            cell.toolbarIndex = index;
            this.icon.sprite = cell.item.icon;
            this.count.text = cell.count.ToString();
            this.Cell = cell;
        }

        public void ClearCell()
        {
            Cell = null;
            count.text = string.Empty;
            icon.sprite = null;
        }

        #endregion
    }
}
