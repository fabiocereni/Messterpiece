using UnityEngine;

public class Target : MonoBehaviour, IDamagable
{
    public float health = 50f;

    public void Damage(float amount)
    {
        health -= amount;
        if (health <= 0f) Destroy(gameObject);
    }
}
