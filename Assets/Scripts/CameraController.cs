/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace MULTIPLAYER_GAME.Client
{
    public class CameraController : MonoBehaviour
    {
        private Transform target;
        public Vector3 offset;

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        private void Update()
        {
            if (target)
                transform.position = target.position + offset;
        }
    }
}
