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

        // you can send the message here, or wherever else you want
        CreateCharacterMessage characterMessage = new CreateCharacterMessage
        {
            ID = ObjectDatabase.NextEntityID()
        };

        conn.Send(characterMessage);
    }

    private void OnCreateCharacter(NetworkConnection conn, CreateCharacterMessage createCharacterMessage)
    {
        GameObject gameobject = Instantiate(playerPrefab);
        ObjectDatabase.AddEntity(gameobject.GetComponent<Player>());

        NetworkServer.AddPlayerForConnection(conn, gameobject);
    }
}
