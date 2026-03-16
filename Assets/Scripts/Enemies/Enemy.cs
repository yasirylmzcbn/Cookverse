using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] private float health;
    private bool isDead = false;
    [SerializeField] private Color damageColor = Color.yellow;
    [SerializeField] private float flashDuration = 0.1f;

    [SerializeField] protected float attackRange = 2f;
    public float AttackRange => attackRange;

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
        mpb = new MaterialPropertyBlock();

        // Grab original color from the material
        rend.GetPropertyBlock(mpb);
        originalColor = rend.sharedMaterial.color;
    }

    private void Update()
    {
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
        mpb.SetColor("_BaseColor", damageColor);
        rend.SetPropertyBlock(mpb);

        yield return new WaitForSeconds(flashDuration);

        mpb.SetColor("_BaseColor", originalColor);
        rend.SetPropertyBlock(mpb);
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
}
