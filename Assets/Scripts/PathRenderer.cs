﻿/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    public static PathRenderer Instance;                                    // singleton

    #region //======            VARIABLES           ======\\

    LineRenderer lineRenderer;                                              // line renderer reference

    #endregion

    #region //======            MONOBEHAVIOURS           ======\\

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(this);

        lineRenderer = GetComponent<LineRenderer>();
    }

    #endregion

    #region //======            PATH METHODS           ======\\

    public static void DrawPath(Vector3[] path)
    {
        Instance.lineRenderer.positionCount = path.Length;
        for (int i = 0; i < path.Length; i++)
        {
            Instance.lineRenderer.SetPosition(i, path[i] + new Vector3(0, 0.3f, 0));
        }
    }


    public static void ResetPath()
    {
        Instance.lineRenderer.positionCount = 0;
    }

    #endregion
}
