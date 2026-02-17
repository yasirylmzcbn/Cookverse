using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public float health;
    
    
    
    
    public Color damageColor = Color.yellow;
    public float flashDuration = 0.1f;
    public int contactDamage = 10;

    [HideInInspector] public EnemySpawner spawner;

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

        // URP/HDRP: "_BaseColor"
        // Built-in: "_Color"

        //if (mpb.HasColor("_BaseColor"))
    
    {
        
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (other.CompareTag("Pickup"))
            return;

            var hitPlayer = other.GetComponentInParent<PlayerController>();
        if (hitPlayer == null)
            return;

        hitPlayer.TakeDamage(contactDamage);
    }

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

        spawner.KilledEnemy();
        Destroy(gameObject);
    }
}
