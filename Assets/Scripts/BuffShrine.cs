using UnityEngine;

public class BuffShrine : MonoBehaviour
{
    public enum BuffType { MaxHealth, MaxShield, MoveSpeed }

    [Header("Buff Values")]
    [SerializeField] private float healthIncreaseAmount = 25f;
    [SerializeField] private float shieldIncreaseAmount = 25f;
    [SerializeField] private float speedIncreaseAmount = 1.5f;

    [Header("Shrine Settings")]
    [SerializeField] private bool canInteractMultipleTimes = false;
    private bool hasBeenUsed = false;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private SpriteRenderer shrineSpriteRenderer;
    [SerializeField] private Color usedColor = Color.gray;
    [SerializeField] private AudioClip buffSound;

    private bool isPlayerInZone = false;
    private PlayerController player;

    private void Update()
    {
        // When player presses 'E' (Interact) while in range and shrine is unused
        if (isPlayerInZone && !hasBeenUsed && player != null)
        {
            if (player.InteractAction.WasPressedThisFrame())
            {
                ApplyRandomBuff();
            }
        }
    }

    private void ApplyRandomBuff()
    {
        hasBeenUsed = true;

        PlayerStats stats = player.GetComponent<PlayerStats>();

        // Roll a random buff type (0 = HP, 1 = Shield, 2 = Speed)
        BuffType randomBuff = (BuffType)Random.Range(0, 3);

        switch (randomBuff)
        {
            case BuffType.MaxHealth:
                if (stats != null) stats.IncreaseMaxHealth(healthIncreaseAmount);
                Debug.Log("<color=green>BUFF GRANTED:</color> Max Health Increased!");
                break;

            case BuffType.MaxShield:
                if (stats != null) stats.IncreaseMaxShield(shieldIncreaseAmount);
                Debug.Log("<color=cyan>BUFF GRANTED:</color> Max Shield Increased!");
                break;

            case BuffType.MoveSpeed:
                player.IncreaseMoveSpeed(speedIncreaseAmount);
                Debug.Log("<color=yellow>BUFF GRANTED:</color> Movement Speed Increased!");
                break;
        }

        // Play audio if assigned
        if (buffSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(buffSound, Camera.main.transform.position);
        }

        // Visually gray out the shrine to indicate it has been used once
        if (shrineSpriteRenderer != null)
        {
            shrineSpriteRenderer.color = usedColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasBeenUsed)
        {
            isPlayerInZone = true;
            player = collision.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = false;
            player = null;
        }
    }
}