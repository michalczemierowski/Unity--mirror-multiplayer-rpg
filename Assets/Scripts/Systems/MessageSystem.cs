/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MULTIPLAYER_GAME.UI
{
    public class MessageSystem : NetworkBehaviour
    {
        public static MessageSystem Instance;                               // singleton

        #region //======            VARIABLES           ======\\

        [SerializeField] private int maxMessages;                           // max messages displayed at once
        [SerializeField] private GameObject messagePrefab;                  // message prefab
        [SerializeField] private Transform messageParent;                   // parent for messages

        [Header("Entity info panel")]
        [SerializeField] private GameObject entityInfoPanel;                // entity info panel parent
        [SerializeField] private Text entityName;                           // text in which entity name will be displayed
        [SerializeField] private Text entityHealth;                         // text in which entity health will be displayed
        [SerializeField] private Image entityHealthFillImage;               // target entity health image - image type must be setted to FILLED

        Player player;                                                      // local player reference

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

        private void Start()
        {
            StartCoroutine(WaitForLocalPlayer());
        }

        #endregion

        #region //======            MESSAGE & ENTITY INFO METHODS           ======\\

        /// <summary>
        /// Instantiate message locally
        /// </summary>
        /// <param name="text">message content</param>
        public static void InstantiateMessage(string text)
        {
            GameObject message = Instantiate(Instance.messagePrefab, Instance.messageParent);
            Text messageText = message.GetComponent<Text>();

            int toRemove = Instance.maxMessages - Instance.messageParent.childCount;
            for (int i = 0; i < -toRemove; i++)
            {
                Destroy(Instance.messageParent.GetChild(i).gameObject);
            }

            messageText.text = text;
            Destroy(message, 3f);
        }

        /// <summary>
        /// Instantiate message for all players
        /// </summary>
        /// <param name="text">content</param>
        public static void AddMessagePlayer(string text)
        {
            if (Instance.player)
                Instance.player.CmdAddMessage(text);
        }

        /// <summary>
        /// Set entity info data
        /// </summary>
        /// <param name="entity">target entity</param>
        public static void SetEntityInfoData(Entity entity)
        {
            if (entity)
            {
                Instance.entityInfoPanel.SetActive(true);
                Instance.entityHealth.text = entity.Health.ToString();
                Instance.entityName.text = entity.Name;
                Instance.entityHealthFillImage.fillAmount = (float)entity.Health / entity.maxHealth;
            }
            else
            {
                Instance.entityInfoPanel.SetActive(false);
            }
        }

        #endregion

        #region //======            RPC           ======\\

        [ClientRpc]
        public void RpcAddMessageServer(string text)
        {
            InstantiateMessage(text);
        }

        #endregion

        #region //======            COROUTINES           ======\\

        /// <summary>
        /// Wait until local player is not null
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForLocalPlayer()
        {
            yield return null;
            yield return new WaitUntil(() => Player.localPlayer != null);
            player = Player.localPlayer;
        }

        #endregion
    }
}
