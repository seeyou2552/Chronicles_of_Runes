using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float rotateSpeed = 200f;
    private Transform target;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Update()
    {
        if (target == null) return;
        if (!GetComponent<SkillAction>().homing) return;

        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float rotateAmount = Vector3.Cross(direction, transform.right).z;

        if (GetComponent<Rigidbody2D>() == null) return;
        GetComponent<Rigidbody2D>().angularVelocity = -rotateAmount * rotateSpeed;
        GetComponent<Rigidbody2D>().velocity = transform.right * speed;
    }

    void OnDisable()
    {
        target = null;
    }
}