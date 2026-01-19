using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controlla che il player abbia visitato tutte le piattaforme della sezione Jump.
/// Ogni piattaforma deve avere un PlatformTrigger child.
/// </summary>
public class PlatformChecker : MonoBehaviour
{
    [Header("Platforms")]
    [SerializeField] private List<GameObject> platforms = new List<GameObject>();

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private HashSet<int> visitedPlatforms = new HashSet<int>();
    private bool sectionCompleted = false;

    void Start()
    {
        // Aggiungi trigger alle piattaforme
        for (int i = 0; i < platforms.Count; i++)
        {
            GameObject platform = platforms[i];
            if (platform == null) continue;

            // Aggiungi o ottieni il componente PlatformTrigger
            PlatformTrigger trigger = platform.GetComponentInChildren<PlatformTrigger>();
            if (trigger == null)
            {
                // Crea un trigger child
                GameObject triggerObj = new GameObject("PlatformTrigger");
                triggerObj.transform.SetParent(platform.transform);
                triggerObj.transform.localPosition = Vector3.up * 0.5f; // Sopra la piattaforma

                BoxCollider col = triggerObj.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(3f, 1f, 3f); // Dimensione trigger

                trigger = triggerObj.AddComponent<PlatformTrigger>();
            }

            // Registra questo checker
            trigger.platformIndex = i;
            trigger.checker = this;
        }

        Debug.Log($"[PlatformChecker] {platforms.Count} piattaforme registrate");
    }

    /// <summary>
    /// Chiamato da PlatformTrigger quando il player visita una piattaforma
    /// </summary>
    public void OnPlatformVisited(int platformIndex)
    {
        if (sectionCompleted) return;

        if (!visitedPlatforms.Contains(platformIndex))
        {
            visitedPlatforms.Add(platformIndex);
            Debug.Log($"[PlatformChecker] Piattaforma {platformIndex + 1} visitata ({visitedPlatforms.Count}/{platforms.Count})");

            CheckCompletion();
        }
    }

    void CheckCompletion()
    {
        if (visitedPlatforms.Count >= platforms.Count)
        {
            CompleteSection();
        }
    }

    void CompleteSection()
    {
        if (sectionCompleted) return;

        sectionCompleted = true;
        Debug.Log("[PlatformChecker] Sezione Jump completata!");

        // Notifica il TutorialManager
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CompleteSection("Jump");
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo || sectionCompleted) return;

        GUILayout.BeginArea(new Rect(10, 100, 300, 100));
        GUILayout.Label("=== JUMP TRACKER ===");
        GUILayout.Label($"Platforms visited: {visitedPlatforms.Count}/{platforms.Count}");

        for (int i = 0; i < platforms.Count; i++)
        {
            string status = visitedPlatforms.Contains(i) ? "✓" : "✗";
            GUILayout.Label($"Platform {i + 1}: {status}");
        }
        GUILayout.EndArea();
    }
}

/// <summary>
/// Trigger da aggiungere ad ogni piattaforma per rilevare quando il player ci salta sopra.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PlatformTrigger : MonoBehaviour
{
    [HideInInspector] public int platformIndex;
    [HideInInspector] public PlatformChecker checker;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (checker != null)
            {
                checker.OnPlatformVisited(platformIndex);
            }
        }
    }
}
