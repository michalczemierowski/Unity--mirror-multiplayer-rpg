/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Entities;
using UnityEngine;

namespace MULTIPLAYER_GAME.Inventory.Items
{
    [CreateAssetMenu(fileName = "Weapon_0", menuName = "Weapon")]
    public class Weapon : Equipment
    {
        [Header("Weapon data")]
        public int Damage;
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
