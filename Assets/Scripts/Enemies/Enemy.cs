using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    private bool healthSet = false;
    [SerializeField] private float health;
    private bool isDead = false;
    [SerializeField] private Color damageColor = Color.yellow;
    [SerializeField] private float flashDuration = 0.1f;

    [SerializeField] protected float attackRange = 2f;
    public float AttackRange => attackRange;
    private bool isStunned = false;

    Renderer rend;
    MaterialPropertyBlock mpb;
    Color originalColor;

    [System.Serializable]
    public class DropEntry
    {
        public ItemData itemData;
        public int weight = 1;
    }
    [Header("Item Drops")]
    [Tooltip("This drop list is weight-based.")]
    [SerializeField] private List<DropEntry> dropList;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();

        if (rend != null)
            originalColor = rend.material.color;
    }

    private void Update()
    {
        if (isStunned) return;
        CheckAttack();
    }

    protected abstract void CheckAttack();

    void FlashDamage()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        if (rend != null)
            rend.material.color = originalColor * damageColor;

        yield return new WaitForSeconds(flashDuration);

        if (rend != null)
        rend.material.color = originalColor;
    }

    public void Damage(int d)
    {
        health -= d;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            FlashDamage();
        }
    }

    public void SetHealthMultiplier(float multiplier)
    {
        if (!healthSet)
        {
            health *= multiplier;
            healthSet = true;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        if (QuestManager.Instance != null)
            QuestManager.Instance.RegisterEnemyKilled();

        if (dropList != null && dropList.Count > 0)
        {
            int totalWeight = 0;
            foreach (var entry in dropList)
            {
                if (entry.weight > 0)
                    totalWeight += entry.weight;
            }

            if (totalWeight > 0)
            {
                int roll = Random.Range(0, totalWeight);

                foreach (var entry in dropList)
                {
                    if (entry.weight <= 0) continue;
                    roll -= entry.weight;

                    if (roll < 0)
                    {
                        Instantiate(entry.itemData.dropPrefab,
                                    transform.position,
                                    Quaternion.identity);
                        break;
                    }
                }
            }
        }

        Destroy(gameObject);
    }

    public void Stun(float duration)
    {
        StopCoroutine(nameof(StunRoutine)); //resets the duration if stunned while already stunned
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        EnemyMovementAI enemyMovement = GetComponent<EnemyMovementAI>();
        enemyMovement.StunMovement();

        yield return new WaitForSeconds(duration);
        isStunned = false;
        enemyMovement.RestoreMovement();
    }
}
