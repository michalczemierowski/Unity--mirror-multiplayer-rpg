/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

namespace MULTIPLAYER_GAME.Interfaces
{
    public interface IEntityAnimation
    {
        void SetTrigger(string name);
        void PlayAnimation(string name);
        void SetState(Animation.AnimationState state);
    }
}
