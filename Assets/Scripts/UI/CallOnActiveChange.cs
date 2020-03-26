/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace MULTIPLAYER_GAME.UI
{
    public class CallOnActiveChange : MonoBehaviour
    {
        [System.NonSerialized] public OpenWindowButton openButton;

        #region //======            MONOBEHAVIOURS           ======\\

        private void OnEnable()
        {
            openButton.OnActiveChange();
        }

        private void OnDisable()
        {
            openButton.OnActiveChange();
        }

        #endregion
    }
}
