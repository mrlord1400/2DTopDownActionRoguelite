using UnityEngine;
using System.Linq; // Added to help sort the hits by distance easily

public class LaserBeam : Projectile 
{
    [Header("Laser Settings")]
    public float laserDuration = 0.15f;
    public LayerMask hitLayers; 
    public float maxDistance = 50f;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        // 1. RaycastAll grabs EVERYTHING along the beam's line
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.right, maxDistance, hitLayers);
        
        // Sort the hits from closest to furthest so the laser handles impacts in the correct order
        hits = hits.OrderBy(h => h.distance).ToArray();

        Vector3 endPoint = transform.position + transform.right * maxDistance;

        // Loop through everything the laser pierced
        foreach (RaycastHit2D hit in hits)
        {
            // If it hits a solid wall or decor, the laser stops piercing here
            if (hit.collider.CompareTag("Walls") || hit.collider.CompareTag("Decor"))
            {
                endPoint = hit.point;
                if (isAoE) ExplodeAtPoint(endPoint);
                break; // Stop looking at any further targets behind the wall
            }

            // If it hits an enemy, damage them and keep going!
            if (hit.collider.TryGetComponent<Health>(out Health enemyHealth))
            {
                if (isAoE)
                {
                    ExplodeAtPoint(hit.point);
                }
                else
                {
                    enemyHealth.TakeDamage(damage);
                }
                
                // Optional: If you want the laser beam visual to stretch all the way to the LAST enemy hit:
                endPoint = hit.point;
            }
        }

        // 2. Draw the piercing laser line visually
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPoint);
        }

        Destroy(gameObject, laserDuration);
    }

    void Update()
    {
        if (lineRenderer != null)
        {
            lineRenderer.widthMultiplier = Mathf.Lerp(lineRenderer.widthMultiplier, 0f, Time.deltaTime * 15f);
        }
    }

    private void ExplodeAtPoint(Vector3 impactPoint)
    {
        if (explosionVFX != null)
        {
            GameObject spawnedVFX = Instantiate(explosionVFX, impactPoint, Quaternion.identity);
            Destroy(spawnedVFX, vfxDuration);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(impactPoint, aoeRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<Health>(out Health enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }
}