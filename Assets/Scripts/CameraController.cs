/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace MULTIPLAYER_GAME.Client
{
    public class CameraController : MonoBehaviour
    {
        #region //======            VARIABLES           ======\\

        public static bool isLooking;                                               // is player looking around

        [Header("Rotate with target")]
        [SerializeField] private bool rotateWithTarget;                             // rotate camera with player movement
        [SerializeField] private float rotationSpeed = 2;                           // rotation speed
        [Space]
        [SerializeField] private Vector2 verticalRotationLimits;                    // rotation limit (up - down)

        private Transform target;                                                   // follow target

        [SerializeField] private float sensitivityX = 0.3f, sensitivityY = 0.3f;    // look sensitivity

        private Vector3 _mouseReference;
        private Vector3 _mouseOffset;
        private Vector3 _rotation;

        #endregion

        #region //======            MONOBEHAVIOURS           ======\\

        private void Start()
        {
            _rotation = Vector3.zero;
        }

        private void Update()
        {
            if (target)
            {
                transform.position = target.position;

                if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(1))
                {
                    // offset
                    _mouseOffset = (Input.mousePosition - _mouseReference);

                    // apply rotation
                    _rotation.y = _mouseOffset.x * sensitivityX;
                    _rotation.x = -_mouseOffset.y * sensitivityX;

                    float x = Mathf.Clamp(transform.eulerAngles.x + _rotation.x, verticalRotationLimits.x, verticalRotationLimits.y);
                    float y = transform.eulerAngles.y + _rotation.y;

                    // rotate
                    transform.eulerAngles = new Vector3(x, y, 0);

                    // store mouse
                    _mouseReference = Input.mousePosition;

                    isLooking = true;
                }
                else
                {
                    isLooking = false;

                    if (rotateWithTarget)
                    {
                        transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.LerpAngle(transform.eulerAngles.y, target.eulerAngles.y, rotationSpeed * Time.deltaTime), 0);
                    }
                }
            }
            _mouseReference = Input.mousePosition;
        }

        #endregion

        /// <summary>
        /// Set camera follow target
        /// </summary>
        /// <param name="target">Target transform</param>
        public void SetTarget(Transform target)
        {
            this.target = target;
        }
    }
}
