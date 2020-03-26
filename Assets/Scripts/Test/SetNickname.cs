/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

public class SetNickname : MonoBehaviour
{
    public void SetName(string name)
    {
        PlayerPrefs.SetString("nickname", name);
    }
}
