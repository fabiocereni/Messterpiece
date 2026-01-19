using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Manager centrale del tutorial che coordina tutte le sezioni e la progressione.
/// Pattern Singleton per accesso globale dalle sezioni.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    #region Singleton
    public static TutorialManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    [Header("Tutorial Sections")]
    [SerializeField] private List<TutorialSection> sections = new List<TutorialSection>();

    [Header("UI Reference")]
    [SerializeField] private TutorialUI tutorialUI;

    [Header("Barriers (Progressive Unlock)")]
    [SerializeField] private GameObject movementBarrier;
    [SerializeField] private GameObject jumpBarrier;
    [SerializeField] private GameObject shootingBarrier;
    [SerializeField] private GameObject healthBarrier;

    [Header("Events")]
    public UnityEvent onTutorialComplete;

    // Stato interno
    private int currentSectionIndex = 0;
    private Dictionary<string, bool> completedSections = new Dictionary<string, bool>();

    void Start()
    {
        InitializeTutorial();
    }

    void InitializeTutorial()
    {
        // Inizializza il tracking delle sezioni
        foreach (var section in sections)
        {
            completedSections[section.sectionName] = false;
        }

        // Nascondi tutte le barriere tranne la prima
        if (movementBarrier != null) movementBarrier.SetActive(true);
        if (jumpBarrier != null) jumpBarrier.SetActive(true);
        if (shootingBarrier != null) shootingBarrier.SetActive(true);
        if (healthBarrier != null) healthBarrier.SetActive(true);

        // Avvia la prima sezione
        if (sections.Count > 0)
        {
            StartSection(0);
        }

        Debug.Log("[TutorialManager] Tutorial inizializzato con " + sections.Count + " sezioni");
    }

    void StartSection(int index)
    {
        if (index < 0 || index >= sections.Count) return;

        currentSectionIndex = index;
        TutorialSection section = sections[index];

        // Aggiorna UI
        if (tutorialUI != null)
        {
            tutorialUI.ShowInstruction(section.instructions);
            tutorialUI.UpdateProgress(index + 1, sections.Count);
        }

        // Attiva GameObject della sezione
        if (section.sectionObject != null)
        {
            section.sectionObject.SetActive(true);
        }

        Debug.Log($"[TutorialManager] Avviata sezione {index + 1}: {section.sectionName}");
    }

    /// <summary>
    /// Chiamato dalle sezioni quando vengono completate
    /// </summary>
    public void CompleteSection(string sectionName)
    {
        if (!completedSections.ContainsKey(sectionName))
        {
            Debug.LogWarning($"[TutorialManager] Sezione sconosciuta: {sectionName}");
            return;
        }

        if (completedSections[sectionName])
        {
            Debug.Log($"[TutorialManager] Sezione {sectionName} già completata");
            return;
        }

        completedSections[sectionName] = true;
        Debug.Log($"[TutorialManager] Sezione completata: {sectionName}");

        // Trova l'indice della sezione
        int sectionIndex = sections.FindIndex(s => s.sectionName == sectionName);

        // Sblocca la barriera corrispondente
        UnlockBarrier(sectionIndex);

        // Mostra feedback UI
        if (tutorialUI != null)
        {
            tutorialUI.ShowSectionComplete(sectionName);
        }

        // Passa alla sezione successiva
        if (sectionIndex < sections.Count - 1)
        {
            Invoke(nameof(MoveToNextSection), 1.5f);
        }
        else
        {
            // Tutorial completato
            Invoke(nameof(CompleteTutorial), 2f);
        }
    }

    void MoveToNextSection()
    {
        int nextIndex = currentSectionIndex + 1;
        if (nextIndex < sections.Count)
        {
            StartSection(nextIndex);
        }
    }

    void UnlockBarrier(int sectionIndex)
    {
        switch (sectionIndex)
        {
            case 0: // Movement completato -> sblocca Jump
                if (movementBarrier != null) movementBarrier.SetActive(false);
                Debug.Log("[TutorialManager] Barriera Movement rimossa");
                break;
            case 1: // Jump completato -> sblocca Shooting
                if (jumpBarrier != null) jumpBarrier.SetActive(false);
                Debug.Log("[TutorialManager] Barriera Jump rimossa");
                break;
            case 2: // Shooting completato -> sblocca Health
                if (shootingBarrier != null) shootingBarrier.SetActive(false);
                Debug.Log("[TutorialManager] Barriera Shooting rimossa");
                break;
            case 3: // Health completato -> sblocca Exit
                if (healthBarrier != null) healthBarrier.SetActive(false);
                Debug.Log("[TutorialManager] Barriera Health rimossa");
                break;
        }
    }

    void CompleteTutorial()
    {
        Debug.Log("[TutorialManager] Tutorial completato!");

        if (tutorialUI != null)
        {
            tutorialUI.ShowTutorialComplete();
        }

        onTutorialComplete?.Invoke();
    }

    /// <summary>
    /// Controlla se una sezione è completata
    /// </summary>
    public bool IsSectionCompleted(string sectionName)
    {
        return completedSections.ContainsKey(sectionName) && completedSections[sectionName];
    }

    /// <summary>
    /// Ottieni il progresso totale (percentuale)
    /// </summary>
    public float GetProgressPercentage()
    {
        int completed = 0;
        foreach (var kvp in completedSections)
        {
            if (kvp.Value) completed++;
        }
        return sections.Count > 0 ? (float)completed / sections.Count : 0f;
    }
}

/// <summary>
/// Struct per definire una sezione del tutorial nell'inspector
/// </summary>
[System.Serializable]
public class TutorialSection
{
    public string sectionName;
    [TextArea(2, 4)]
    public string instructions;
    public GameObject sectionObject; // GameObject contenitore della sezione (opzionale)
}
