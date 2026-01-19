using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestisce la transizione di scena quando il player entra nel trigger.
/// Usato per l'exit portal del tutorial.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "MainMenu";

    [Header("Tutorial Completion")]
    [SerializeField] private bool markTutorialAsCompleted = true;

    [Header("Optional UI")]
    [SerializeField] private GameObject transitionUI; // Opzionale: schermata "Tutorial Completato!"

    void Start()
    {
        // Assicurati che il collider sia impostato come trigger
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        // Salva che il tutorial è stato completato (PlayerPrefs)
        if (markTutorialAsCompleted)
        {
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
            Debug.Log("Tutorial completato! Salvato in PlayerPrefs.");
        }

        // Mostra UI di transizione (opzionale)
        if (transitionUI != null)
        {
            transitionUI.SetActive(true);
            // Potresti aggiungere un delay qui con Invoke
            Invoke(nameof(LoadScene), 2f);
        }
        else
        {
            LoadScene();
        }
    }

    void LoadScene()
    {
        // Carica la scena
        SceneManager.LoadScene(sceneToLoad);
    }
}
