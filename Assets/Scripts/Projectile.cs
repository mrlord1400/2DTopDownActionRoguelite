using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public int damage;

    [Header("AoE Settings")]
    public bool isAoE = false;
    public float aoeRadius = 2f;
    public LayerMask enemyLayer;
    public GameObject explosionVFX;
    public float vfxDuration = 2f; // How many seconds the explosion stays on screen

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Instead of tag checking, see if the object has a Health script
        if (other.TryGetComponent<Health>(out Health enemyHealth))
        {
            if (isAoE)
            {
                Explode();
            }
            else
            {
                // We already found the Health component, so we pass it directly!
                HitSingleTarget(enemyHealth);
            }
            Destroy(gameObject);
        }
        // 2. Keep tag checks only for static static environment objects (Walls/Decor)
        else if (other.CompareTag("Walls") || other.CompareTag("Decor"))
        {
            if (isAoE) Explode();
            Destroy(gameObject);
        }
    }

    // Adjusted to take the Health component directly (saves an extra GetComponent call!)
    private void HitSingleTarget(Health enemyHealth)
    {
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }

    private void Explode()
    {
        if (explosionVFX != null)
        {
            // Spawn the effect
            GameObject spawnedVFX = Instantiate(explosionVFX, transform.position, Quaternion.identity);

            // Clean it up automatically after vfxDuration seconds
            Destroy(spawnedVFX, vfxDuration);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<Health>(out Health enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isAoE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}