using UnityEngine;

/// <summary>
/// Zona che infligge danno al player quando entra.
/// Usata nella Health Demo section per mostrare il sistema di danno.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class DamageZone : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 30f;
    [SerializeField] private float damageInterval = 0f; // 0 = danno una volta sola
    [SerializeField] private bool continuousDamage = false; // Danno continuo mentre dentro

    [Header("Visual Feedback")]
    [SerializeField] private Material zoneMaterial = null; // Materiale custom assegnato manualmente
    [SerializeField] private Color zoneColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private bool showZone = true;

    private float lastDamageTime = 0f;
    private bool playerInside = false;
    private GameObject currentPlayer = null;

    void Start()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        // Rendi visibile la zona (opzionale)
        if (showZone)
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null)
            {
                mr = gameObject.AddComponent<MeshRenderer>();
                MeshFilter mf = gameObject.AddComponent<MeshFilter>();
                mf.mesh = CreateCubeMesh();
            }

            // Usa il materiale assegnato manualmente, se disponibile
            if (zoneMaterial != null)
            {
                mr.material = zoneMaterial;
            }
            else
            {
                // Fallback: crea materiale procedurale
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = zoneColor;
                mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                mr.material = mat;
            }
        }
    }

    void Update()
    {
        if (continuousDamage && playerInside && currentPlayer != null)
        {
            if (Time.time >= lastDamageTime + damageInterval)
            {
                ApplyDamage(currentPlayer);
                lastDamageTime = Time.time;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            currentPlayer = other.gameObject;

            // Applica danno immediato se non è continuo
            if (!continuousDamage)
            {
                ApplyDamage(other.gameObject);
            }
            else
            {
                lastDamageTime = Time.time;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            currentPlayer = null;
        }
    }

    void ApplyDamage(GameObject player)
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            // Usa il metodo TakeDamage che accetta hitPoint e attacker opzionali
            ph.TakeDamage(damageAmount, transform.position, gameObject);
            Debug.Log($"[DamageZone] Inflitto {damageAmount} danno al player");
            Destroy(this.gameObject); // Distruggi la zona dopo aver inflitto danno
        }
    }

    Mesh CreateCubeMesh()
    {
        return Resources.GetBuiltinResource<Mesh>("Cube.fbx");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = zoneColor;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
