using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Movement & Range")]
    public float speed = 2f;
    public float chaseRadius = 15f;
    public float attackRadius = 7f; // Tầm đánh xa
    
    [Header("Ranged Attack Settings")]
    public float attackCooldown = 2.5f;
    public float attackDuration = 1f;
    [Range(0f, 1f)] public float fireProjectileRatio = 0.5f; // Thời điểm phóng đạn
    public GameObject projectilePrefab;
    public Transform firePoint; // Tọa độ đạn bay ra (kéo thả 1 object rỗng nằm ở tay Boss vào đây, hoặc bỏ trống)
    public int damage = 15;
    
    [Header("Phase 2 Settings (Thả đệ)")]
    public GameObject batPrefab;
    public int numberOfBats = 4;
    
    [Header("Animation State Names")]
    public string idleAnimName = "ArchDemon_Idle";
    public string attackAnimName = "ArchDemon_Attack";
    public string dieAnimName = "ArchDemon_Die"; // Anim khi lặn mất

    private Health health;
    private Animator anim;
    private Rigidbody2D rb;
    private Transform player;
    
    private bool isAttacking = false;
    private bool isPhase2 = false; // Đánh dấu đã thả đệ chưa
    private bool isHidden = false; // Đang tàng hình
    private float nextAttackTime = 0f;
    
    private List<GameObject> activeBats = new List<GameObject>();
    private Vector3 initialPosition;
    private bool isInvulnerable = false; // Bất tử 1 giây khi mới hồi sinh

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();
        
        // Lưu lại vị trí ngai vàng ban đầu
        initialPosition = transform.position;
        
        GameObject p = GameObject.Find("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        // Đang tàng hình hoặc không thấy Player thì nghỉ
        if (player == null || isHidden) return;
        
        // Mới hồi sinh, đang trong thời gian 1 giây đứng im gầm thét
        if (isInvulnerable)
        {
            rb.linearVelocity = Vector2.zero;
            return; 
        }

        // KÍCH HOẠT PHASE 2: Nếu máu <= 50% và chưa từng thả đệ
        if (health != null && health.currentHealth <= health.maxHealth / 2 && !isPhase2)
        {
            StartCoroutine(Phase2Transition());
            return;
        }

        // Đang vung tay niệm chú ném đạn thì đứng yên
        if (isAttacking) 
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // Chạy rượt Player nếu ngoài tầm ném
        if (distance <= chaseRadius && distance > attackRadius)
        {
            MoveTowardsPlayer();
        }
        // Player lọt vào tầm ném đạn
        else if (distance <= attackRadius)
        {
            rb.linearVelocity = Vector2.zero;
            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }
            else 
            {
                anim.Play(idleAnimName);
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
        
        // Vì Boss không có Anim Run, nên ta tiếp tục dùng Anim Idle khi nó trôi về phía Player
        anim.Play(idleAnimName);
        
        // Lật mặt Boss
        if (direction.x < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (direction.x > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.Play(attackAnimName); // Diễn hoạt ảnh ném

        // Chờ đúng thời điểm tay chỉ về phía trước (Delay Ratio)
        float delay = attackDuration * fireProjectileRatio;
        yield return new WaitForSeconds(delay);

        // Sinh ra cục đạn (NẾU CÓ)
        if (projectilePrefab != null && player != null)
        {
            Transform spawnPoint = firePoint != null ? firePoint : transform;
            GameObject proj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            
            // Tính hướng bay từ vị trí tay Boss thẳng tới mặt Player
            Vector2 shootDir = ((Vector2)player.position - (Vector2)spawnPoint.position).normalized;
            
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null)
            {
                ep.Setup(shootDir, damage);
            }
        }
        else if (player != null)
        {
            // NẾU KHÔNG CÓ ĐẠN (Chỉ là đòn đánh xa bình thường): 
            // Gây sát thương trực tiếp nếu Player vẫn đứng trong vùng ảnh hưởng của đòn đánh
            float currentDist = Vector2.Distance(transform.position, player.position);
            if (currentDist <= attackRadius + 0.2f)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null) stats.DamageShield((float)damage);
            }
        }

        // Chờ diễn nốt phần còn lại của hoạt ảnh
        yield return new WaitForSeconds(attackDuration - delay);
        isAttacking = false;
    }

    IEnumerator Phase2Transition()
    {
        isPhase2 = true;
        isHidden = true;
        isAttacking = false; // Hủy đòn đánh dở dang
        rb.linearVelocity = Vector2.zero;

        // Bật Anim Chết
        anim.Play(dieAnimName);
        
        // Chờ 1.5 giây cho diễn xong Anim chết rồi mới biến mất
        yield return new WaitForSeconds(1.5f);
        
        // Tàng hình Boss (Tắt hiển thị và tắt va chạm)
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        
        // Sinh ra bầy Dơi
        SpawnBats();
        
        // Vòng lặp vô tận: Liên tục kiểm tra xem dơi đã chết hết chưa
        while (activeBats.Count > 0)
        {
            // Xóa những con dơi đã bị Player tiêu diệt khỏi danh sách
            activeBats.RemoveAll(b => b == null);
            yield return new WaitForSeconds(0.5f); // Nghỉ nửa giây rồi kiểm tra tiếp
        }
        
        // DƠI ĐÃ CHẾT HẾT! BOSS CHÍNH THỨC TRỞ LẠI!
        transform.position = initialPosition;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        anim.Play(idleAnimName);
        
        // Bật trạng thái đứng im 1 giây để công bằng cho Player
        isInvulnerable = true;
        isHidden = false;
        
        yield return new WaitForSeconds(1f);
        
        // Chính thức tiếp tục cuộc chiến
        isInvulnerable = false;
    }

    void SpawnBats()
    {
        if (batPrefab == null) return;
        
        for (int i = 0; i < numberOfBats; i++)
        {
            // Đẻ ngẫu nhiên xung quanh vị trí boss bán kính 3 mét
            Vector2 randomPos = (Vector2)transform.position + Random.insideUnitCircle * 3f;
            GameObject bat = Instantiate(batPrefab, randomPos, Quaternion.identity);
            activeBats.Add(bat);
        }
    }
}
