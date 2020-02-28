/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MULTIPLAYER_GAME.Inventory.UI
{
    public class InventoryUICell : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private Text itemName;
        [SerializeField] private Text itemCount;

        [HideInInspector] public Vector2Int indexPosition;

        public Item item;
        public int count;

        private CanvasGroup canvasGroup;
        private Vector3 startPosition;

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetItem(Item item, int count)
        {
            if(item == null)
            {
                itemIcon.sprite = null;
                itemName.text = string.Empty;
                itemCount.text = string.Empty;
            }
            else if (this.item != null && this.item.ID == item.ID)
                AddCount(count);
            else
                InsertItem(item, count);
        }

        private void InsertItem(Item item, int count)
        {
            this.item = item;
            this.count = count;

            itemIcon.sprite = item.icon;
            itemName.text = item.name;
            itemCount.text = count.ToString();
        }

        private void AddCount(int count)
        {
            this.count += count;
            itemCount.text = this.count.ToString();
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.5f;

            startPosition = transform.position;
            transform.SetAsLastSibling();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GameObject currentRaycast = eventData.pointerCurrentRaycast.gameObject;
            if (currentRaycast && currentRaycast.tag == "InventorySlot")
            {
                InventorySystem.InventoryCellSwap(indexPosition, currentRaycast.gameObject.GetComponent<InventoryUICell>().indexPosition);

                transform.position = currentRaycast.transform.position;
                currentRaycast.transform.position = startPosition;
            }
            else
                transform.position = startPosition;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case (PointerEventData.InputButton.Left):
                    Debug.Log("ON CLICK Left");
                    break;
                case (PointerEventData.InputButton.Middle):
                    Debug.Log("ON CLICK Middle");
                    break;
                case (PointerEventData.InputButton.Right):
                    Debug.Log("ON CLICK Right");
                    break;
            }
        }
    }
}
