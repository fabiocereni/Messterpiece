using UnityEngine;
using TMPro;

// Gestisce l'animazione del numero danno floating
// Move up + fade out + destroy
public class DamageNumber : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Velocità movimento verso l'alto")]
    public float moveSpeed = 2f;
    
    [Tooltip("Durata totale prima di destroy")]
    public float lifetime = 1.5f;
    
    [Tooltip("Quando inizia il fade (0-1, es. 0.5 = metà vita)")]
    public float fadeStartPercent = 0.5f;

    [Header("References")]
    public TextMeshProUGUI damageText;
    public TextMeshPro damageText3D;

    private float timer = 0f;
    private Color originalColor;
    private CanvasGroup canvasGroup;
    private Camera mainCamera;

    void Start()
    {
        // Get color originale
        if (damageText != null)
        {
            originalColor = damageText.color;
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
        else if (damageText3D != null)
        {
            originalColor = damageText3D.color;
        }

        // Cache camera reference (più efficiente che cercarla ogni frame)
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[DamageNumber] Camera.main not found! Assicurati che la camera del player abbia tag 'MainCamera'");
        }

        // Auto-destroy dopo lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move up
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Fade out nella seconda metà della vita
        float lifePercent = timer / lifetime;
        if (lifePercent > fadeStartPercent)
        {
            float fadePercent = (lifePercent - fadeStartPercent) / (1f - fadeStartPercent);
            float alpha = Mathf.Lerp(1f, 0f, fadePercent);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            else if (damageText != null)
            {
                Color color = damageText.color;
                color.a = alpha;
                damageText.color = color;
            }
            else if (damageText3D != null)
            {
                Color color = damageText3D.color;
                color.a = alpha;
                damageText3D.color = color;
            }
        }

        if (mainCamera != null)
        {
            transform.LookAt(mainCamera.transform);

            // Ruota di 180° sull'asse Y per evitare che il testo sia al contrario
            transform.Rotate(0, 180, 0);
        }
    }

    // Imposta il valore del danno da mostrare
    public void SetDamage(float damage)
    {
        string damageString = Mathf.RoundToInt(damage).ToString();

        if (damageText != null)
        {
            damageText.text = damageString;
        }
        else if (damageText3D != null)
        {
            damageText3D.text = damageString;
        }
    }
}
