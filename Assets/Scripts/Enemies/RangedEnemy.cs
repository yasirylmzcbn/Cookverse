using UnityEngine;
using System.Collections;

public class RangedEnemy : Enemy
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Projectile")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;

    private PlayerController player;
    private Transform playerTransform;
    private bool canAttack;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        canAttack = true;
    }

    protected override void CheckAttack()
    {
        if (!canAttack) return;

        float sqrDistance = (playerTransform.position - transform.position).sqrMagnitude;

        if (sqrDistance <= attackRange * attackRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;

        Attack();

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    void Attack()
    {
        if (enemyBulletPrefab == null || firePoint == null) return;

        Vector3 direction = (playerTransform.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(
            enemyBulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }

        bullet.GetComponent<EnemyBullet>().damage = damage;
    }
}