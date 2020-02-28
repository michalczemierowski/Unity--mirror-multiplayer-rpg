/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Systems;
using UnityEngine;

public class SERVERNetworkManager : NetworkManager
{
    public class CreateCharacterMessage : MessageBase
    {
        public int ID;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CreateCharacterMessage>(OnCreateCharacter);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        CreateCharacterMessage characterMessage = new CreateCharacterMessage
        {
            ID = 0
        };

        conn.Send(characterMessage);
    }

    private void OnCreateCharacter(NetworkConnection conn, CreateCharacterMessage createCharacterMessage)
    {
        Player player = Instantiate(playerPrefab).GetComponent<Player>();
        //player.ID = createCharacterMessage.ID;

        ObjectDatabase.AddEntity(player);
        Debug.Log("ADD ENTITY " + player.ID + " " + createCharacterMessage.ID);

        NetworkServer.AddPlayerForConnection(conn, player.gameObject);
    }
}
