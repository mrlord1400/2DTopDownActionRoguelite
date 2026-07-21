using UnityEngine;

public class GoblinAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2.5f;
    public float chaseRadius = 5f;
    public float attackRadius = 1f;

    [Header("Attack")]
    public float attackCooldown = 1.2f;
    public int damage = 10;
    private float nextAttackTime = 0f;

    [Header("Animation State Names")]
    public string idleAnimName = "Goblin_Idle";
    public string runAnimName = "Goblin_Run";
    public string attackAnimName = "Goblin_Attack";

    [Header("Sprite Configuration")]
    public bool isFacingRightByDefault = true; // Tick nếu ảnh gốc của quái quay mặt sang phải

    private Vector3 originalScale;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        originalScale = transform.localScale;
        // Tự động tìm chính xác tên PlayerCharacter có trong Hierarchy của bạn
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            
            // Xóa bỏ va chạm vật lý (đẩy nhau) giữa tất cả các Collider của Quái và Player
            Collider2D[] playerCols = playerObj.GetComponentsInChildren<Collider2D>();
            Collider2D[] myCols = GetComponentsInChildren<Collider2D>();
            
            foreach(var pCol in playerCols)
            {
                foreach(var mCol in myCols)
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
                foreach(var myC in allMyCols)
                {
                    foreach(var otherC in otherCols)
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

        // Tính khoảng cách đến nhân vật chính
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseRadius && distance > attackRadius)
        {
            MoveTowardsPlayer();
        }
        else if (distance <= attackRadius)
        {
            // Đến sát thì dừng lại bằng công nghệ Unity 6 (linearVelocity) và tấn công
            rb.linearVelocity = Vector2.zero; 

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            // Khi người chơi chạy quá xa, quái đứng im thở
            rb.linearVelocity = Vector2.zero;
            anim.Play(idleAnimName);
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        
        // Di chuyển bằng lực vật lý để không đi xuyên qua lớp Wall (Tường) của map
        rb.linearVelocity = direction * speed;

        // Bật chuyển động chạy
        anim.Play(runAnimName);

        
        // Lật mặt con Quái dựa theo hướng gốc của ảnh vẽ
        float flipMultiplier = isFacingRightByDefault ? 1f : -1f;

        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * flipMultiplier, originalScale.y, originalScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * -flipMultiplier, originalScale.y, originalScale.z);
    }

    void Attack()
    {
        anim.Play(attackAnimName);
        Debug.Log(gameObject.name + " đã chém trúng Player!");
    }

    // Vẽ vòng tròn bán kính ảo trên Scene để bạn dễ tùy chỉnh tầm nhìn của quái
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}