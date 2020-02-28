/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using MULTIPLAYER_GAME.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace MULTIPLAYER_GAME.Client
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PositionSynchronization : NetworkBehaviour
    {
        private NavMeshAgent agent;
        private Player player;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            if (isLocalPlayer)
                player = GetComponent<Player>();
        }

        [Command]
        public void CmdSetDestination(Vector3 destination)
        {
            agent.SetDestination(destination);
            RpcSetDestination(destination);
        }

        [ClientRpc]
        void RpcSetDestination(Vector3 destination)
        {
            agent.SetDestination(destination);
            //player.SetPath(destination);
            if (isLocalPlayer)
            {
                player.SetIndicator(agent.destination);
            }
        }
    }
}
