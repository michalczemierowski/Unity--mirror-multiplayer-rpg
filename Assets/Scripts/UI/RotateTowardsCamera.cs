/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace MULTIPLAYER_GAME.Utils
{
    public class RotateTowardsCamera : MonoBehaviour
    {
        #region  Private variables

        private Camera m_Camera;

        #endregion

        #region  Unity methods

        private void Start()
        {
            m_Camera = Camera.main;
        }

        private void Update()
        {
            transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward, m_Camera.transform.rotation * Vector3.up);
        }

        #endregion
    }
}
