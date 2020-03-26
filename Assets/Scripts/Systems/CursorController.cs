/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MULTIPLAYER_GAME.UI
{
    public class CursorController : MonoBehaviour
    {
        #region //======            VARIABLES           ======\\

        public int cursorCount;
        public List<Texture2D> cursorTexture;
        public List<string> cursorTag;

        private Camera m_Camera;                                    // main camera reference

        #endregion

        #region //======            MONOBEHAVIOURS           ======\\

        private void Start()
        {
            m_Camera = Camera.main;
        }


        /// <summary>
        /// Change cursor texture based on raycast object tag
        /// </summary>
        private void Update()
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                bool found = false;
                for (int i = 0; i < cursorTag.Count; i++)
                {
                    if (hit.transform.tag == cursorTag[i])
                    {
                        Cursor.SetCursor(cursorTexture[i], Vector2.zero, CursorMode.Auto);
                        found = true;
                        break;
                    }
                }
                if (!found)
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        #endregion

        /// <summary>
        /// Check if cursor is over UI
        /// </summary>
        /// <returns></returns>
        public static bool IsCursorOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
