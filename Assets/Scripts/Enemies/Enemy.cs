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

    private Renderer[] enemyRenderers;
    private Material[][] enemyMaterials;
    private Color[][] originalColors;

    [System.Serializable]
    public class DropEntry
    {
        public ItemData itemData;
        public int weight = 1;
    }
    [Header("Item Drops")]
    [Tooltip("This drop list is weight-based.")]
    [SerializeField] private List<DropEntry> dropList;
    
    [Header("Audio")]
    [SerializeField] protected AudioClip attackSound;
    protected AudioSource audioSource;

    private void Awake()
    {
        enemyRenderers = GetComponentsInChildren<Renderer>(true);
        enemyMaterials = new Material[enemyRenderers.Length][];
        originalColors = new Color[enemyRenderers.Length][];

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i] == null)
                continue;

            enemyMaterials[i] = enemyRenderers[i].materials;
            originalColors[i] = new Color[enemyMaterials[i].Length];

            for (int m = 0; m < enemyMaterials[i].Length; m++)
            {
                Material mat = enemyMaterials[i][m];
                if (mat != null && mat.HasProperty("_Color"))
                    originalColors[i][m] = mat.color;
            }
        }
            
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound (global)
    }

    protected void PlayAttackSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyAttackSound();
            return;
        }

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
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
        for (int i = 0; i < enemyMaterials.Length; i++)
        {
            if (enemyMaterials[i] == null || originalColors[i] == null)
                continue;

            for (int m = 0; m < enemyMaterials[i].Length && m < originalColors[i].Length; m++)
            {
                Material mat = enemyMaterials[i][m];
                if (mat != null && mat.HasProperty("_Color"))
                    mat.color = originalColors[i][m] * damageColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < enemyMaterials.Length; i++)
        {
            if (enemyMaterials[i] == null || originalColors[i] == null)
                continue;

            for (int m = 0; m < enemyMaterials[i].Length && m < originalColors[i].Length; m++)
            {
                Material mat = enemyMaterials[i][m];
                if (mat != null && mat.HasProperty("_Color"))
                    mat.color = originalColors[i][m];
            }
        }
    }

    public void Damage(int d)
    {
        health -= d;
        
        // Play hit sound via SoundManager
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayEnemyHitSound();
        
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
        
        // Play enemy defeated sound via SoundManager
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayEnemyDefeatedSound();
        
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
