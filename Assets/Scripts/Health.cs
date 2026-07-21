using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 30;
    public int currentHealth;

    [Header("Audio Settings")]
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Header("Stun Settings")]
    public float stunDuration = 0.3f;

    private EnemyAI enemyAI;

    private bool isDead = false;
    private Animator animator;
    private FlashEffect flashEffect;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        flashEffect = GetComponent<FlashEffect>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " bị chém " + amount + " máu! Còn lại: " + currentHealth);

        // Phát âm thanh bị đánh trúng
        if (hitSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);
        }

        if (flashEffect != null)
        {
            flashEffect.FlashRed();
        }

        if (enemyAI != null)
        {
            enemyAI.Stun(stunDuration);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " ĐÃ BỊ TIÊU DIỆT!");

        if (deathSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position);
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Dừng vật lý NGAY LẬP TỨC trước khi tắt script AI
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // ngăn hẳn mọi lực/va chạm tác động tiếp
        }

        // Tắt toàn bộ các script AI khác để quái vật ngã gục nằm im, không bị trượt trên mặt đất
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.StopAllCoroutines(); // hủy AttackRoutine nếu đang chạy dở
                script.enabled = false;
            }
        }

        // Tắt va chạm để Player có thể đi xuyên qua xác chết
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 1.5f);
    }
}
