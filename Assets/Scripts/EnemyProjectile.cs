using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 7f;
    public float lifetime = 4f; // Sống được 4 giây nếu bay không trúng ai thì tự nổ
    private int damage;
    private Vector2 direction;
    private Rigidbody2D rb;

    public void Setup(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;
        rb = GetComponent<Rigidbody2D>();
        
        // Xoay đầu viên đạn về phía trước cho đẹp (nếu đạn có hình mũi tên/mũi giáo)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Hẹn giờ tự hủy
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rb != null)
        {
            // Dùng Rigidbody để bay (nếu có gắn Rigidbody)
            rb.linearVelocity = direction * speed;
        }
        else 
        {
            // Nếu không có Rigidbody thì tự dịch chuyển
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.DamageShield((float)damage); 
            }
            Destroy(gameObject); // Bắn trúng người thì nổ đạn
        }
        else if (collision.CompareTag("Walls"))
        {
            Destroy(gameObject); // Bắn trúng tường thì nổ đạn
        }
    }
}
