using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.TextCore.Text;
public class Wave : MonoBehaviour
{
    public float damage;
    public float speed;
    public string objName;
    public Vector3 targetPos;
    private Transform target;

    void OnEnable()
    {
        StartCoroutine(Return());
    }


    void Update()
    {
        transform.position += targetPos * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.GetComponent<IDamageable>().OnDamage(damage);
        }
    }

    private IEnumerator Return()
    {
        yield return YieldCache.WaitForSeconds(1f);
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }
}
