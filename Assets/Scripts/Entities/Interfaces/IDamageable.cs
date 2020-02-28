/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

namespace MULTIPLAYER_GAME.Interfaces
{
    public interface IDamageable
    {
        void Damage(int attackerID, float value);
    }
}