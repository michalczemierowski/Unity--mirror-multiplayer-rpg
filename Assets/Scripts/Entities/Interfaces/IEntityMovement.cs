/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;

namespace MULTIPLAYER_GAME.Interfaces
{
    public interface IEntityMovement
    {
        void RpcSetDestination(float x, float z);
    }
}

