/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using UnityEngine.UI;

namespace MULTIPLAYER_GAME.UI
{
    public class UIStatsPanel : MonoBehaviour
    {
        public static UIStatsPanel Instance;                            // singleton

        #region //======            VARIABLES           ======\\

        [Header("Health")]
        [SerializeField] private Image healthBar;                       // healthbar image - must have image type set to FILLED
        [SerializeField] private Text healthValue;                      // text in which health value will be displayed
        [Header("Nickname")]
        [SerializeField] private GameObject nicknameInputField;         // input field for setting nickname
        [SerializeField] private Text nicknameText;                     // text in which nickname will be displayed

        #endregion

        #region //======            MONOBEHAVIOURS           ======\\

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);
        }

        #endregion

        #region //======            UI METHODS           ======\\

        public static void SetNameText(string name)
        {
            Instance.nicknameText.text = name;
            Instance.nicknameInputField.SetActive(false);
        }

        public static void SetHealthbarValue(int health, int maxHealth)
        {
            Instance.healthBar.fillAmount = (float)health / maxHealth;
            Instance.healthValue.text = health + "HP";
        }

        #endregion
    }
}
