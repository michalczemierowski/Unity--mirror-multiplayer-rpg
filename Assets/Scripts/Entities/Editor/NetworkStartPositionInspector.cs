/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Mirror;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkStartPosition))]
public class NetworkStartPositionInspector : Editor
{
    NetworkStartPosition networkStartPosition;
    private Vector3 lastPosition;

    private void OnEnable()
    {
        networkStartPosition = (NetworkStartPosition)target;
        lastPosition = networkStartPosition.transform.position;
    }

    public override void OnInspectorGUI()
    {
        Vector3 position = networkStartPosition.transform.position;
        if (lastPosition != position)
        {
            networkStartPosition.gameObject.name = $"Player Spawn Position [ x:{position.x}, z:{position.z} ]";
            lastPosition = position;
        }
    }
}
