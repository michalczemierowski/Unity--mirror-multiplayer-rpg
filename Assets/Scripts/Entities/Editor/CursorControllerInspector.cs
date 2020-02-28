/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CursorController))]
public class CursorControllerInspector : Editor
{
    CursorController cursorController;

    private void OnEnable()
    {
        cursorController = (CursorController)target;
    }

    public override void OnInspectorGUI()
    {
        cursorController.cursorCount = EditorGUILayout.IntSlider("Cursor count: ", cursorController.cursorCount, 1, 32);

        if (cursorController.cursorCount != cursorController.cursorTag.Count) FixCount();

        for (int i = 0; i < cursorController.cursorCount; i++)
        {
            GUILayout.Space(15);
            cursorController.cursorTag[i] = EditorGUILayout.TextField("Tag: ", cursorController.cursorTag[i]);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Texture: ");
            cursorController.cursorTexture[i] = (Texture2D)EditorGUILayout.ObjectField(cursorController.cursorTexture[i], typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();
        }
    }

    private void FixCount()
    {
        while (cursorController.cursorTag.Count < cursorController.cursorCount)
        {
            cursorController.cursorTexture.Add(null);
            cursorController.cursorTag.Add(null);
        }

        while (cursorController.cursorTag.Count > cursorController.cursorCount)
        {
            int index = cursorController.cursorTag.Count - 1;
            cursorController.cursorTexture.RemoveAt(index);
            cursorController.cursorTag.RemoveAt(index);
        }
    }
}
