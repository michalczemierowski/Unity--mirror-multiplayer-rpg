/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Inventory.Items;
using MULTIPLAYER_GAME.Systems;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Weapon))]
public class WeaponInspector : Editor
{
    Weapon weapon;

    private bool isInDatabase;
    private ObjectDatabase objectDatabase;

    private void OnEnable()
    {
        weapon = (Weapon)target;
        objectDatabase = FindObjectOfType<ObjectDatabase>();
        if (objectDatabase)
            isInDatabase = objectDatabase._allItems.Contains(weapon);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (objectDatabase)
        {
            GUILayout.Space(10);
            if (weapon.isRanged)
            {
                EditorGUILayout.LabelField("Ranged weapon data", EditorStyles.boldLabel);
                weapon.bulletPrefab = (GameObject)EditorGUILayout.ObjectField(weapon.bulletPrefab, typeof(GameObject), false);
                weapon.bulletSpeed = EditorGUILayout.FloatField("Bullet speed: ", weapon.bulletSpeed);
            }
            else
            {
                EditorGUILayout.LabelField("Melee weapon data", EditorStyles.boldLabel);
            }

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
                ObjectDatabase objectDatabase = FindObjectOfType<ObjectDatabase>();

                if (!objectDatabase._allItems.Contains(weapon))
                {
                    objectDatabase._allItems.Add(weapon);
                    objectDatabase._allWeapons.Add(weapon);
                    isInDatabase = true;
                }
            }
            if (GUILayout.Button("Remove from ObjectDatabase", GUILayout.Height(30)))
            {
                ObjectDatabase objectDatabase = FindObjectOfType<ObjectDatabase>();

                if (objectDatabase._allItems.Contains(weapon))
                {
                    objectDatabase._allItems.Remove(weapon);
                    objectDatabase._allWeapons.Remove(weapon);
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
