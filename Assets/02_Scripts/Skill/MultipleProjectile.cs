using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.TextCore.Text;
public class MultipleProjectile : MonoBehaviour
{
    public float damage = 2f; 
    public float speed = 10f;
    public float rotateSpeed = 200f;
    private Transform target;

    void Start()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
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

        SetTarget(closest);
        Return();
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Update()
    {
        if (target == null) return;

        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float rotateAmount = Vector3.Cross(direction, transform.right).z;

        GetComponent<Rigidbody2D>().angularVelocity = -rotateAmount * rotateSpeed;
        GetComponent<Rigidbody2D>().velocity = transform.right * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.GetComponent<IDamageable>().OnDamage((damage + PlayerController.Instance.AttackPower())*PlayerController.Instance.DamageOutcome());
            ObjectPoolManager.Instance.Return(this.gameObject, GetComponent<MultipleProjectile>().name);
        }
    }

    public async void Return()
    {
        await UniTask.Delay(5000);
        ObjectPoolManager.Instance.Return(this.gameObject, GetComponent<MultipleProjectile>().name);
    }
}
