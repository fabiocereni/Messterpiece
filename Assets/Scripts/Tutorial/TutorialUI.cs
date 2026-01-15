using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Gestisce l'interfaccia utente del tutorial: istruzioni, progresso, feedback.
/// </summary>
public class TutorialUI : MonoBehaviour
{
    [Header("Instruction Panel")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Progress Display")]
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Slider progressBar;

    [Header("Completion Feedback")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TextMeshProUGUI completionText;
    [SerializeField] private float completionDisplayDuration = 2f;

    [Header("Final Completion")]
    [SerializeField] private GameObject finalCompletionPanel;
    [SerializeField] private TextMeshProUGUI finalCompletionText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeSpeed = 2f;

    void Start()
    {
        // Nascondi i pannelli di completamento all'inizio
        if (completionPanel != null) completionPanel.SetActive(false);
        if (finalCompletionPanel != null) finalCompletionPanel.SetActive(false);

        // Mostra il pannello istruzioni
        if (instructionPanel != null) instructionPanel.SetActive(true);
    }

    /// <summary>
    /// Mostra le istruzioni per la sezione corrente
    /// </summary>
    public void ShowInstruction(string instruction)
    {
        if (instructionText != null)
        {
            instructionText.text = instruction;
        }

        if (instructionPanel != null && !instructionPanel.activeSelf)
        {
            instructionPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Aggiorna il progresso del tutorial
    /// </summary>
    public void UpdateProgress(int currentSection, int totalSections)
    {
        // Aggiorna testo
        if (progressText != null)
        {
            progressText.text = $"Progresso: {currentSection}/{totalSections}";
        }

        // Aggiorna barra
        if (progressBar != null)
        {
            float progress = totalSections > 0 ? (float)currentSection / totalSections : 0f;
            progressBar.value = progress;
        }
    }

    /// <summary>
    /// Mostra feedback quando una sezione viene completata
    /// </summary>
    public void ShowSectionComplete(string sectionName)
    {
        if (completionPanel == null) return;

        if (completionText != null)
        {
            completionText.text = $"✓ {sectionName} Completato!";
        }

        StartCoroutine(ShowCompletionFeedback());
    }

    IEnumerator ShowCompletionFeedback()
    {
        completionPanel.SetActive(true);

        // Animazione fade in (opzionale)
        CanvasGroup cg = completionPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.5f);
                yield return null;
            }
        }

        // Attendi
        yield return new WaitForSeconds(completionDisplayDuration);

        // Animazione fade out
        if (cg != null)
        {
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.5f);
                yield return null;
            }
        }

        completionPanel.SetActive(false);
    }

    /// <summary>
    /// Mostra il pannello di completamento finale
    /// </summary>
    public void ShowTutorialComplete()
    {
        // Nascondi istruzioni
        if (instructionPanel != null) instructionPanel.SetActive(false);

        // Mostra pannello finale
        if (finalCompletionPanel != null)
        {
            finalCompletionPanel.SetActive(true);

            if (finalCompletionText != null)
            {
                finalCompletionText.text = "Tutorial Completato!\n\nPuoi procedere al menu principale.";
            }
        }
    }

    /// <summary>
    /// Nascondi le istruzioni (utile quando il player ha capito)
    /// </summary>
    public void HideInstructions()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Mostra di nuovo le istruzioni
    /// </summary>
    public void ShowInstructions()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
        }
    }
}
