using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour
{
    [Header("Inputs")]
    public InputAction InteractAction;

    [Header("Loot")]
    public Weapon[] weaponPool;           // Drag Rare_Gun, Epic_Sword, Legendary_Gun, etc. here
    public Transform spawnPoint;          // Optional - defaults to chest position

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    private bool isOpened = false;

    void Start()
    {
        InteractAction.Enable();
    }

    void Update()
    {
        if (isOpened) return;

        if (InteractAction.WasPressedThisFrame() && PlayerInRange())
        {
            OpenChest();
        }
    }

    private bool PlayerInRange()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRadius, playerLayer);
        return hit != null;
    }

    private void OpenChest()
    {
        if (weaponPool == null || weaponPool.Length == 0) return;

        isOpened = true;

        Weapon chosen = weaponPool[Random.Range(0, weaponPool.Length)];
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up * 0.5f;

        Weapon dropped = Instantiate(chosen, pos, Quaternion.identity);
        dropped.gameObject.SetActive(true); // ensure it's ready to be picked up

        if (sr != null && openSprite != null)
            sr.sprite = openSprite;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}