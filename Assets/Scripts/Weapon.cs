using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Identity")]
    public string weaponName;
    public Sprite weaponSprite;
    protected Collider2D weaponCollider;
    protected Rigidbody2D rb2d;
    public bool IsEquipped { get; private set; } = false;
    public Vector3 holdOffset;

    protected virtual void Awake()
    {
        weaponCollider = GetComponent<Collider2D>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Virtual method that derived classes (like Melee or Ranged) will override
    public virtual void Attack()
    {
        // Core attack logic wrapper
    }

    public virtual void PickUp(Transform handTransform)
    {
        IsEquipped = true;
        if (weaponCollider != null) weaponCollider.enabled = false;
        if (rb2d != null) rb2d.bodyType = RigidbodyType2D.Kinematic;

        transform.SetParent(handTransform);
        transform.localPosition = holdOffset;
        transform.localRotation = Quaternion.identity;

        gameObject.SetActive(true);
    }

    public virtual void Drop(Vector3 dropPosition)
    {
        IsEquipped = false;

        // Separate from the player anchor
        transform.SetParent(null);
        transform.position = dropPosition;
        transform.rotation = Quaternion.identity;

        // REMOVE OR COMMENT OUT: transform.localScale = Vector3.one;
        // Leaving this out allows the weapon to keep its intended scale (like 5, 5, 1) on the ground!

        // Turn physics and ground colliders back on
        if (weaponCollider != null) weaponCollider.enabled = true;

        if (rb2d != null)
        {
            // Fixes the Unity 6 deprecation warning perfectly
            rb2d.bodyType = RigidbodyType2D.Dynamic;
        }

        gameObject.SetActive(true);
    }

    // Add this to handle the visual flip
    public void FlipWeapon(bool facingRight)
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            // If facing left (facingRight == false), flip the sprite
            sr.flipX = !facingRight;
        }
    }
}