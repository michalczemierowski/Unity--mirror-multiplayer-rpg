/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Inventory.Items;
using MULTIPLAYER_GAME.Systems;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item), true)]
public class ItemInspector : Editor
{
    Item item;

    private bool isInDatabase;
    private ObjectDatabase objectDatabase;

    private void OnEnable()
    {
        item = (Item)target;
        objectDatabase = FindObjectOfType<ObjectDatabase>();
        if(objectDatabase)
            isInDatabase = objectDatabase._allItems.Contains(item);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (objectDatabase)
        {
            GUILayout.Space(30);

            GUILayout.BeginHorizontal();

            if (isInDatabase)
            {
                GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Label("Object is in database", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Label("Object is not in database", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Add to ObjectDatabase", GUILayout.Height(30)))
            {
                if (!objectDatabase._allItems.Contains(item))
                {
                    objectDatabase._allItems.Add(item);
                    isInDatabase = true;
                }
            }
            if (GUILayout.Button("Remove from ObjectDatabase", GUILayout.Height(30)))
            {
                if (objectDatabase._allItems.Contains(item))
                {
                    objectDatabase._allItems.Remove(item);
                    isInDatabase = false;
                }
            }
        }
        else
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label("There is no active Object Database in current scene", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);

            GUILayout.EndHorizontal();
        }
    }
}