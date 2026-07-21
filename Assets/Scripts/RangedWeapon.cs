using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RangedWeapon : Weapon
{
    [Header("Ranged Configurations")]
    public GameObject projectilePrefab;
    public Transform firePoint; // Child object representing the tip of the gun barrel
    public float fireRate = 0.25f;
    public float projectileSpeed = 15f;

    [Header("Damage Settings")]
    public int damage = 8;

    [Header("Audio Settings")]
    public AudioClip[] fireSounds; // Kéo 1 hoặc nhiều clip vào đây, sẽ chọn ngẫu nhiên mỗi lần bắn
    [Range(0f, 1f)]
    public float fireVolume = 1f;
    [Range(0f, 0.3f)]
    public float pitchVariance = 0.1f; // Random nhẹ cao độ để tiếng bắn liên tục đỡ đơn điệu

    private AudioSource audioSource;
    private float nextFireTime;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public override void Attack()
    {
        if (Time.time < nextFireTime) return;

        FireProjectile();
        nextFireTime = Time.time + fireRate;
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // 1. Spawn the projectile at the barrel tip
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 2. Pass the damage value to your Projectile script
        Projectile bulletScript = bullet.GetComponent<Projectile>();
        if (bulletScript != null)
        {
            bulletScript.damage = this.damage;
        }

        // 3. Propel the bullet forward
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = firePoint.right * projectileSpeed;
        }

        // 4. Phát âm thanh bắn
        PlayFireSound();
    }

    private void PlayFireSound()
    {
        if (fireSounds == null || fireSounds.Length == 0) return;

        AudioClip clip = fireSounds[Random.Range(0, fireSounds.Length)];
        if (clip == null) return;

        audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        audioSource.PlayOneShot(clip, fireVolume);
    }
}