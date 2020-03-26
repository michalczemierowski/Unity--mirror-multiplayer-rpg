/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DraggableWindow))]
public class DraggableWindowInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10);
        GUILayout.Label("ANCHOR MUST BE SET TO CENTER", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Label("PIVOT MUST BE SET TO (0.5, 0.5)", EditorStyles.centeredGreyMiniLabel);
    }
}
