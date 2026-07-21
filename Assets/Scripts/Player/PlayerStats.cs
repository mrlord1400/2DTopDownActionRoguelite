using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float staminaMax = 100f;
    [SerializeField] private float staminaRegenRate = 20f;   // stamina/giây (tốc độ hồi tối đa), chỉnh được trong Inspector
    [SerializeField] private float staminaRegenDelay = 1f;   // thời gian chờ trước khi hồi
    [Tooltip("Đường cong hồi stamina: trục X = thời gian (giây) kể từ lúc bắt đầu hồi, trục Y = hệ số nhân (0-1) áp lên staminaRegenRate. Mặc định: hồi chậm lúc đầu rồi tăng dần.")]
    [SerializeField]
    private AnimationCurve staminaRegenCurve = new AnimationCurve(
        new Keyframe(0f, 0.3f),
        new Keyframe(1f, 1f));
    [SerializeField] private float staminaRegenCurveDuration = 1f; // thời gian để curve đạt hệ số cuối (sau đó giữ nguyên hệ số cuối)
    private float stamina;
    private float staminaRegenTimer;
    private float staminaRegenElapsed;
    private bool staminaIsRegenerating;

    [Header("Shield Settings")]
    [SerializeField] private float shieldMax = 100f;
    [SerializeField] private float shieldRegenRate = 10f;    // shield/giây, chỉnh được trong Inspector
    [SerializeField] private float shieldRegenDelay = 3f;    // cooldown bình thường sau khi nhận dmg
    [SerializeField] private float shieldBreakRegenDelay = 6f; // cooldown dài hơn khi shield bị phá vỡ hoàn toàn (về 0)
    private float shield;
    private float shieldRegenTimer;
    private bool shieldIsRegenerating;

    [Header("Health Settings")]
    [SerializeField] private float healthMax = 100f;
    private float health;

    [Header("Invincibility")]
    [Tooltip("Nếu để trống, sẽ tự tìm PlayerController trên cùng GameObject. Khi IsInvincible() = true, mọi damage vào shield/health sẽ bị bỏ qua.")]
    [SerializeField] private PlayerController playerController;

    [Header("Effects & Audio")]
    [SerializeField] private FlashEffect flashEffect;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    // ============ EVENTS (dùng cho UI: health bar, shield bar, stamina bar) ============
    // Tham số: (giá trị hiện tại, giá trị max)
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnShieldChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnDeath;

    // Public getters
    public float Stamina => stamina;
    public float StaminaMax => staminaMax;
    public float Shield => shield;
    public float ShieldMax => shieldMax;
    public float Health => health;
    public float HealthMax => healthMax;

    private bool isDead = false;

    void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (flashEffect == null)
        {
            flashEffect = GetComponent<FlashEffect>();
        }

        stamina = staminaMax;
        shield = shieldMax;
        health = healthMax;

        // Bắn event 1 lần lúc khởi động để UI tự khởi tạo đúng giá trị ban đầu
        OnStaminaChanged?.Invoke(stamina, staminaMax);
        OnShieldChanged?.Invoke(shield, shieldMax);
        OnHealthChanged?.Invoke(health, healthMax);
    }

    void Update()
    {
        HandleStaminaRegen();
        HandleShieldRegen();
    }

    private bool IsInvincible()
    {
        return playerController != null && playerController.IsInvincible();
    }

    // ============ STAMINA ============

    private void HandleStaminaRegen()
    {
        if (stamina >= staminaMax) return;

        if (staminaRegenTimer > 0f)
        {
            staminaRegenTimer -= Time.deltaTime;
            return;
        }

        if (!staminaIsRegenerating)
        {
            staminaIsRegenerating = true;
            staminaRegenElapsed = 0f;
            Debug.Log("Stamina bắt đầu hồi lại.");
        }

        staminaRegenElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(staminaRegenElapsed / staminaRegenCurveDuration);
        float multiplier = staminaRegenCurve.Evaluate(t);

        stamina = Mathf.Min(stamina + staminaRegenRate * multiplier * Time.deltaTime, staminaMax);
        OnStaminaChanged?.Invoke(stamina, staminaMax);
    }

    /// <summary>
    /// Trừ stamina cho 1 hành động (VD: dash tốn 30 stamina).
    /// Trả về false nếu không đủ stamina để thực hiện hành động.
    /// </summary>
    public bool UseStamina(float amount)
    {
        if (stamina < amount)
        {
            Debug.Log($"Không đủ stamina! Cần {amount}, hiện có {stamina}.");
            return false;
        }

        stamina -= amount;
        staminaRegenTimer = staminaRegenDelay; // reset cooldown hồi (kể cả khi đang hồi dở)
        staminaIsRegenerating = false;

        Debug.Log($"Trừ {amount} stamina. Còn lại: {stamina}/{staminaMax}");
        OnStaminaChanged?.Invoke(stamina, staminaMax);
        return true;
    }

    // ============ SHIELD ============

    private void HandleShieldRegen()
    {
        if (shield >= shieldMax) return;

        if (shieldRegenTimer > 0f)
        {
            shieldRegenTimer -= Time.deltaTime;
            return;
        }

        if (!shieldIsRegenerating)
        {
            shieldIsRegenerating = true;
            Debug.Log("Shield bắt đầu hồi lại.");
        }

        shield = Mathf.Min(shield + shieldRegenRate * Time.deltaTime, shieldMax);
        OnShieldChanged?.Invoke(shield, shieldMax);
    }

    /// <summary>
    /// Trừ shield khi nhận damage. Nếu shield không đủ để chịu hết damage,
    /// phần dư sẽ được tự động chuyển qua trừ máu. Bỏ qua nếu đang invincible (VD: đang dash).
    /// </summary>
    public void DamageShield(float amount)
    {
        if (IsInvincible())
        {
            Debug.Log("Đang bất tử (invincible), bỏ qua damage.");
            return;
        }
        if (isDead)
        {
            Debug.Log("Đang chết, bỏ qua damage.");
            return;
        }

        // Phát âm thanh khi bị đánh trúng
        if (hitSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);
        }

        if (shield <= 0f)
        {
            // Shield đã hết từ trước, toàn bộ damage dồn vào máu
            DamageHealth(amount);
            return;
        }

        float remaining = amount - shield;
        shield = Mathf.Max(shield - amount, 0f);

        // Nếu shield bị phá vỡ hoàn toàn (về 0), dùng cooldown hồi dài hơn bình thường
        shieldRegenTimer = shield <= 0f ? shieldBreakRegenDelay : shieldRegenDelay;
        shieldIsRegenerating = false;

        Debug.Log($"Trừ {amount} shield. Còn lại: {shield}/{shieldMax}");
        OnShieldChanged?.Invoke(shield, shieldMax);

        if (shield <= 0f && remaining > 0f)
        {
            DamageHealth(remaining);
        }
    }

    // ============ HEALTH ============

    /// <summary>
    /// Trừ máu trực tiếp. Được gọi khi shield <= 0 (hoặc damage bỏ qua shield).
    /// </summary>
    public void DamageHealth(float amount)
    {
        if (IsInvincible())
        {
            Debug.Log("Đang bất tử (invincible), bỏ qua damage.");
            return;
        }
        if (isDead)
        {
            Debug.Log("Đang chết, bỏ qua damage.");
            return;
        }

        health = Mathf.Max(health - amount, 0f);
        Debug.Log($"Trừ {amount} máu. Còn lại: {health}/{healthMax}");

        if (flashEffect != null) flashEffect.FlashRed();

        OnHealthChanged?.Invoke(health, healthMax);

        if (health <= 0f)
        {
            GameOver();
        }
    }

    /// <summary>
    /// Hồi máu khi dùng vật phẩm.
    /// </summary>
    public void HealHealth(float amount)
    {
        health = Mathf.Min(health + amount, healthMax);
        Debug.Log($"Hồi {amount} máu. Hiện tại: {health}/{healthMax}");
        OnHealthChanged?.Invoke(health, healthMax);
    }

    private void GameOver()
    {
        Debug.Log("Player đã chết. Game Over.");

        // Phát âm thanh chết
        if (deathSound != null && Camera.main != null && isDead == false)
        {
            AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position);
        }

        if (flashEffect != null) flashEffect.FadeOutDeath();
        if (playerController != null) playerController.enabled = false;
        isDead = true;
        OnDeath?.Invoke();
    }

    // ============ SAVE / LOAD ============
    // Dùng khi load lại giá trị đã lưu, không qua logic damage/heal (không log, không check invincible/GameOver)

    public void SetHealth(float value)
    {
        health = Mathf.Clamp(value, 0f, healthMax);
        OnHealthChanged?.Invoke(health, healthMax);
    }

    public void SetShield(float value)
    {
        shield = Mathf.Clamp(value, 0f, shieldMax);
        OnShieldChanged?.Invoke(shield, shieldMax);
    }

    public void SetStamina(float value)
    {
        stamina = Mathf.Clamp(value, 0f, staminaMax);
        OnStaminaChanged?.Invoke(stamina, staminaMax);
    }

    // ============ BUFF HELPER METHODS ============

    public void IncreaseMaxHealth(float amount)
    {
        healthMax += amount;
        health += amount; // Also give current health equal to the boost
        Debug.Log($"Max Health boosted by {amount}! New Max: {healthMax}");
        OnHealthChanged?.Invoke(health, healthMax);
    }

    public void IncreaseMaxShield(float amount)
    {
        shieldMax += amount;
        shield += amount; // Also give current shield equal to the boost
        Debug.Log($"Max Shield boosted by {amount}! New Max: {shieldMax}");
        OnShieldChanged?.Invoke(shield, shieldMax);
    }
}