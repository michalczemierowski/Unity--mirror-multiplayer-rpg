/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Entities;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntitySpawnPosition))]
public class EntitySpawnPositionInspector : Editor
{
    EntitySpawnPosition entitySpawnPosition;
    private Vector3 lastPosition;

    private void OnEnable()
    {
        entitySpawnPosition = (EntitySpawnPosition)target;
        lastPosition = entitySpawnPosition.transform.position;
    }

    public override void OnInspectorGUI()
    {
        entitySpawnPosition.spawnRange = EditorGUILayout.FloatField("Spawn range: ", entitySpawnPosition.spawnRange);

        Vector3 position = entitySpawnPosition.transform.position;
        float halfRange = entitySpawnPosition.spawnRange / 2;
        Vector2 minPos = new Vector2(position.x - halfRange, position.z - halfRange);
        Vector2 maxPos = new Vector2(position.x + halfRange, position.z + halfRange);

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Min position:");
        GUILayout.Label($"x:{minPos.x}");
        GUILayout.Label($"z:{minPos.y}");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Max position:");
        GUILayout.Label($"x:{maxPos.x}");
        GUILayout.Label($"z:{maxPos.y}");
        GUILayout.EndHorizontal();
        if (lastPosition != entitySpawnPosition.transform.position)
        {
            entitySpawnPosition.gameObject.name = $"Entity Spawn Position [ x:{position.x}, z:{position.z} ]";
            lastPosition = entitySpawnPosition.transform.position;
        }
    }
}
