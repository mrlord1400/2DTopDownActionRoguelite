using UnityEngine;

public class BloodMonsterAI : MonoBehaviour
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
    public string idleAnimName = "Idle";
    public string runAnimName = "Run";
    public string attackAnimName = "Attack";

    [Header("Sprite Configuration")]
    public bool isFacingRightByDefault = false; // Blood Monster quay mặt sang trái mặc định

    private Vector3 originalScale;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        originalScale = transform.localScale;
        
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            
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
                Attack();
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
        
        rb.linearVelocity = direction * speed;

        anim.Play(runAnimName);
        
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
