using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 10f; // Danno che il proiettile infligge

    void Start()
    {
        // Avvia una Coroutine per distruggere l'oggetto dopo 3 secondi
        StartCoroutine(DestroyAfterTime(3.0f));
    }

    void Update()
    {
        // Muove il proiettile in avanti (nella sua direzione locale)
        transform.Translate(0, 0, speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // IGNORA trigger zones logici
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerZone"))
        {
            return; // Non distruggere il proiettile
        }

        // IGNORA SphereCollider dei nemici (detection radius per AI, non hitbox)
        // La hitbox vera è la CapsuleCollider
        if (other is SphereCollider)
        {
            // Verifica se questo GameObject o il parent ha un TutorialEnemy/EnemyAI script
            // Se sì, è la detection sphere, quindi ignora
            if (other.GetComponent<TutorialEnemy>() != null ||
                other.GetComponentInParent<TutorialEnemy>() != null ||
                other.GetComponent<EnemyAI_NavMesh>() != null ||
                other.GetComponentInParent<EnemyAI_NavMesh>() != null)
            {
                return; // Passa attraverso la detection sphere del nemico
            }
        }

        // Controlla se l'oggetto colpito ha l'interfaccia IDamagable
        IDamagable target = other.GetComponent<IDamagable>();
        if (target != null)
        {
            // Applica il danno al bersaglio
            target.Damage(damage);
        }

        // Distrugge il proiettile SOLO se colpisce qualcosa di solido
        // (non TriggerZone o altri triggers logici)
        if (other.isTrigger && target == null)
        {
            return; // Non distruggere se è un trigger senza IDamagable
        }

        Destroy(this.gameObject);
    }

    // attende un tempo specifico e poi distrugge l'oggetto
    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}