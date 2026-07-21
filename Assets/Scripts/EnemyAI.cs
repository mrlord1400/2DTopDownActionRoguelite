using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2.5f;
    public float chaseRadius = 5f;
    public float attackRadius = 1f;

    [Header("Attack")]
    public float attackCooldown = 1.2f;
    public int damage = 10;
    public float attackDuration = 0.5f; // thời gian quái đứng yên khi đánh, nên khớp độ dài animation attack
    [Range(0f, 1f)]
    public float attackDamageDelayRatio = 0.5f; // thời điểm (theo % thời lượng đòn đánh) mà damage thực sự áp lên player, 0.5 = giữa animation
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    [Header("Animation State Names")]
    public string idleAnimName = "Idle";
    public string runAnimName = "Run";
    public string attackAnimName = "Attack";

    [Header("Sprite Configuration")]
    public bool isFacingRightByDefault = true; // Tick nếu ảnh gốc của quái quay mặt sang phải

    [Header("Stun Lock Protection")]
    public int maxStunBeforeImmunity = 2;   // Số lần bị stun liên tiếp tối đa trước khi miễn nhiễm
    public float stunImmuneDuration = 10f;  // Thời gian miễn nhiễm stun

    private int stunCount = 0;
    private bool isStunImmune = false;

    private Vector3 originalScale;
    private Transform player;
    private PlayerStats playerStats;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isStunned = false;
    private Coroutine attackCoroutine;
    private Coroutine stunCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        originalScale = transform.localScale;

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerStats = playerObj.GetComponent<PlayerStats>();

            // Xóa bỏ va chạm vật lý (đẩy nhau) giữa tất cả các Collider của Quái và Player
            Collider2D[] playerCols = playerObj.GetComponentsInChildren<Collider2D>();
            Collider2D[] myCols = GetComponentsInChildren<Collider2D>();

            foreach (var pCol in playerCols)
            {
                foreach (var mCol in myCols)
                {
                    Physics2D.IgnoreCollision(mCol, pCol);
                }
            }
        }

        // Xóa bỏ va chạm vật lý giữa các Quái với nhau (Tránh tình trạng đùn đẩy nhau)
        GameObject[] otherEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        Collider2D[] allMyCols = GetComponentsInChildren<Collider2D>();

        foreach (GameObject enemy in otherEnemies)
        {
            if (enemy != this.gameObject)
            {
                Collider2D[] otherCols = enemy.GetComponentsInChildren<Collider2D>();
                foreach (var myC in allMyCols)
                {
                    foreach (var otherC in otherCols)
                    {
                        Physics2D.IgnoreCollision(myC, otherC);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        if (isStunned)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Đang trong lúc đánh thì đứng yên, không xử lý di chuyển/kiểm tra khoảng cách
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseRadius && distance > attackRadius)
        {
            MoveTowardsPlayer();
        }
        else if (distance <= attackRadius)
        {
            rb.linearVelocity = Vector2.zero;

            if (Time.time >= nextAttackTime)
            {
                attackCoroutine = StartCoroutine(AttackRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.Play(idleAnimName);
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;

        // Di chuyển bằng lực vật lý để không đi xuyên qua lớp Wall (Tường) của map
        rb.linearVelocity = direction * speed;

        anim.Play(runAnimName);

        // Lật mặt con Quái dựa theo hướng gốc của ảnh vẽ
        float flipMultiplier = isFacingRightByDefault ? 1f : -1f;

        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * flipMultiplier, originalScale.y, originalScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * -flipMultiplier, originalScale.y, originalScale.z);
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.Play(attackAnimName);

        // Chờ tới đúng thời điểm giữa animation (theo attackDamageDelayRatio) rồi mới gây damage
        float damageDelay = attackDuration * attackDamageDelayRatio;
        yield return new WaitForSeconds(damageDelay);

        // ĐOẠN ĐO KHOẢNG CÁCH TRƯỚC KHI TRỪ MÁU
        if (player != null)
        {
            float currentDist = Vector2.Distance(transform.position, player.position);
            if (currentDist <= attackRadius + 0.2f) // +0.2f bù trừ sai số
            {
                if (playerStats != null) playerStats.DamageShield(damage);
            }
        }

        Debug.Log(gameObject.name + " đã chém trúng Player!");

        // Chờ hết phần còn lại của animation trước khi cho phép di chuyển tiếp
        float remainingDuration = attackDuration - damageDelay;
        yield return new WaitForSeconds(remainingDuration);
        isAttacking = false;
    }

    // Vẽ vòng tròn bán kính ảo trên Scene để bạn dễ tùy chỉnh tầm nhìn của quái
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    public void Stun(float duration)
    {
        // Đang trong thời gian miễn nhiễm thì bỏ qua yêu cầu stun này
        if (isStunImmune) return;

        stunCount++; // Tính ngay lập tức, không phụ thuộc coroutine có chạy trọn vẹn hay không

        // Nếu đang stun rồi thì restart lại timer (bị đánh liên tiếp -> stun nối tiếp)
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine(duration));

        // Đủ số lần quy định -> kích hoạt miễn nhiễm NGAY, chặn các cú đánh tiếp theo trong cùng frame
        if (stunCount >= maxStunBeforeImmunity)
        {
            StartCoroutine(StunImmunityRoutine());
        }
    }

    private IEnumerator StunRoutine(float duration)
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        isAttacking = false;

        isStunned = true;
        rb.linearVelocity = Vector2.zero;
        anim.Play(idleAnimName);

        yield return new WaitForSeconds(duration);

        isStunned = false;
    }

    private IEnumerator StunImmunityRoutine()
    {
        isStunImmune = true;
        stunCount = 0;

        yield return new WaitForSeconds(stunImmuneDuration);

        isStunImmune = false;
    }
}