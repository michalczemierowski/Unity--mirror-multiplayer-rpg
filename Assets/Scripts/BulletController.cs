/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Entities;
using UnityEngine;

namespace MULTIPLAYER_GAME.Client
{
    public class BulletController : MonoBehaviour
    {
        [HideInInspector] public WeaponController weaponController;
        [HideInInspector] public int attackerID;

        public Transform target;
        public float damage = 0.25f;
        public float speed = 10;

        private void FixedUpdate()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            if (Vector3.Distance(transform.position, target.position) > 0.05)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
            }
            else
            {
                if (weaponController)
                {
                    Entity entity = target.GetComponent<Entity>();
                    entity.Damage(attackerID, damage);
                    weaponController.RpcSetEntityHealth(entity.ID, entity.Health);
                }

                Destroy(gameObject);
            }
        }
    }
}
