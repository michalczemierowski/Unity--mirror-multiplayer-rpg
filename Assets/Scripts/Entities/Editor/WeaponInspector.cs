/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Inventory;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Weapon))]
public class WeaponInspector : Editor
{
    Weapon weapon;

    private void OnEnable()
    {
        weapon = (Weapon)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if(weapon.isRanged)
        {
            EditorGUILayout.LabelField("Ranged weapon data", EditorStyles.boldLabel);
            weapon.bulletPrefab = (GameObject)EditorGUILayout.ObjectField(weapon.bulletPrefab, typeof(GameObject), false);
            weapon.bulletSpeed = EditorGUILayout.FloatField("Bullet speed: ", weapon.bulletSpeed);
        }
        else
        {
            EditorGUILayout.LabelField("Melee weapon data", EditorStyles.boldLabel);
        }
    }
}
