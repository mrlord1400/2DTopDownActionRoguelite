using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Inputs")]
    public InputAction MoveAction;
    public InputAction LookAction;
    public InputAction AttackAction;
    public InputAction DashAction;
    public InputAction InteractAction; // New: E Key Action
    public InputAction SwitchAction;   // New: R Key Action

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    private Vector2 lastMoveDirection;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing;
    private bool canDash = true;
    private bool isInvincible;

    [Header("Weapon System")]
    public Transform weaponHoldPoint;
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private LayerMask weaponLayer;

    // 2-Slot Inventory System
    private Weapon[] weaponSlots = new Weapon[2];
    private int currentSlotIndex = 0;

    private Camera mainCamera;
    private Animator am;
    private SpriteRenderer sr;

    [Header("Stats")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Audio")]
    [SerializeField] public AudioSource audioSource;      // Drag an AudioSource component here
    [SerializeField] public AudioClip footstepLeftClip;   // Left foot sound
    [SerializeField] public AudioClip footstepRightClip;  // Right foot sound
    [SerializeField] public float footstepInterval = 0.35f; // Time between steps (seconds)
    [SerializeField] public AudioClip dashClip;            // Dash sound
    [SerializeField] public AudioClip interactSwitchClip;  // Shared Interact/Switch sound

    private float footstepTimer;
    private bool nextFootIsLeft = true;

    void Start()
    {
        am = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        MoveAction.Enable();
        LookAction.Enable();
        AttackAction.Enable();
        DashAction.Enable();
        InteractAction.Enable();
        SwitchAction.Enable();

        playerStats = GetComponent<PlayerStats>();

        // Fallback: try to grab an AudioSource on this object if one wasn't assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isDashing) return;

        HandleMovement();
        HandleAiming();

        // Attack Input Action Check
        if (AttackAction.WasPressedThisFrame() && GetCurrentWeapon() != null)
        {
            GetCurrentWeapon().Attack();
        }

        // Switch Weapon Input Action Check (R Key)
        if (SwitchAction.WasPressedThisFrame())
        {
            SwitchWeaponSlot();
            PlaySound(interactSwitchClip);
        }

        // Proximity Pickup Input Action Check (E Key)
        if (InteractAction.WasPressedThisFrame())
        {
            TryPickupWeapon();
            PlaySound(interactSwitchClip);
        }
    }

    private void HandleMovement()
    {
        Vector2 move = MoveAction.ReadValue<Vector2>();

        if (MoveAction.IsPressed())
        {
            am.SetBool("Move", true);
            SpriteDirectionCheck(move);
            lastMoveDirection = move.normalized;
            HandleFootsteps();
        }
        else
        {
            am.SetBool("Move", false);
            footstepTimer = 0f; // Reset so the first step after stopping plays immediately
        }

        Vector2 position = (Vector2)transform.position + move * moveSpeed * Time.deltaTime;
        transform.position = position;

        if (DashAction.WasPressedThisFrame() && canDash)
        {
            if (playerStats.UseStamina(30f))
            {
                Vector2 dashDir = move != Vector2.zero ? move.normalized : (lastMoveDirection != Vector2.zero ? lastMoveDirection : Vector2.right);
                StartCoroutine(DashRoutine(dashDir));
            }
        }
    }

    private void HandleFootsteps()
    {
        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            footstepTimer = footstepInterval;

            if (nextFootIsLeft)
            {
                PlaySound(footstepLeftClip);
            }
            else
            {
                PlaySound(footstepRightClip);
            }

            nextFootIsLeft = !nextFootIsLeft;
        }
    }

    private void HandleAiming()
    {
        if (weaponHoldPoint == null) return;

        Vector2 mouseWorldPos = GetMouseWorldPosition();
        Vector2 aimDirection = (mouseWorldPos - (Vector2)weaponHoldPoint.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        weaponHoldPoint.rotation = Quaternion.Euler(0, 0, angle);

        // Flipping threshold handling: Prevents the gun/sword from turning upside down when aiming left
        if (angle > 90 || angle < -90)
        {
            weaponHoldPoint.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            weaponHoldPoint.localScale = new Vector3(1, 1, 1);
        }
    }

    private void TryPickupWeapon()
    {
        // Search globally for weapons on the weapon layer within reach
        Collider2D[] weaponsInRange = Physics2D.OverlapCircleAll(transform.position, pickupRadius, weaponLayer);
        Weapon closestWeapon = null;
        float closestDistance = float.MaxValue;

        foreach (var col in weaponsInRange)
        {
            Weapon groundWeapon = col.GetComponent<Weapon>();
            if (groundWeapon != null && !groundWeapon.IsEquipped)
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestWeapon = groundWeapon;
                }
            }
        }

        if (closestWeapon != null)
        {
            EquipWeapon(closestWeapon);
        }
    }

    private void EquipWeapon(Weapon newWeapon)
    {
        // If current slot is occupied, drop that weapon back down onto the ground first
        if (weaponSlots[currentSlotIndex] != null)
        {
            weaponSlots[currentSlotIndex].Drop(transform.position);
        }

        // Add the new weapon to inventory slot and bind it to the hand point
        weaponSlots[currentSlotIndex] = newWeapon;
        newWeapon.PickUp(weaponHoldPoint);
    }

    private void SwitchWeaponSlot()
    {
        int nextSlotIndex = (currentSlotIndex + 1) % 2;

        // Hide old slot
        if (weaponSlots[currentSlotIndex] != null)
        {
            weaponSlots[currentSlotIndex].gameObject.SetActive(false);
        }

        // Move active index over
        currentSlotIndex = nextSlotIndex;

        // Display new slot
        if (weaponSlots[currentSlotIndex] != null)
        {
            weaponSlots[currentSlotIndex].gameObject.SetActive(true);
        }
    }

    public Weapon GetCurrentWeapon()
    {
        return weaponSlots[currentSlotIndex];
    }

    public Vector2 GetMouseWorldPosition()
    {
        Vector2 screenPosition = LookAction.ReadValue<Vector2>();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        canDash = false;
        isDashing = true;
        isInvincible = true;

        PlaySound(dashClip);

        float elapsedTime = 0f;
        while (elapsedTime < dashDuration)
        {
            transform.position += (Vector3)(direction * dashSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        isInvincible = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public bool IsInvincible() => isInvincible;

    void SpriteDirectionCheck(Vector2 move)
    {
        if (move.x < 0) sr.flipX = true;
        else if (move.x > 0) sr.flipX = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        moveSpeed += amount;
        Debug.Log($"Movement speed boosted by {amount}! New Speed: {moveSpeed}");
    }
}