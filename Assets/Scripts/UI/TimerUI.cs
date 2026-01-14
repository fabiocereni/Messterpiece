using UnityEngine;
using TMPro;

/// <summary>
/// Semplice UI per mostrare il timer della partita
/// Da collegare al MatchManager esistente
/// </summary>
public class TimerUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    
    private MatchManager matchManager;
    
    void Start()
    {
        // Trova il MatchManager nella scena
        matchManager = FindObjectOfType<MatchManager>();
        
        if (matchManager == null)
        {
            Debug.LogError("TimerUI: MatchManager non trovato!");
            return;
        }
        
        if (timerText == null)
        {
            Debug.LogError("TimerUI: TimerText non assegnato!");
            return;
        }
    }
    
    void Update()
    {
        if (matchManager == null || timerText == null) return;
        
        // Mostra il tempo rimanente solo se la partita è attiva
        if (matchManager.IsMatchActive)
        {
            float tempoRimanente = matchManager.RemainingTime;
            
            // Converti in minuti e secondi
            int minuti = (int)(tempoRimanente / 60);
            int secondi = (int)(tempoRimanente % 60);
            
            // Mostra il tempo
            timerText.text = $"{minuti:00}:{secondi:00}";
            
            // Rosso se mancano meno di 10 secondi
            if (tempoRimanente <= 10f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
        else
        {
            timerText.text = "";
        }
    }
}