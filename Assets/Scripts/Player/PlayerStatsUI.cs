using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Fill Images")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image shieldFill;
    [SerializeField] private Image staminaFill;

    void OnEnable()
    {
        if (playerStats == null) return;

        playerStats.OnHealthChanged += UpdateHealthBar;
        playerStats.OnShieldChanged += UpdateShieldBar;
        playerStats.OnStaminaChanged += UpdateStaminaBar;
    }

    void OnDisable()
    {
        if (playerStats == null) return;

        playerStats.OnHealthChanged -= UpdateHealthBar;
        playerStats.OnShieldChanged -= UpdateShieldBar;
        playerStats.OnStaminaChanged -= UpdateStaminaBar;
    }

    void Start()
    {
        // Đảm bảo UI hiển thị đúng giá trị ngay khi vào scene,
        // phòng trường hợp UI Start() chạy sau PlayerStats Start() nên bị lỡ event đầu tiên
        if (playerStats == null) return;

        UpdateHealthBar(playerStats.Health, playerStats.HealthMax);
        UpdateShieldBar(playerStats.Shield, playerStats.ShieldMax);
        UpdateStaminaBar(playerStats.Stamina, playerStats.StaminaMax);
    }

    private void UpdateHealthBar(float current, float max)
    {
        if (healthFill != null) healthFill.fillAmount = current / max;
    }

    private void UpdateShieldBar(float current, float max)
    {
        if (shieldFill != null) shieldFill.fillAmount = current / max;
    }

    private void UpdateStaminaBar(float current, float max)
    {
        if (staminaFill != null) staminaFill.fillAmount = current / max;
    }
}