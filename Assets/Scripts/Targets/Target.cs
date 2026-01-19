using UnityEngine;

public class Target : MonoBehaviour, IDamagable
{
    public float health = 50f;

    public void Damage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
        }
        Destroy(gameObject);
    }
}
