/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace MULTIPLAYER_GAME.Inventory
{
    [CreateAssetMenu(fileName = "Weapon_0", menuName = "Weapon")]
    public class Weapon : Item
    {
        [Header("Weapon data")]
        public float Damage;
        public float attackRange;
        public float attackCooldown;
        public float cantMoveTime;
        public string animationTrigger;

        public bool isRanged;

        // Hidden because they're in custom inspector
        // RANGED WEAPON DATA
        [HideInInspector] public GameObject bulletPrefab;
        [HideInInspector] public float bulletSpeed;

        // MELEE WEAPON DATA
        // TODO
    }
}
