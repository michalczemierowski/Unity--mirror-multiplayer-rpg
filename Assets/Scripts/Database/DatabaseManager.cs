/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Entities;
using UnityEngine;

namespace MULTIPLAYER_GAME.Database
{
    public class DatabaseManager : MonoBehaviour
    {
        //#if SERVER
        private void Awake()
        {
            Database.Init();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("SAVE DATA");
            foreach (var player in FindObjectsOfType<Player>())
            {
                Database.SaveCharacter(CharacterData.FromPlayerEntity(player));
            }
            Database.CloseConnection();
        }

        //#endif
    }
}
