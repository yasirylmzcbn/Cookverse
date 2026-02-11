using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    void Start()
    {
        GetComponent<Rigidbody>().linearVelocity = transform.forward * speed;
    }

    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        Enemy enemy = collision.collider.GetComponentInParent<Enemy>(); //currently hitting the enemy body
        if (enemy != null)
        {
            enemy.Damage(1);
        }
        Destroy(gameObject);
    }
}
