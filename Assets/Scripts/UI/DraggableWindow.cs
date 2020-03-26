/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using UnityEngine.EventSystems;

namespace MULTIPLAYER_GAME.UI
{
    public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] private WindowCloseEvent closeEvent;
        [Header("On Begin Drag")]
        [SerializeField] private float onBeginDragAlpha = 0.6f;
        [Header("On End Drag")]
        [SerializeField] private float onEndDragAlpha = 1;

        private Vector2 offset;
        private Vector4 rectPositions;
        private CanvasGroup canvasGroup;

        #region //======            MONOBEHAVIOURS           ======\\

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Rect canvasGroupRect = canvasGroup.GetComponent<RectTransform>().rect;

            float width = Screen.width - (canvasGroupRect.width * transform.localScale.x) / 2;
            float height = Screen.height - (canvasGroupRect.height * transform.localScale.y) / 2;

            rectPositions = new Vector4(
                (canvasGroupRect.width * transform.localScale.x) / 2,       // position x
                (canvasGroupRect.height * transform.localScale.y) / 2,      // position y
                width,                                                      // width
                height                                                      // height
            );
        }

        #endregion

        #region //======    EVENT SYSTEMS    ======\\

        public void OnBeginDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = onBeginDragAlpha;

            offset = new Vector2(eventData.position.x - transform.position.x, eventData.position.y - transform.position.y);
        }

        public void OnDrag(PointerEventData eventData)
        {
            float x = eventData.position.x - offset.x;
            float y = eventData.position.y - offset.y;

            if (x > rectPositions.z)
                x = rectPositions.z;
            else if (x < rectPositions.x)
                x = rectPositions.x;

            if (y > rectPositions.w)
                y = rectPositions.w;
            else if (y < rectPositions.y)
                y = rectPositions.y;

            transform.position = new Vector2(x, y);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = onEndDragAlpha;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (closeEvent)
            {
                case (WindowCloseEvent.NONE):
                    break;
                case (WindowCloseEvent.LEFT_CLICK):
                    if (eventData.button == PointerEventData.InputButton.Left)
                        gameObject.SetActive(false);
                    break;
                case (WindowCloseEvent.MIDDLE_CLICK):
                    if (eventData.button == PointerEventData.InputButton.Middle)
                        gameObject.SetActive(false);
                    break;
                case (WindowCloseEvent.RIGHT_CLICK):
                    if (eventData.button == PointerEventData.InputButton.Right)
                        gameObject.SetActive(false);
                    break;
            }
        }

        #endregion
    }

    public enum WindowCloseEvent
    {
        NONE,
        LEFT_CLICK,
        MIDDLE_CLICK,
        RIGHT_CLICK
    }
}
