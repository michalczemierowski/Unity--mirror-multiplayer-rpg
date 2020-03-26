/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

/*
 * Game settings script
 */
public class GameSettings : MonoBehaviour
{
    #region //======            VARIABLES           ======\\

    public int targetFramerate = 60;                                // target framerate

    #endregion

    #region //======            MONOBEHAVIOURS           ======\\

    private void Awake()
    {
        Application.targetFrameRate = targetFramerate;
    }

    #endregion
}
