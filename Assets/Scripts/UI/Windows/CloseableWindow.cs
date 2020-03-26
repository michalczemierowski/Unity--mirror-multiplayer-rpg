/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace MULTIPLAYER_GAME.UI
{
    public class CloseableWindow : MonoBehaviour
    {
        /// <summary>
        /// Switch GameObject's activeSelf bool
        /// activeSelf = !activeSelf
        /// </summary>
        public void SwitchActive()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
