/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

/// <summary>
/// Rotate object towards camera
/// </summary>
namespace MULTIPLAYER_GAME.Utils
{
    public class RotateTowardsCamera : MonoBehaviour
    {
        #region //======            VARIABLES           ======\\

        private Camera m_Camera;

        #endregion

        #region //======            MONOBEHAVIOURS           ======\\

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
