/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using System.Collections.Generic;
using UnityEngine;

namespace MULTIPLAYER_GAME.UI
{
    public class CursorController : MonoBehaviour
    {
        #region  Public variables

        #endregion

        #region Serializable variables

        public int cursorCount;
        public List<Texture2D> cursorTexture;
        public List<string> cursorTag;

        #endregion

        #region  Private variables

        private Camera m_Camera;

        #endregion

        #region  Unity methods

        private void Start()
        {
            m_Camera = Camera.main;
        }

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
    }
}
