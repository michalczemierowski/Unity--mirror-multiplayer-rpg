/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using UnityEngine.UI;

namespace MULTIPLAYER_GAME.UI
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class OpenWindowButton : MonoBehaviour
    {

        #region //======            VARIABLES           ======\\

        [SerializeField] private GameObject targetWindow;
        [Space(10)]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.white;

        private Image image;
        private Button button;

        #endregion



        #region //======            MONOBEHAVIOURS           ======\\

        private void Start()
        {
            image = GetComponent<Image>();

            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            CallOnActiveChange call = (CallOnActiveChange)targetWindow.AddComponent(typeof(CallOnActiveChange));
            call.openButton = this;
        }

        #endregion

        #region //======            PUBLIC METHODS           ======\\

        public void OnActiveChange()
        {
            if (targetWindow.activeSelf)
            {
                image.color = inactiveColor;
            }
            else
            {
                image.color = activeColor;
            }
        }

        public void OnClick()
        {
            targetWindow.SetActive(!targetWindow.activeSelf);
        }

        #endregion
    }
}
