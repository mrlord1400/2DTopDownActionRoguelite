using UnityEngine;

public class DummyTarget : MonoBehaviour
{
    public int health = 999999;

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log("Dummy took damage! Health left: " + health);

        if (health <= 0)
        {
            Debug.Log("Dummy Defeated!");
            Destroy(gameObject);
        }
    }
}