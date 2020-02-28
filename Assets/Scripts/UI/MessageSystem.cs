/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MULTIPLAYER_GAME.UI
{
    public class MessageSystem : MonoBehaviour
    {
        public static MessageSystem Instance;

        #region  Public variables

        #endregion

        #region Serializable variables

        [SerializeField] private int maxMessages;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private Transform messageParent;

        #endregion

        #region  Private variables

        #endregion

        #region  Unity methods

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

        public static void AddMessage(string text)
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
    }
}
