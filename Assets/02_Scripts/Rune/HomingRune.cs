using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "HomingRune", menuName = "Rune/Homing Projectile")]
public class HomingRune : Rune
{
    public float searchRadius = 200f;
    public float speed = 10f;
    public float rotateSpeed = 200f;
    GameObject projectileDevide;

    public override void Apply(Skill skill)
    {
        skill.MiddleSet(() =>
        {
            skill.skillAction.homing = true;
            if (skill.devidePool != null)
            {
                skill.skillActionDevide.homing = true;
            }

            Logger.Log("유도중");
            GameObject projectile = skill.pool;
            var homing = projectile.GetComponent<HomingProjectile>();
            if (homing == null)
            {
                return;
                // homing = projectile.AddComponent<HomingProjectile>();
            }

            GameObject[] GetEnemiesByPhysics()
            {
                // 현재 오브젝트 위치를 중심으로 반경 searchRadius 내의 Collider2D를 레이어 마스크로 검색
                Collider2D[] hits = Physics2D.OverlapCircleAll(projectile.transform.position, searchRadius, LayerMask.GetMask("Enemy"));
                return hits
                    .Select(c => c.gameObject)
                    .Where(go => !go.CompareTag("AttackAble")) //특정 태그 제외
                    .Distinct()  // 중복 제거
                    .ToArray();
            }
            var enemies = GetEnemiesByPhysics();
            // GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
            Logger.Log(enemies.Length.ToString());
            if (enemies.Length == 0) return;
            Transform closest = null;
            float minDist = Mathf.Infinity;
            Vector3 origin = PlayerController.Instance.transform.position;

            foreach (var enemy in enemies)
            {
                float dist = Vector3.Distance(origin, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy.transform;
                }
            }

            if (closest == null) return;


            homing.speed = speed;
            homing.rotateSpeed = rotateSpeed;
            homing.SetTarget(closest);

            // Devide 용
            if (skill.devidePool != null)
            {
                skill.skillActionDevide.homing = true;
                projectileDevide = skill.devidePool;

                var homingDevide = projectileDevide.GetComponent<HomingProjectile>();
                if (homingDevide == null)
                {
                    homingDevide = projectileDevide.AddComponent<HomingProjectile>();
                }

                homingDevide.speed = speed;
                homingDevide.rotateSpeed = rotateSpeed;
                homingDevide.SetTarget(closest);
            }
        });
    }
}