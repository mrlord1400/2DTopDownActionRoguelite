using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MeleeWeapon : Weapon
{
    [Header("Melee Configurations")]
    public float swingArc = 120f;
    public float swingDuration = 0.15f;
    public float attackCooldown = 0.3f;

    [Header("Damage Settings")]
    public int damage = 10;
    public float attackRange = 1.2f;
    public Transform attackPoint; // Optional offset point on the blade
    public LayerMask enemyLayer;  // Set this to your "Enemy" layer

    [Header("Audio Settings")]
    public AudioClip[] swingSounds; // Kéo 1 hoặc nhiều clip vào đây, sẽ chọn ngẫu nhiên mỗi lần đánh
    [Range(0f, 1f)]
    public float swingVolume = 1f;
    [Range(0f, 0.3f)]
    public float pitchVariance = 0.1f; // Random nhẹ cao độ để tiếng vung đỡ đơn điệu

    private AudioSource audioSource;
    private bool isSwinging = false;
    private float nextAttackTime;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public override void Attack()
    {
        if (isSwinging || Time.time < nextAttackTime) return;

        StartCoroutine(SwingRoutine());
        nextAttackTime = Time.time + attackCooldown;
    }

    private IEnumerator SwingRoutine()
    {
        isSwinging = true;
        Quaternion originalRotation = transform.localRotation;

        float halfArc = swingArc / 2f;
        Quaternion startRot = originalRotation * Quaternion.Euler(0, 0, halfArc);
        Quaternion endRot = originalRotation * Quaternion.Euler(0, 0, -halfArc);

        // Phát âm thanh vung vũ khí ngay khi bắt đầu đánh
        PlaySwingSound();

        // Deal damage right as the physical swing starts
        DealMeleeDamage();

        float elapsed = 0f;
        // 1. Swing Through Path
        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Lerp(startRot, endRot, elapsed / swingDuration);
            yield return null;
        }

        // 2. Return Smoothly to baseline positions
        elapsed = 0f;
        float returnDuration = 0.05f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Lerp(endRot, originalRotation, elapsed / returnDuration);
            yield return null;
        }

        transform.localRotation = originalRotation;
        isSwinging = false;
    }

    private void PlaySwingSound()
    {
        if (swingSounds == null || swingSounds.Length == 0) return;

        AudioClip clip = swingSounds[Random.Range(0, swingSounds.Length)];
        if (clip == null) return;

        // Random nhẹ pitch để tiếng đánh liên tục nghe tự nhiên hơn, không bị lặp y hệt
        audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        audioSource.PlayOneShot(clip, swingVolume);
    }

    private void DealMeleeDamage()
    {
        Vector3 detectionCenter = attackPoint != null ? attackPoint.position : transform.position;

        // Find all enemy colliders inside the swing hit box range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(detectionCenter, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Try to find the Health component on the object we hit
            if (enemy.TryGetComponent<Health>(out Health enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    private void OnDisable()
    {
        // Fixes the stalling bug: Stops the swing instantly if swapped away mid-attack
        StopAllCoroutines();
        isSwinging = false;
        transform.localRotation = Quaternion.identity;
    }

    private void OnDrawGizmosSelected()
    {
        // Draws the melee range circle in red when selecting the weapon in the Editor
        Gizmos.color = Color.red;
        Vector3 detectionCenter = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(detectionCenter, attackRange);
    }
}